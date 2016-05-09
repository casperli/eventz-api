using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoEventGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const string uname = "test";
            const string pwd = "test";

            Console.WriteLine("Start MongoDB connection");
            Console.ReadLine();

            var connectionString = $"mongodb://{uname}:{pwd}@ds032579.mlab.com:32579/mLabMongoDB-4";

            var client = new MongoClient(connectionString);
            var x = client.GetDatabase("mLabMongoDB-4");

            Console.WriteLine($"Mongo DB connection successfull. DB Namespace:{x.DatabaseNamespace}");
            Console.ReadLine();
        }

        public class EventZEvent
        {
            public string Title { get; set; }

            public string Organizer { get; set; }

            public DateTime StartsAt { get; set; }

            public DateTime EndsAt { get; set; }

            public string Description { get; set; }

            public InvitedPerson[] Invitees { get; set; }
        }

        public class InvitedPerson
        {
            public string Name { get; set; }

            public int Accepted { get; set; }
        }
    }
}
