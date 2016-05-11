using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace EventZ.API.Controllers
{
    [Route("api/[controller]")]
    public class EventzController : Controller
    {
        private readonly IMongoDatabase db;

        public EventzController()
        {
            const string uname = "test";
            const string pwd = "test";

            var connectionString = $"mongodb://{uname}:{pwd}@ds032579.mlab.com:32579/mLabMongoDB-4";

            var client = new MongoClient(connectionString);
            this.db = client.GetDatabase("mLabMongoDB-4");
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<EventZEvent> Get()
        {
            var eventCollection = db.GetCollection<EventZEvent>("EventList");
            var list = eventCollection.AsQueryable().Take(100).ToList();

            foreach (var ivent in list)
            {
                ivent.Image = "http://lorempixel.com/100/100";
            }

            return list;

            //return new EventZEvent[] {new EventZEvent
            //{
            //    Description = "Test desc",
            //    Title = "Test Title",
            //    Organizer = "Organizer",
            //    StartsAt = DateTime.Today,
            //    EndsAt = DateTime.Today.AddHours(1),
            //    Invitees = new List<InvitedPerson> {new InvitedPerson { Accepted = 0, Name = "Mark"} }
            //} };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            var idx = new ObjectId(id);
            var eventCollection = db.GetCollection<EventZEvent>("EventList");

            var filter = Builders<EventZEvent>.Filter.Eq(e=>e.Id,idx);
            var result = await eventCollection.Find(filter).FirstOrDefaultAsync();

            return this.Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]EventZEvent ivent)
        {
            var eventCollection = db.GetCollection<EventZEvent>("EventList");
            await eventCollection.InsertOneAsync(ivent);

            return this.Ok();
        }

        [HttpPost("{id}/register/")]
        public async Task<ActionResult> RegisterEvent(string id, [FromBody]RegisterNameDto name)
        {
            var idx = new ObjectId(id);
            var eventCollection = db.GetCollection<EventZEvent>("EventList");

            var filter = Builders<EventZEvent>.Filter.Eq(e => e.Id, idx);
            var result = await eventCollection.Find(filter).FirstOrDefaultAsync();

            result.Invitees.Add(new InvitedPerson { Accepted = 1, Name = name.Name });


            var update = Builders<EventZEvent>.Update.Set(e => e.Invitees, result.Invitees);

            eventCollection.FindOneAndUpdate(filter, update);

            return this.Ok();
        }

        public class EventZEvent
        {
            [BsonId]
            public ObjectId Id { get; set; }

            public string Title { get; set; }

            public string Organizer { get; set; }

            public DateTime StartsAt { get; set; }

            public DateTime EndsAt { get; set; }

            public string Description { get; set; }

            public string Image { get; set; }

            public List<InvitedPerson> Invitees { get; set; } = new List<InvitedPerson>();
        }

        public class InvitedPerson
        {
            public string Name { get; set; }

            public int Accepted { get; set; }
        }

        public class RegisterNameDto
        {
            public string Name { get; set; }
        }
    }
}
