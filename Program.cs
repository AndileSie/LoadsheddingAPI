﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                await StartUp();
                Console.Write("Welcome.\n" +
                    "1. Check Allowance\n" +
                    "2. Check Todays Schedule\n" +
                    "3. Load shedding stages\n" +
                    "4. Auto Shutdown\n" +
                    "5. Search Area\n" +
                    "6. News around\n" +
                    "Choice: ");
                string option = Console.ReadLine();
                if (string.IsNullOrEmpty(option))
                {
                    option = "0";
                }
                try
                {
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
                            break;
                        case 5:
                            await Search();
                            break;
                        case 6:
                            await Topics();
                            break;
                        default:
                            Console.WriteLine("\nInvalid Selection\n");
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("\nInvalid Selection\n");
                }
                
            }


        }
        static readonly string defaultFile = "default.txt";
        static string loctionID;
        public static async Task StartUp()
        {
            try
            {
                StreamReader streamReader = new StreamReader(defaultFile);
                string[] line = streamReader.ReadLine().Split('~');
                loctionID = line[0];
                Console.WriteLine($"\nCurrent location is set to:\n Name: {line[1]}\n Region: {line[2]}\n");
                streamReader.Close();
            }
            catch
            {
                Console.WriteLine("Default location is not set. Please set a location");
                await Search();
            }
        }
        public static void WriteToFile(Area area)
        {
            StreamWriter writer = new StreamWriter(defaultFile);
            writer.WriteLine($"{area.id}~{area.name}~{area.region}");
            writer.Close();
        }
        public static async Task Search()
        {

            try
            {
                Console.Write("Enter area name: ");
                string area = Console.ReadLine();
                string responseBody = await GetData($"https://developer.sepush.co.za/business/2.0/areas_search?text={area}");
                Location message = JsonSerializer.Deserialize<Location>(responseBody);
                Console.WriteLine("\nSelect area: ");

                for (int i = 0; i < message.areas.Length; i++)
                {
                    Console.WriteLine($"{i}. {message.areas[i].name} {message.areas[i].region}\n");

                }
                while (true)
                {
                    Console.Write("Enter selected area number: ");
                    string choice = Console.ReadLine();
                    if (!string.IsNullOrEmpty(choice) && int.TryParse(choice, out int i))
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"You have selected:\n Name: {message.areas[i].name}\n Region: {message.areas[i].region}\n");
                            Console.ForegroundColor = ConsoleColor.White;
                            WriteToFile(message.areas[i]);
                            Todaydata = "";
                            break;
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid Selection\n");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nInvalid Selection\n");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }


            }
            catch
            {
                PrintError("Searching For Area");
            }

        }
        public static async Task Topics()
        {
            try
            {
                string lon = "25.670687";
                string lat = "-34.001678";
                string responseBody = await GetData($"https://developer.sepush.co.za/business/2.0/topics_nearby?lat={lat}&lon={lon}");
                var message = JsonSerializer.Deserialize<Topics>(responseBody);
                Console.WriteLine($"Topics for {lat}, {lon}\n");
                for (int i = 0; i < message.topics.Length; i++)
                {
                    if (message.topics[i].timestamp.Date == DateTime.Now.Date)
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Event {i + 1}\n" +
                        $"Category: {message.topics[i].category}\n" +
                        $"Text: {message.topics[i].body}\n" +
                        $"Distance: {message.topics[i].distance}\n" +
                        $"Time stamp: {message.topics[i].timestamp.ToShortDateString()} {message.topics[i].timestamp.ToShortTimeString()}\n\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine("End of Topics");
            }
            catch
            {
                PrintError("Topics");
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
                PrintError("Check Stage");
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
                AllowanceClass message = JsonSerializer.Deserialize<AllowanceClass>(responseBody);
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
                    Todaydata = await GetData($"https://developer.sepush.co.za/business/2.0/area?id={loctionID}");
                }
                LoadsheddingEvents message = JsonSerializer.Deserialize<LoadsheddingEvents>(Todaydata);
                Console.WriteLine($"Load shedding for: {message.info.name} \n {message.info.region}\n Amount of Events: {message.events.Length}\n\n" +
                    $"");
                int x = 1;
                if (message.events.Length != 0)
                {
                    foreach (var item in message.events)
                    {
                        Console.WriteLine($"Event: {x}\n{item.note}\n" +
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
                    {
                        d = "FUTURE";
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                        


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

                await SaveData(message, message.info.name.Replace(" ", "_"), message.events[0].note);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch
            {
                PrintError("Today");
            }
        }
        public static Task SaveData(LoadsheddingEvents events, string location, string Stage)
        {
            //store load shedding times one day you may need the data for AI training, predict future load shedding times
            StreamWriter writer = new StreamWriter($"{DateTime.Now:dd_MM_yyy}_{location}.txt", true);

            try
            {
                writer.WriteLine("Date~Day~LoadSheddingDate~Stage~Stage1~Stage2~Stage3~Stage4~Stage5~Stage6~Stage7~Stage8");
                foreach (var item in events.schedule.days)
                {
                    string line = $"{item.date}~{item.name}~{events.events[0].start:yyyy-MM-dd}~{events.events[0].note}~";
                    string x = "";
                    for (int i = 0; i < 8; i++)
                    {
                        if (item.stages[i].Length != 0)
                        {
                            for (int j = 0; j < item.stages[i].Length; j++)
                            {
                                x += $"{item.stages[i][j]},";
                            }
                        }
                        else
                            x += "NULL ";
                        x = x.Substring(0, x.Length - 1) + "~";
                    }
                    line += $"{x.Substring(0, x.Length - 1)}";
                    writer.WriteLine(line);
                }
                writer.Close();
                Console.WriteLine("Done Writing Data");
            }
            catch
            {
                writer.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error while writing data to txt file.");
                Console.ForegroundColor = ConsoleColor.White;

            }

            //writer = new StreamWriter($"{DateTime.Now:dd_MM_yyy}_Stages",true);
            //writer.WriteLine("DateCreated~LoadSheddingDate~Stage");
            //writer.WriteLine($"{DateTime.Now:d}~{events.events[0].start:d}~{events.events[0].note}");
            //writer.Close();


            return Task.CompletedTask;
        }
        static string Todaydata = "";
        public static async void Shutdown()
        {
            try
            {
                if (string.IsNullOrEmpty(Todaydata))
                {
                    Todaydata = await GetData($"https://developer.sepush.co.za/business/2.0/area?id={loctionID}");
                }
                LoadsheddingEvents message = JsonSerializer.Deserialize<LoadsheddingEvents>(Todaydata);
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

                    if (diff.TotalSeconds > 0)
                    {
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
                        for (int i = 3; i >= 1; i--)
                        {
                            Console.WriteLine($"{i}..");
                            Thread.Sleep(1000);
                        }

                        Console.WriteLine("shutdown the system".ToUpperInvariant());
                        //shutdown the system
                        Process.Start("shutdown", "/s /t 2");
                    }
                    
                }
            }
            catch
            {
                PrintError("Shutdown");
            }
            

        }
        
        public static async Task<string> GetData(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //documentation on how to use the api
                    //https://documenter.getpostman.com/view/1296288/UzQuNk3E

                    //how to get the token key
                    //https://eskomsepush.gumroad.com/l/api

                    string token = "";
                    

                    client.DefaultRequestHeaders.Add("token", token);

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
