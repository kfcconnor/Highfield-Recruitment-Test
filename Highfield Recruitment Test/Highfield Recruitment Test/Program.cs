using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Highfield_Recruitment_Test
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class apiCalls
    {
        static readonly HttpClient client = new HttpClient();
        public static readonly string path = "https://recruitment.highfieldqualifications.com/api/test";
        //path used for getting and posting data
        static async Task<string> getUserJson(string path)
        {
            string userJson = "";
            HttpResponseMessage response = await client.GetAsync(path);
            //code to get json from server
            if (response.IsSuccessStatusCode)
            {
                userJson = await response.Content.ReadAsStringAsync();
            }
            return userJson;
        }

        public static async Task<string> postResponse(Response respToSend)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
      
            HttpResponseMessage response = await client.PostAsJsonAsync(path, respToSend);
            //sends converst response class to json and sends it to server
            string contents = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            //ensures success of the send or posts an exception
            string returningString = response.StatusCode.ToString() + " API return: " + contents;
            //returns status code as well as the contents the API returns.
            return returningString;
        }

        public static async Task<List<userEntity>> updateUserData()
        {
            
            jsonFunctions jsonHandler = new jsonFunctions();


            string jsonString;
            jsonString = await getUserJson(path);
            List<userEntity> users = jsonHandler.getUsersFromJson(jsonString);
            //calls class to convert the json string into a list of classes that can be used within c#
            return users;
        }

    }

    public class jsonFunctions
    {
        public List<userEntity> getUsersFromJson(string jsonString)
        {
            List<userEntity> userToReturn = JsonSerializer.Deserialize<List<userEntity>>(jsonString);
            //deserializes the json into a list of classes
            return userToReturn;
        }

        public string returnDataToJson(List<userEntity> users, List<AgePlusTwenty> ages, List<TopColours> topColours)
        {
            Response resp = new Response(users, ages, topColours);
            string jsonString = JsonSerializer.Serialize(resp);
            //unused in final program but converts all the classes into a response json object to send back. Was used in testing
            return jsonString;
        }
    }

    public class dataProcessingFunctions
    {
        public int getAge(string dob)
        {
            int age = 0;
            DateTime currentTime = DateTime.Now;
            int year = int.Parse(dob.Substring(0, 4));
            int month = int.Parse(dob.Substring(5, 2));
            int day = int.Parse(dob.Substring(8, 2));
            DateTime dobD = new DateTime(year, month, day);
            decimal daysDiff = ((TimeSpan)(currentTime - dobD)).Days;
            age = Decimal.ToInt32(Math.Floor(daysDiff / 365.25M));
            //converts the test string into a date and conpares it against todays date to give a number of days between the two days, these are then devided by 365 and a quarter to give an accurate age in years.
            return age;
        }
        
        public TopColours newColour(string colour)
        {
            TopColours colourFreq = new TopColours();
            colourFreq.colour = colour;
            colourFreq.count = 1;
            //this code adds a new entry into the Colour list when one comes up to ensure that this code doesn't have to be repeated when needed.
            return colourFreq;
        }

        public (List<AgePlusTwenty>, List<TopColours>) processTable(List<userEntity> users)
        {
            //this class goes through each item on the list tallying up the colours as well as working out the age for each entry as it does so. This ensures that only one loop is needed through the data for it to be processed.
            List<AgePlusTwenty> aPT = new List<AgePlusTwenty>();
            AgePlusTwenty cA;
            List<TopColours> tC = new List<TopColours>();
            bool cFound = false;
            foreach(userEntity u in users)
            {
                cA = new AgePlusTwenty();
                cA.userId = u.id;
                cA.originalAge = getAge(u.dob);
                cA.agePlusTwenty = cA.originalAge + 20;
                aPT.Add(cA);
                if (tC.Count == 0)
                {
                    tC.Add(newColour(u.favouriteColour));
                }
                else
                {
                    foreach(TopColours c in tC)
                    {
                        if (c.colour == u.favouriteColour)
                        {
                            c.count++;
                            cFound = true;
                            break;
                        }
                    }

                    if (!cFound)
                    {
                       tC.Add(newColour(u.favouriteColour));
                    }
                    cFound = false;
                }
            }
            tC = tC.OrderByDescending(colour => colour.count).ThenBy(colour => colour.colour).ToList();
            //this code updates the colour list so that it is in order by number of times a colour appears and then alphabetically
            return (aPT,tC);
        }

    }


    public class userEntity
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string dob { get; set; }
        public string favouriteColour { get; set; }
    }

    public class AgePlusTwenty
    {
        public string userId { get; set; }
        public int originalAge { get; set; }
        public int agePlusTwenty { get; set; }
    }

    public class TopColours
    {
        public string colour { get; set; }
        public int count { get; set; }
    }

    public class Response
    {
        public List<userEntity> users { get; set; }
        public List<AgePlusTwenty> ages { get; set; }
        public List<TopColours> topColours { get; set; }

        public Response(List<userEntity> u, List<AgePlusTwenty> a, List<TopColours> c)
        {
            users = u;
            ages = a;
            topColours = c;
        }
    }

}
