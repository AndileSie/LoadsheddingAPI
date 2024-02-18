using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace loadshedding
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Write("Welcome.\n" +
                    "1. Check Allowance\n" +
                    "2. Check Todays Schedule\n" +
                    "3. Coming soon\n");
                await CheckAllowance();
                Console.ReadLine();
            }
            
            
        }
        public static async Task CheckAllowance()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Define the API endpoint URL
                    string apiUrl = "https://developer.sepush.co.za/business/2.0/api_allowance";
                    
                    client.DefaultRequestHeaders.Add("token", "2CFFCF5B-FFDD4CD8-B3EC867E-F5CECE38");

                    // Make a GET request to the API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Ensure the response is successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a JSON string
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Root message = JsonSerializer.Deserialize<Root>(responseBody);
                    if (message == null)
                    {
                        Console.WriteLine("Failed");

                    }
                    else
                    {
                        Console.WriteLine($"Used: {message.allowance.count}\nLimit: {message.allowance.limit}\nType: {message.allowance.type}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Handle any errors with the API request
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }public static async Task Today()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Define the API endpoint URL
                    string apiUrl = "https://developer.sepush.co.za/business/2.0/area?id=nelsonmandelabay-5-summerstranduptomarinehotelarea8";
                    //string apiUrl1 = "https://developer.sepush.co.za/business/2.0/areas_nearby?lat=-34.0010&lon=25.6715";
                    //var s = "nelsonmandelabay-5-summerstranduptomarinehotelarea8";
                    // Add your token key to the request headers
                    //string tokenKey = "YourTokenKeyHere";
                    client.DefaultRequestHeaders.Add("token", "2CFFCF5B-FFDD4CD8-B3EC867E-F5CECE38");

                    // Make a GET request to the API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Ensure the response is successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a JSON string
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Rootobject message = JsonSerializer.Deserialize<Rootobject>(responseBody);
                    if (message == null)
                    {
                        Console.WriteLine("Failed");

                    }
                    else
                    {
                        Console.WriteLine($"Load shedding for: {message.info.name} \n {message.info.region}\n Amount of Events: {message.events.Length}\n\n" +
                            $"");
                        int x = 1;
                        foreach (var item in message.events)
                        {
                            Console.WriteLine($"Event: {x}\nStage : {item.note}\n" +
                                $"Date: {item.start.ToString("dddd d MMMM yyyy")}\n" +
                                $"Start Time: {item.start.ToString("t")}\n\n" +
                                $"End Time: {item.end.ToString("t")}\n\n");
                            x++;
                        }


                        //Console.WriteLine($"Source: {message.schedule.source}");

                        //Console.WriteLine(responseBody);



                    }
                    // Output the JSON response
                    //Console.WriteLine(responseBody);
                }
                catch (HttpRequestException ex)
                {
                    // Handle any errors with the API request
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
