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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Welcome.\n" +
                    "1. Check Allowance\n" +
                    "2. Check Todays Schedule\n" +
                    "3. Load shedding stages\n" +
                    "Choice: ");
                int option = int.Parse(Console.ReadLine());
                switch (option)
                {
                    case 1:
                        await CheckAllowance();
                    break;
                    case 2:
                        await Today();
                    break;
                    case 3:
                        await CheckStage();
                    break;
                    default:
                        Console.WriteLine("Invalid Selection");
                        break;
                }
                //await Today();
                //Console.ReadLine();
            }
            
            
        }
        public static async Task CheckStage()
        {
            
            string responseBody = await GetData("https://developer.sepush.co.za/business/2.0/status");
            StatusClass message = JsonSerializer.Deserialize<StatusClass>(responseBody);
            if (message == null)
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine($"Current And Future Stages\n" +
                    $"Current Stage: {message.status.eskom.stage} Last Updated: {message.status.eskom.stage_updated.ToShortDateString()} {message.status.eskom.stage_updated.ToShortTimeString()}\n");

                Console.WriteLine("\nFuture stages implantations\n");
                if (message.status.eskom.next_stages.Length == 0)
                {
                    Console.WriteLine("No future updates");

                }
                else
                {

                    foreach (var item in message.status.eskom.next_stages)
                    {

                        Console.WriteLine($"Stage: {item.stage}\nStart Time: {item.stage_start_timestamp.ToShortDateString()} {item.stage_start_timestamp.ToShortTimeString()}\n\n");
                    }
                }
                
            }
        }
        public static async Task CheckAllowance()
        {
            
            string responseBody = await GetData("https://developer.sepush.co.za/business/2.0/api_allowance");
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
        public static async Task Today()
        {
            string i = await GetData("https://developer.sepush.co.za/business/2.0/area?id=nelsonmandelabay-5-summerstranduptomarinehotelarea8");
            Rootobject message = JsonSerializer.Deserialize<Rootobject>(i);
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
                        $"Date: {item.start:dddd d MMMM yyyy}\n" +
                        $"Start Time: {item.start:t}\n\n" +
                        $"End Time: {item.end:t}\n\n");
                    x++;
                }

                Console.WriteLine($"================ NOTICE !!! =====================\n" +
                    $"== Stage : {message.events[0].note}\t\t\t\t=\n" +
                        $"== Date: {message.events[0].start:dddd d MMMM yyyy}\t\t=\n" +
                        $"== Start Time: {message.events[0].start:t}\t\t\t\t=\n" +
                        $"== End Time: {message.events[0].end:t}\t\t\t\t=\n" +
                        $"================ NOTICE !!! =====================\n\n");
                var diff = message.events[0].start - DateTime.Now;
                Console.WriteLine($" Total Minutes till LOAD SHEDDING: {diff.TotalMinutes:f0}");
            }
            if (false)
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
                        Rootobject message1 = JsonSerializer.Deserialize<Rootobject>(responseBody);
                        if (message1 == null)
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
        public static async Task Shutdown(Rootobject rootobject)
        {
            string i = await GetData("https://developer.sepush.co.za/business/2.0/area?id=nelsonmandelabay-5-summerstranduptomarinehotelarea8");
            Rootobject message = JsonSerializer.Deserialize<Rootobject>(i);
            if (message.events[0].start.Date == DateTime.Now.Date)
            {
                Console.WriteLine($"================ NOTICE !!! =====================\n" +
                    $"== Stage : {message.events[0].note}\t\t\t\t=\n" +
                        $"== Date: {message.events[0].start:dddd d MMMM yyyy}\t\t=\n" +
                        $"== Start Time: {message.events[0].start:t}\t\t\t\t=\n" +
                        $"== End Time: {message.events[0].end:t}\t\t\t\t=\n" +
                        $"================ NOTICE !!! =====================\n\n");
                var diff = message.events[0].start - DateTime.Now;
                switch (diff.TotalMinutes)
                {
                    case 30:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("");

                        break;
                    case 20:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case 10:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Black;
                    break;
                }
            }
        }
        public static async Task<string> GetData(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {

                    client.DefaultRequestHeaders.Add("token", "2CFFCF5B-FFDD4CD8-B3EC867E-F5CECE38");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (responseBody == null)
                    {
                        return null;
                    }
                    else
                    {
                        return responseBody;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
