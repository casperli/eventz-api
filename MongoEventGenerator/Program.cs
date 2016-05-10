using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoEventGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const string uname = "test";
            const string pwd = "test";

            var connectionString = $"mongodb://{uname}:{pwd}@ds032579.mlab.com:32579/mLabMongoDB-4";

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase("mLabMongoDB-4");

            var eventCollection = db.GetCollection<EventZEvent>("EventList");

            ShowInitDialog(db.DatabaseNamespace.DatabaseName);

            var command = Console.ReadLine();

            while (!string.Equals(command, "x", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Create a new Event:");
                var eventItem = new EventZEvent();

                Console.Write("Title:");
                eventItem.Title = Console.ReadLine();

                Console.Write("Description:");
                eventItem.Description = Console.ReadLine();

                Console.Write("Organizer:");
                eventItem.Organizer = Console.ReadLine();

                eventItem.StartsAt = DateTime.Today.AddHours(new Random().Next(500));
                Console.WriteLine($"Start: {eventItem.StartsAt}");

                eventItem.EndsAt = eventItem.StartsAt.AddMinutes(new Random().Next(500));
                Console.WriteLine($"End: {eventItem.EndsAt}");

                Console.Write("Enter participants (comma separated):");
                foreach (var participant in Console.ReadLine().Split(','))
                {
                    eventItem.Invitees.Add(new InvitedPerson { Accepted = 0, Name = participant });
                }

                eventCollection.InsertOne(eventItem);

                Console.WriteLine("Event created successfully.");
                Thread.Sleep(1500);
                Console.Clear();

                ShowInitDialog(db.DatabaseNamespace.DatabaseName);

                command = Console.ReadLine();
            }

            Console.WriteLine("Bye!");
            Console.ReadLine();
        }

        private static void ShowInitDialog(string db)
        {
            Console.WriteLine("********************************************************************");
            Console.WriteLine("* Casper.li's EventZ Console - Adding/Showing eventz               *");
            Console.WriteLine($"* Connected to {db}");
            Console.WriteLine("********************************************************************");
            Console.WriteLine("* Options:                                                         *");
            Console.WriteLine("* c - create a new event                                           *");
            Console.WriteLine("* x - exit console                                                 *");
            Console.WriteLine("********************************************************************");
            Console.WriteLine("* (c) 2016 by Web.NET Camp                                         *");
            Console.WriteLine("********************************************************************");
        }
    }

    public class EventZEvent
    {
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
