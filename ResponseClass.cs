using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding
{
    public class ResponseClass
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Day
        {
            public string date { get; set; }
            public string name { get; set; }
            public List<List<string>> stages { get; set; }
        }

        public class Event
        {
            public DateTime end { get; set; }
            public string note { get; set; }
            public DateTime start { get; set; }
        }

        public class Info
        {
            public string name { get; set; }
            public string region { get; set; }
        }

        public class Root
        {
            public List<Event> events { get; set; }
            public Info info { get; set; }
            public Schedule schedule { get; set; }
        }

        public class Schedule
        {
            public List<Day> days { get; set; }
            public string source { get; set; }
        }


    }
}
