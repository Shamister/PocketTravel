using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketTravel
{
    class TimeObject
    {
        public class RootObject
        {
            public string status { get; set; }
            public string message { get; set; }
            public string countryCode { get; set; }
            public string zoneName { get; set; }
            public string abbreviation { get; set; }
            public string gmtOffset { get; set; }
            public string dst { get; set; }
            public int timestamp { get; set; }
        }
    }
}
