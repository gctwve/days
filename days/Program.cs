using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VelocityAPI;

namespace DaysExecutor
{
    internal class Program
    {
        static Dictionary<string, string> validKeys = new Dictionary<string, string>
    {
        { "Guiisepic", "Access" }, //guys if you want role based just copy and paste
    };

        static void GrantAccess(string userKey)
        {
            if (validKeys.ContainsKey(userKey))
            {
                //nothing cuz then it will break asf
            }
            else
            {
                Console.WriteLine("Invalid key. Access denied.");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }
        public static async Task Main(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
██████╗░░█████╗░██╗░░░██╗░██████╗  ███████╗██╗░░██╗███████╗░█████╗░██╗░░░██╗████████╗░█████╗░██████╗░
██╔══██╗██╔══██╗╚██╗░██╔╝██╔════╝  ██╔════╝╚██╗██╔╝██╔════╝██╔══██╗██║░░░██║╚══██╔══╝██╔══██╗██╔══██╗
██║░░██║███████║░╚████╔╝░╚█████╗░  █████╗░░░╚███╔╝░█████╗░░██║░░╚═╝██║░░░██║░░░██║░░░██║░░██║██████╔╝
██║░░██║██╔══██║░░╚██╔╝░░░╚═══██╗  ██╔══╝░░░██╔██╗░██╔══╝░░██║░░██╗██║░░░██║░░░██║░░░██║░░██║██╔══██╗
██████╔╝██║░░██║░░░██║░░░██████╔╝  ███████╗██╔╝╚██╗███████╗╚█████╔╝╚██████╔╝░░░██║░░░╚█████╔╝██║░░██║
╚═════╝░╚═╝░░╚═╝░░░╚═╝░░░╚═════╝░  ╚══════╝╚═╝░░╚═╝╚══════╝░╚════╝░░╚═════╝░░░░╚═╝░░░░╚════╝░╚═╝░░╚═╝");
            Console.ForegroundColor = ConsoleColor.White;
            var task1 = Task.Run(() => ChangeConsoleTitle()); //keep it running in the bc
            var task2 = Task.Run(() => ShowProgressBar());

            await Task.WhenAll(task2);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Welcome to Days Executor!");
            Console.ResetColor();
            Console.WriteLine("Please input the key:");
            string thefuckingkey = Console.ReadLine().Trim();
            GrantAccess(thefuckingkey);

            Console.WriteLine("Checking Roblox and Injection status...");

            if (!VelocityAPI.VelAPI.IsRobloxOpen())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Roblox is not open. Please open Roblox and restart.");
                Console.ReadLine();
                Console.ResetColor();
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🔰 Please Enter your Roblox username.");
                Console.ReadLine();
                string userenter = Console.ReadLine();
                Console.WriteLine("User:", userenter);
                string finale = "print('" + userenter + "')";
            }

            Console.WriteLine("Injecting...");
            var vel = new VelocityAPI.VelAPI();
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
            int pid = processes[0].Id;
            var result = await vel.AttachAsync(pid);

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("1. Search scripts from ScriptBlox");
                Console.WriteLine("2. View latest scripts");
                Console.WriteLine("3. Execute your own script");
                Console.WriteLine("4. Close");
                Console.Write("Choice (1, 2, 3, or 4): ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                if (choice == "1" || choice == "2")
                {
                    ScriptBloxScript[] scripts;

                    if (choice == "1")
                    {
                        Console.Write("\nEnter search query: ");
                        string query = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(query))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("❌ Query cannot be empty.");
                            Console.ResetColor();
                            continue;
                        }

                        scripts = await ScriptBloxApi.SearchScriptsAsync(query);
                    }
                    else
                    {
                        Console.WriteLine("\nFetching latest scripts...");
                        scripts = await ScriptBloxApi.FetchHomepageScriptsAsync();
                    }

                    if (scripts == null || scripts.Length == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ No scripts found.");
                        Console.ResetColor();
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nAvailable Scripts:");
                    for (int i = 0; i < scripts.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}. {scripts[i].title} (Game: {scripts[i].game?.name ?? "Unknown"})");
                    }
                    Console.ResetColor();

                    Console.Write($"\nChoose a script to view (1–{scripts.Length}): ");
                    if (int.TryParse(Console.ReadLine(), out int scriptChoice) && scriptChoice >= 1 && scriptChoice <= scripts.Length)
                    {
                        string selectedScript = scripts[scriptChoice - 1].script;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n📜 Script Content:\n");
                        Console.ResetColor();
                        Console.WriteLine(selectedScript);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n🔹 You can now copy the script above.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ Invalid choice.");
                        Console.ResetColor();
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("\nPaste your script below:\n");
                    string customScript = Console.ReadLine();
                    try
                    {
                        var execResilt = vel.Execute(customScript);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ Your script has been executed.");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ Script execution failed: {ex.Message}");
                    }
                    finally
                    {
                        Console.ResetColor();
                    }
                }
                else if (choice == "4")
                {
                    Console.WriteLine("Thanks for using days!");
                    Thread.Sleep(3000);
                    return;
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Invalid selection.");
                    Console.ResetColor();
                }
                

                    Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
                
            }
        }

        private static void ChangeConsoleTitle()
        {
            Random random = new Random();
            while (true)
            {
                string title = GenerateRandomString(random, 15);
                Console.Title = title;
                Thread.Sleep(100);
            }
        }

        private static string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWGUIGUIGUIGUIGUIGUIGUIGUIGUIGUIXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        private static void ShowProgressBar()
        {
            for (int i = 0; i <= 100; i += 5)
            {
                int totalBlocks = 20;
                int filledBlocks = i / 5;
                string value = $"[{new string('=', filledBlocks)}{new string(' ', totalBlocks - filledBlocks)}] {i}%";

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(value);
                Thread.Sleep(50);
            }
            Console.WriteLine();
        }
    }

    public class ScriptBloxApi
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<ScriptBloxScript[]> SearchScriptsAsync(string query, string mode = "free")
        {
            try
            {
                string url = $"https://scriptblox.com/api/script/search?q={Uri.EscapeDataString(query)}&mode={mode}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<ScriptBloxApiResponse>(json);
                return parsed?.result?.scripts ?? Array.Empty<ScriptBloxScript>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error fetching scripts: {ex.Message}");
                Console.ResetColor();
                return Array.Empty<ScriptBloxScript>();
            }
        }

        public static async Task<ScriptBloxScript[]> FetchHomepageScriptsAsync()
        {
            try
            {
                string url = "https://scriptblox.com/api/script/fetch";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<ScriptBloxFetchResult>(json);
                return parsed?.result?.scripts ?? Array.Empty<ScriptBloxScript>();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error fetching homepage scripts: {ex.Message}");
                Console.ResetColor();
                return Array.Empty<ScriptBloxScript>();
            }
        }
    }

    public class ScriptBloxApiResponse
    {
        public ScriptBloxResult result { get; set; }
    }

    public class ScriptBloxFetchResult
    {
        public ScriptBloxResult result { get; set; }
    }

    public class ScriptBloxResult
    {
        public ScriptBloxScript[] scripts { get; set; }
    }

    public class ScriptBloxScript
    {
        public string _id { get; set; }
        public string title { get; set; }
        public GameInfo game { get; set; }
        public string script { get; set; }
        public bool verified { get; set; }
        public int views { get; set; }
        public string scriptType { get; set; }
    }

    public class GameInfo
    {
        public long gameId { get; set; }
        public string name { get; set; }
        public string imageUrl { get; set; }
    }
}
