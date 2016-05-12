using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parse;

namespace EventZ.API.Controllers
{
    [Route("api/[controller]")]
    public class EventzController : Controller
    {
        private const string parseAddress = "https://eventz-parse.azurewebsites.net/parse/";
        private static readonly string AppId = "dsXH3syuEuuIZvXi1niEtX49LLil50JK5oIBcLM3";

        private readonly IMongoDatabase db;

        public EventzController()
        {
            const string uname = "test";
            const string pwd = "test";

            var connectionString = $"mongodb://{uname}:{pwd}@ds032579.mlab.com:32579/mLabMongoDB-4";

            var client = new MongoClient(connectionString);
            this.db = client.GetDatabase("mLabMongoDB-4");

            ParseClient.Initialize(new ParseClient.Configuration { ApplicationId = AppId, Server = parseAddress });
        }

        private IMongoCollection<WriteEvent> EventCollection => this.db.GetCollection<WriteEvent>("EventZ_Write");
        
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]WriteEvent ivent)
        {
            var eventCollection = this.EventCollection;
            await eventCollection.InsertOneAsync(ivent);

            await this.UpdateReadStore(ivent);

            return this.Ok();
        }

        [HttpPost("{id}/register/")]
        public async Task<ActionResult> RegisterEvent(string id, [FromBody]RegisterNameDto name)
        {
            var idx = new ObjectId(id);
            var eventCollection = this.EventCollection;

            var filter = Builders<WriteEvent>.Filter.Eq(e => e.Id, idx);
            var result = await eventCollection.Find(filter).FirstOrDefaultAsync();

            if (result.Invitees.Exists(i => i.Name.Equals(name.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return this.Ok();
            }

            result.Invitees.Add(new InvitedPerson { Accepted = 1, Name = name.Name });
            var update = Builders<WriteEvent>.Update.Set(e => e.Invitees, result.Invitees);

            eventCollection.FindOneAndUpdate(filter, update);

            await UpdateReadStore(result);

            return this.Ok();
        }

        [HttpPost("{id}/deregister/")]
        public async Task<ActionResult> DeRegisterEvent(string id, [FromBody]RegisterNameDto name)
        {
            var idx = new ObjectId(id);
            var eventCollection = this.EventCollection;

            var filter = Builders<WriteEvent>.Filter.Eq(e => e.Id, idx);
            var result = await eventCollection.Find(filter).FirstOrDefaultAsync();

            result.Invitees = result.Invitees.FindAll(i => i.Name != name.Name).ToList();

            var update = Builders<WriteEvent>.Update.Set(e => e.Invitees, result.Invitees);

            eventCollection.FindOneAndUpdate(filter, update);

            await UpdateReadStore(result);

            return this.Ok();
        }

        private async Task UpdateReadStore(WriteEvent wevent)
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery("EventZ");
            ParseObject ivent = await query.WhereEqualTo("uid", wevent.Id.ToString()).FirstOrDefaultAsync();

            var readId = ivent?.ObjectId;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var readEvent = new ReadEvent
            {
                Uid = wevent.Id.ToString(),
                Title = wevent.Title,
                Description = wevent.Description,
                Image = wevent.Image,
                Invitees = wevent.Invitees.Select(i => i.Name).ToList(),
                StartsAt = wevent.StartsAt,
                EndsAt = wevent.EndsAt,
                Organizer = wevent.Organizer
            };

            var toStore = JsonConvert.SerializeObject(readEvent, settings);

            using (var client = new HttpClient())
            {
                var baseUri = $"{parseAddress}classes/EventZ/{readId}";
                client.BaseAddress = new Uri(baseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Parse-Application-Id", AppId);
                //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                if (ivent == null)
                {
                    await client.PostAsync(baseUri, new StringContent(toStore, Encoding.UTF8, "application/json"));
                }
                else
                {
                    await client.PutAsync(baseUri, new StringContent(toStore, Encoding.UTF8, "application/json"));
                }
            }
        }


        public class ReadEvent
        {
            /// <summary>
            /// This is the Parse ID
            /// </summary>
            [BsonElement("id")]
            [JsonIgnore]
            public string Id { get; set; }

            [BsonElement("uid")]
            public string Uid { get; set; }

            [BsonElement("title")]
            public string Title { get; set; }

            [BsonElement("organizer")]
            public string Organizer { get; set; }

            [BsonElement("startsAt")]
            public DateTime StartsAt { get; set; }

            [BsonElement("endsAt")]
            public DateTime EndsAt { get; set; }

            [BsonElement("description")]
            public string Description { get; set; }

            [BsonElement("image")]
            public string Image { get; set; }

            [BsonElement("invitees")]
            public List<string> Invitees { get; set; } = new List<string>();
        }

        [BsonIgnoreExtraElements]
        public class WriteEvent
        {
            [BsonId]
            //[BsonElement("id")]
            public ObjectId Id { get; set; }

            [BsonElement("title")]
            public string Title { get; set; }

            [BsonElement("organizer")]
            public string Organizer { get; set; }

            [BsonElement("startsAt")]
            public DateTime StartsAt { get; set; }

            [BsonElement("endsAt")]
            public DateTime EndsAt { get; set; }

            [BsonElement("description")]
            public string Description { get; set; }

            [BsonElement("image")]
            public string Image { get; set; }

            [BsonElement("invitees")]
            public List<InvitedPerson> Invitees { get; set; } = new List<InvitedPerson>();
        }

        [BsonIgnoreExtraElements]
        public class InvitedPerson
        {
            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("accepted")]
            public int Accepted { get; set; }
        }

        public class RegisterNameDto
        {
            public string Name { get; set; }
        }
    }
}
