using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding
{
    public class ResponseClass1
    {

    }
    public class Rootobject
    {
        public Event[] events { get; set; }
        public Info info { get; set; }
        public Schedule schedule { get; set; }
    }

    public class Info
    {
        public string name { get; set; }
        public string region { get; set; }
    }

    public class Schedule
    {
        public Day[] days { get; set; }
        public string source { get; set; }
    }

    public class Day
    {
        public string date { get; set; }
        public string name { get; set; }
        public string[][] stages { get; set; }
    }

    public class Event
    {
        public DateTime end { get; set; }
        public string note { get; set; }
        public DateTime start { get; set; }
    }
    public class Allowance
    {
        public int count { get; set; }
        public int limit { get; set; }
        public string type { get; set; }
    }

    public class Root
    {
        public Allowance allowance { get; set; }
    }

    public class StatusClass
    {
        public Status status { get; set; }
    }

    public class Status
    {
        public Capetown capetown { get; set; }
        public Eskom eskom { get; set; }
    }

    public class Capetown
    {
        public string name { get; set; }
        public Next_Stages[] next_stages { get; set; }
        public string stage { get; set; }
        public DateTime stage_updated { get; set; }
    }

    public class Next_Stages
    {
        public string stage { get; set; }
        public DateTime stage_start_timestamp { get; set; }
    }

    public class Eskom
    {
        public string name { get; set; }
        public Next_Stages1[] next_stages { get; set; }
        public string stage { get; set; }
        public DateTime stage_updated { get; set; }
    }

    public class Next_Stages1
    {
        public string stage { get; set; }
        public DateTime stage_start_timestamp { get; set; }
    }




}
