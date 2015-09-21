using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketTravel
{
    class TravelInfo
    {
        // destination city name
        public string city { get; set; }
        // destination country name
        public string country { get; set; }
        // coordinate of the airport
        public string coordinate { get; set; }
        // timezone of the city
        public string timezone { get; set; }
        // current time of the origin
        public DateTime originTime { get; set; }
        // current time of the destination
        public DateTime destTime { get; set; }
        // weather of the city
        public string weather { get; set; }
        // temperature of the city
        public double temp { get; set; }

        public TravelInfo(string city, string country, string coordinate, string timezone, DateTime originTime, DateTime destTime, string weather, double temp)
        {
            this.city = city;
            this.country = country;
            this.coordinate = coordinate;
            this.timezone = timezone;
            this.originTime = originTime;
            this.destTime = destTime;
            this.weather = weather;
            this.temp = temp;
        }
    }
}
