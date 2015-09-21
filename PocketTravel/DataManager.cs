using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PocketTravel
{
    class DataManager
    {
        // all states of this data manager
        public enum State {LOADING_AIRPORT, ERROR, CONTSTRUCTING_DATA, READY, LOADING_DEST}

        // the current state of this manager
        public State _currentState;

        // the accessor of the state
        public State currentState {
            get { return _currentState; }
            set { _currentState = value; }
        }

        // list of airports across the world
        private List<AirportObject.Airport> airports;
        // tree of airport names, each node point to the index of airport object in the list
        private PatriciaTrie<AirportObject.Airport> airportTree;
        private PatriciaTrie<AirportObject.Airport> cityTree;
        private PatriciaTrie<AirportObject.Airport> countryTree;

        /**
         * Get All the airports from the API
         */
        public async Task getAirports()
        {
            try
            {
                // curl -v  -X GET "https://airport.api.aero/airport?user_key=be11346f5ed9869ab0f054037a9bd19a"
                // create HttpClient
                HttpClient client = new HttpClient();
                //set request headers to accept JSON data
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // call the airport API (developer.aero)
                string response = await client.GetStringAsync(new Uri("https://airport.api.aero/airport?user_key=be11346f5ed9869ab0f054037a9bd19a")).ConfigureAwait(continueOnCapturedContext: false);
                // create root object to store the response
                AirportObject.RootObject rootObject;
                // deserialize the JSON object response, the information will become an AirportObject.RootObject instance
                rootObject = JsonConvert.DeserializeObject<AirportObject.RootObject>(response);
                // return the list of all airports from rootObject
                airports = rootObject.airports;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                // set the error state
                currentState = State.ERROR;
            }
        }

        /**
         * Construct airportTree, cityTree, and countryTree
         * REQUIREMENT: 
         * 1. The list of airports should be constructed first
         */
        public void constructAirportTrees()
        {
            // if error occured when loading the airports
            // skip this step
            if (_currentState == State.ERROR) return;

            // initialize the fields
            airportTree = new PatriciaTrie<AirportObject.Airport>();
            cityTree = new PatriciaTrie<AirportObject.Airport>();
            countryTree = new PatriciaTrie<AirportObject.Airport>();

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

            // set the final state
            currentState = State.READY;
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
            foreach (AirportObject.Airport a in airports)
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

        public async Task<TravelInfo> getDestInfo(string originName, string destName)
        {
            AirportObject.Airport originAirport = findAirport(originName);
            AirportObject.Airport destAirport = findAirport(destName);
            if (originAirport != null && destAirport != null)
            {
                // set the state
                currentState = State.LOADING_DEST;
                // get information of the destination
                string city = destAirport.city;
                string country = destAirport.country;
                string coordinate = destAirport.lat + ", " + destAirport.lng;
                string timezone = destAirport.timezone;

                TimeObject.RootObject originTimeObject = await getTime(originAirport.lat, originAirport.lng);
                TimeObject.RootObject destTimeObject = await getTime(destAirport.lat, destAirport.lng);

                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

                DateTime originTime = new DateTime();
                DateTime destTime = new DateTime();

                if (originTimeObject != null)
                {
                    originTime = dtDateTime.AddSeconds(originTimeObject.timestamp).ToUniversalTime();
                }
                if (destName != null)
                {
                    destTime = dtDateTime.AddSeconds(destTimeObject.timestamp).ToUniversalTime();
                }

                WeatherObject.RootObject weather = await getWeather(destAirport.city);

                double temp = double.NaN;
                string weatherGroup = "not available";

                if (weather != null) {
                    // convert temperature from kelvin to celcius
                    temp = weather.main.temp - 273.15;
                    weatherGroup = weather.weather[0].description;
                }

                // set the final state
                currentState = State.READY;
                return new TravelInfo(city, country, coordinate, timezone, originTime, destTime, weatherGroup, temp);
            }
            return null;
        }

        public AirportObject.Airport findAirport(string name)
        {
            string[] names = name.Split(',');
            IEnumerable<AirportObject.Airport> airports = airportTree.Retrieve(names[0].ToLower());
            foreach (AirportObject.Airport a in airports)
            {
                return a;
            }
            return null;
        }

        public async Task<WeatherObject.RootObject> getWeather(string city)
        {
            try
            {
                // Initializing HTTPClient.
                HttpClient client = new HttpClient();
                //set request headers to accept JSON data
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // Creating a new Weather Object to bind the results from our API call.
                WeatherObject.RootObject rootObject;
                // Calling our weather API, passing the string 'city' so we're getting the correct weather returned.
                // The 'await' tag tells the computer to wait for the results to be returned before continuing with
                // the rest of the code. The results are then assigned to string 'x' to be used later in the code.
                string x = await client.GetStringAsync(new Uri("http://api.openweathermap.org/data/2.5/weather?q=" + city)).ConfigureAwait(continueOnCapturedContext: false);

                // We're now binding the returned data assigned to string 'x' to the object 'rootObject'.
                rootObject = JsonConvert.DeserializeObject<WeatherObject.RootObject>(x);

                return rootObject;
            }

            // Only called if an error occurs.
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public async Task<TimeObject.RootObject> getTime(double lat, double lng)
        {
            try
            {
                // Initializing HTTPClient.
                HttpClient client = new HttpClient();
                //set request headers to accept JSON data
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // Creating a new Weather Object to bind the results from our API call.
                TimeObject.RootObject rootObject;
                // Calling our weather API, passing the string 'city' so we're getting the correct weather returned.
                // The 'await' tag tells the computer to wait for the results to be returned before continuing with
                // the rest of the code. The results are then assigned to string 'x' to be used later in the code.
                string x = await client.GetStringAsync(new Uri("http://api.timezonedb.com/?lat="+lat+"&lng="+lng+"&format=json&key=59RQWA7VYKTV")).ConfigureAwait(continueOnCapturedContext: false);

                // We're now binding the returned data assigned to string 'x' to the object 'rootObject'.
                rootObject = JsonConvert.DeserializeObject<TimeObject.RootObject>(x);

                return rootObject;
            }

            // Only called if an error occurs.
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }
    }
}
