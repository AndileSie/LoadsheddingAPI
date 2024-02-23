using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace loadshedding
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Welcome.\n" +
                    "1. Check Allowance\n" +
                    "2. Check Todays Schedule\n" +
                    "3. Load shedding stages\n" +
                    "4. Auto Shutdown\n" +
                    "Choice: ");
                string option = Console.ReadLine();
                if (string.IsNullOrEmpty(option))
                {
                    option = "0";
                }
                switch (int.Parse(option))
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
                    case 4:
                        
                        Thread backgroundThread = new Thread(new ThreadStart(Shutdown));
                        backgroundThread.Start();
                        //Console.Write("Welcome.\n" +
                        //    "1. Check Allowance\n" +
                        //    "2. Check Todays Schedule\n" +
                        //    "3. Load shedding stages\n" +
                        //    "4. Auto Shutdown\n" +
                        //    "Choice: ");
                        break;
                    default:
                        Console.WriteLine("\nInvalid Selection\n");
                        break;
                }
            }
            
            
        }
        
        public static async Task CheckStage()
        {

            try
            {
                string responseBody = await GetData("https://developer.sepush.co.za/business/2.0/status");
                StatusClass message = JsonSerializer.Deserialize<StatusClass>(responseBody);
                Console.WriteLine($"Current And Future Stages\n" +
                        $"Current Stage: {message.status.eskom.stage} Last Updated: {message.status.eskom.stage_updated.ToShortDateString()} {message.status.eskom.stage_updated.ToShortTimeString()}\n");

                Console.WriteLine("\nFuture stages implantations\n");
                if (message.status.eskom.next_stages.Length == 0)
                {
                    Console.WriteLine("No future updates\n");

                }
                else
                {

                    foreach (var item in message.status.eskom.next_stages)
                    {

                        Console.WriteLine($"Stage: {item.stage}\nStart Time: {item.stage_start_timestamp.ToShortDateString()} {item.stage_start_timestamp.ToShortTimeString()}\n\n");
                    }
                }
                
            }
            catch
            {
                Console.WriteLine("\nNetwork Error. At method 'Checking Stage'\n");
            }
            
        }
        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nNetwork Error. At method '{message}'\n");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static async Task CheckAllowance()
        {
            try
            {
                string responseBody = await GetData("https://developer.sepush.co.za/business/2.0/api_allowance");
                Root message = JsonSerializer.Deserialize<Root>(responseBody);
                Console.WriteLine($"\nUsed: {message.allowance.count}\nLimit: {message.allowance.limit}\nType: {message.allowance.type}\n");
            }
            catch
            {
                PrintError("Check Allowance");
            }

        }
        public static async Task Today()
        {
            try
            {
                if (string.IsNullOrEmpty(Todaydata))
                {
                    Todaydata = await GetData("https://developer.sepush.co.za/business/2.0/area?id=nelsonmandelabay-5-summerstranduptomarinehotelarea8");
                }
                Rootobject message = JsonSerializer.Deserialize<Rootobject>(Todaydata);
                Console.WriteLine($"Load shedding for: {message.info.name} \n {message.info.region}\n Amount of Events: {message.events.Length}\n\n" +
                    $"");
                int x = 1;
                if (message.events.Length != 0)
                {
                    foreach (var item in message.events)
                    {
                        Console.WriteLine($"Event: {x}\nStage : {item.note}\n" +
                            $"Date: {item.start:dddd d MMMM yyyy}\n" +
                            $"Start Time: {item.start:t}\n\n" +
                            $"End Time: {item.end:t}\n\n");
                        x++;
                    }
                    string d;
                    if (message.events[0].start.Date == DateTime.Now.Date)
                    {
                        d = "TODAY";
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                        d = "FUTURE";


                    Console.WriteLine($"================ NOTICE !!! =====================\n" +
                        $"== Stage : {message.events[0].note}\t\t\t\t=\n" +
                            $"== Date: {message.events[0].start:dddd d MMMM yyyy} ({d})\t=\n" +
                            $"== Start Time: {message.events[0].start:t}\t\t\t\t=\n" +
                            $"== End Time: {message.events[0].end:t}\t\t\t\t=\n" +
                            $"================ NOTICE !!! =====================\n\n");
                    var diff = message.events[0].start - DateTime.Now;
                    Console.WriteLine($" Total Minutes till LOAD SHEDDING: {diff.TotalMinutes:f0}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("There is no load shedding");
                }
                
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch
            {
                PrintError("Today");
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
                            Console.WriteLine($"Load shedding for: {message1.info.name} \n {message1.info.region}\n Amount of Events: {message1.events.Length}\n\n" +
                                $"");
                            int x = 1;
                            foreach (var item in message1.events)
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
        static string Todaydata = "";
        public static async void Shutdown()
        {
            try
            {
                if (string.IsNullOrEmpty(Todaydata))
                {
                    Todaydata = await GetData("https://developer.sepush.co.za/business/2.0/area?id=nelsonmandelabay-5-summerstranduptomarinehotelarea8");
                }
                Rootobject message = JsonSerializer.Deserialize<Rootobject>(Todaydata);
                if (message.events[0].start.Date == DateTime.Now.Date)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\n================ NOTICE !!! =====================\n" +
                       $"== Stage : {message.events[0].note}\t\t\t\t=\n" +
                           $"== Date: {message.events[0].start:dddd d MMMM yyyy}\t\t=\n" +
                           $"== Start Time: {message.events[0].start:t}\t\t\t\t=\n" +
                           $"== End Time: {message.events[0].end:t}\t\t\t\t=\n" +
                           $"================ NOTICE !!! =====================\n");

                    var future = message.events[0].start;
                    var now = DateTime.Now;
                    var diff = future - now;
                    var seconds = (int)(diff.TotalSeconds - 120) / 3;
                    var span = new TimeSpan(0, 0, seconds);

                    //if negative kill process
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n {diff.TotalMinutes:f0} Minutes left\n");
                    Thread.Sleep(span);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    diff = diff.Subtract(span);
                    Console.WriteLine($"\n\n {diff.TotalMinutes:f0} Minutes left");
                    Thread.Sleep(span);

                    Console.ForegroundColor = ConsoleColor.Red;
                    diff = diff.Subtract(span);
                    Console.WriteLine($"\n {diff.TotalMinutes:f0} Minutes left");
                    Thread.Sleep(span);

                    Console.WriteLine("Shutting down in...\n");
                    for (int i = 3; i >= 1 ; i--)
                    {
                        Console.WriteLine($"{i}..");
                        Thread.Sleep(1000);
                    }
                    
                    Process.Start("shutdown", "/s /t 2");
                    //shutdown the system
                    Console.WriteLine("shutdown the system".ToUpperInvariant());
                }
            }
            catch
            {
                PrintError("Shutdown");
            }
            

        }
        public static async Task Shutdown1(Rootobject rootobject)
        {
            //useless at the momemnt
            try
            {
                if (string.IsNullOrEmpty(Todaydata))
                {
                    Todaydata = await GetData("https://developer.sepush.co.za/business/2.0/area?id=nelsonmandelabay-5-summerstranduptomarinehotelarea8");
                }
                Rootobject message = JsonSerializer.Deserialize<Rootobject>(Todaydata);
                if (message.events[0].start.Date == DateTime.Now.Date)
                {
                    Console.WriteLine($"================ NOTICE !!! =====================\n" +
                        $"== Stage : {message.events[0].note}\t\t\t\t=\n" +
                            $"== Date: {message.events[0].start:dddd d MMMM yyyy}\t\t=\n" +
                            $"== Start Time: {message.events[0].start:t}\t\t\t\t=\n" +
                            $"== End Time: {message.events[0].end:t}\t\t\t\t=\n" +
                            $"================ NOTICE !!! =====================\n\n");
                    var diff = message.events[0].start - DateTime.Now;
                    Console.WriteLine($" Total Minutes till LOAD SHEDDING: {diff.TotalMinutes:f0}");
                    var s = (int)diff.TotalSeconds / 3;
                    Thread.Sleep(s);

                    if (diff.TotalMinutes >= 30)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("+30 Minutes Left");
                        Thread.Sleep(10000);

                    }
                    else if (diff.TotalMinutes <= 20)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("20 Minutes Left");
                    }
                    else if (diff.TotalMinutes <= 10)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("30 Minutes Left");

                        if (diff.TotalMinutes <= 3)
                        {
                            Console.WriteLine("Shutting down in...\n");
                            Thread.Sleep(1000);
                            Console.WriteLine("3..");
                            Thread.Sleep(1000);
                            Console.WriteLine("2..");
                            Thread.Sleep(1000);
                            Console.WriteLine("1..");
                            Thread.Sleep(1000);
                            //Process.Start("shutdown", "/s /t 0");
                            //shutdown the system
                        }
                    }
                }
            }
            catch
            {

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
