using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketTravelTest
{
    class AirportObject
    {
        public class Airport
        {
            public string code { get; set; }
            public string name { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public double lat { get; set; }
            public double lng { get; set; }
            public string timezone { get; set; }
            public override string ToString()
            {
                return "[Name: " + name + ", Code: " + code + ", City: " + city + ", Country: " + country + ", Lat: " + lat + ", Lng: " + lng + ", Timezone: " + timezone + "]";
            }
            public string getListedName()
            {
                return name + ", " + city + ", " + country;
            }
        }

        public class RootObject
        {
            public int processingDurationMillis { get; set; }
            public bool authorisedAPI { get; set; }
            public bool success { get; set; }
            public List<Airport> airports { get; set; }
        }
    }
}
