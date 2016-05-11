using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;


namespace EventZ.API.Controllers
{
    [Route("api/[controller]")]
    public class EventzController : Controller
    {
        private IMongoDatabase db;

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
        public string Get(int id)
        {
            return "value";
        }

        public class EventZEvent
        {
            public BsonObjectId _id { get; set; }

            public string Title { get; set; }

            public string Organizer { get; set; }

            public DateTime StartsAt { get; set; }

            public DateTime EndsAt { get; set; }

            public string Description { get; set; }

            public List<InvitedPerson> Invitees { get; set; } = new List<InvitedPerson>();
        }

        public class InvitedPerson
        {
            public string Name { get; set; }

            public int Accepted { get; set; }
        }
    }
}
