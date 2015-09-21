using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace PocketTravelTest
{
    class Program
    {
        // list of airports across the world
        private List<AirportObject.Airport> airports;
        // tree of airport names, each node point to the index of airport object in the list
        private PatriciaSuffixTrie<AirportObject.Airport> airportTree;
        private PatriciaSuffixTrie<AirportObject.Airport> cityTree;
        private PatriciaSuffixTrie<AirportObject.Airport> countryTree;
        

        public Program()
        {

            test();
        }

        public async void test()
        {
            Console.WriteLine("Requesting list of all airports...");
            // get all the airports
            getAirports();

            while (airports == null)
            {
                //wait
            }

            Console.WriteLine("Construct the data...");
            constructAirportTrees();
            Console.WriteLine("Data received.");
            /**
            Console.WriteLine("Print all the airports :");
            // show all the airports
            foreach (AirportObject.Airport a in airports)
            {
                Console.WriteLine(a);
            }
            */

            Console.WriteLine("Get completion with substring \'soekar\'...");
            HashSet<AirportObject.Airport> airportObjects = getAutoCompleteAirport("soekar");
            foreach (AirportObject.Airport a in airportObjects)
            {
                Console.WriteLine(a.getListedName());
                Console.WriteLine("Get the city information of the airport...");
                WeatherObject.RootObject weather = await getWeather(a.city);

                Console.WriteLine("City :" + a.city);
                Console.WriteLine("Lon :" + weather.coord.lon);
                Console.WriteLine("Lat :" + weather.coord.lat);
                Console.WriteLine(":" + weather.weather[0].main);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("THIS IS TEST");
            new Program();
            Console.ReadLine();
        }

        /**
         * Get All the airports from the API
         */
        public async void getAirports()
        {
            try {
                // curl -v  -X GET "https://airport.api.aero/airport?user_key=be11346f5ed9869ab0f054037a9bd19a"
                // create HttpClient
                HttpClient client = new HttpClient();
                //set request headers to accept JSON data
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // call the airport API (developer.aero)
                string response = await client.GetStringAsync(new Uri("https://airport.api.aero/airport?user_key=be11346f5ed9869ab0f054037a9bd19a"));
                // create root object to store the response
                AirportObject.RootObject rootObject;
                // deserialize the JSON object response, the information will become an AirportObject.RootObject instance
                rootObject = JsonConvert.DeserializeObject<AirportObject.RootObject>(response);
                // store the list of all airports from rootObject
                airports = rootObject.airports;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /**
         * Construct airportTree, cityTree, and countryTree
         * REQUIREMENT: 
         * 1. The list of airports should be constructed first
         */
        public void constructAirportTrees()
        {
            // initialize the fields
            airportTree = new PatriciaSuffixTrie<AirportObject.Airport>(1);
            cityTree = new PatriciaSuffixTrie<AirportObject.Airport>(1);
            countryTree = new PatriciaSuffixTrie<AirportObject.Airport>(1);

            for (int i = 0; i < airports.Count; i++)
            {
                AirportObject.Airport a = airports[i];

                // skip it the airport name is null
                if (a.name == null) continue;

                // insert airport name, city, and country to each tree
                airportTree.Add(a.name.ToLower(), a);
                cityTree.Add(a.city.ToLower(), a);
                countryTree.Add(a.country.ToLower(), a);
            }
        }

        /**
         * Get the completion of the substring specified
         * The input text can be the subtring of either airport name, city, or country
         * Retrieve a set of all the possible airports that contain the completion text
         */
        public HashSet<AirportObject.Airport> getAutoCompleteAirport(string substring)
        {
            // a set of all possible airports that will be returned
            HashSet<AirportObject.Airport> completeAirports = new HashSet<AirportObject.Airport>();
            // retrieve all possible airport names that contain the substring
            IEnumerable<AirportObject.Airport> airports = airportTree.Retrieve(substring);
            // retrieve all possible cities that contain the substring
            IEnumerable<AirportObject.Airport> cities = cityTree.Retrieve(substring);
            // retrieve all possible countries that contain the substring
            IEnumerable<AirportObject.Airport> countries = countryTree.Retrieve(substring);

            // Put all the possible completion to the set
            foreach(AirportObject.Airport a in airports)
            {
                completeAirports.Add(a);
            }
            foreach (AirportObject.Airport a in cities)
            {
                completeAirports.Add(a);
            }
            foreach (AirportObject.Airport a in countries)
            {
                completeAirports.Add(a);
            }

            // return the set of all possible airports
            return completeAirports;
        }

        public async Task<WeatherObject.RootObject> getWeather(string city)
        {
            try
            {
                // Initializing HTTPClient.
                HttpClient client = new HttpClient();

                // Creating a new Weather Object to bind the results from our API call.
                WeatherObject.RootObject rootObject;

                // Calling our weather API, passing the string 'city' so we're getting the correct weather returned.
                // The 'await' tag tells the computer to wait for the results to be returned before continuing with
                // the rest of the code. The results are then assigned to string 'x' to be used later in the code.
                string x = await client.GetStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q=" + city));

                // We're now binding the returned data assigned to string 'x' to the object 'rootObject'.
                rootObject = JsonConvert.DeserializeObject<WeatherObject.RootObject>(x);

                return rootObject;
            }

            // Only called if an error occurs.
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }
    }
}
