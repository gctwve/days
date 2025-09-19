using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using coms;

namespace VelocityAPI
{
    public class VelAPI
    {
        private HttpClient client = new HttpClient();
        private string current_injector_url = "https://getvelocity.live/assets/erto3e4rortoergn.exe";
        private string current_decompiler_url = "https://getvelocity.live/assets/Decompiler.exe";

        private Process decompilerProcess;
        public VelocityStates VelocityStatus = VelocityStates.NotAttached;
        public List<int> injected_pids = new List<int>();
        private Timer CommunicationTimer;

        public static string Base64Encode(string plainText) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

        private bool IsPidRunning(int pid)
        {
            try
            {
                Process.GetProcessById(pid);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static bool IsRobloxOpen()
        {
            return Process.GetProcessesByName("RobloxPlayerBeta").Any<Process>();
        }

        private async Task DownloadVelocityAsync()
        {
            Directory.CreateDirectory("Bin");

            if (File.Exists("Bin\\erto3e4rortoergn.exe")) File.Delete("Bin\\erto3e4rortoergn.exe");
            if (File.Exists("Bin\\Decompiler.exe")) File.Delete("Bin\\Decompiler.exe");

            var injectorResponse = await client.GetAsync(current_injector_url);
            if (injectorResponse.IsSuccessStatusCode)
            {
                var data = await injectorResponse.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes("Bin\\erto3e4rortoergn.exe", data);
            }

            var decompilerResponse = await client.GetAsync(current_decompiler_url);
            if (decompilerResponse.IsSuccessStatusCode)
            {
                var data = await decompilerResponse.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes("Bin\\Decompiler.exe", data);
            }
        }

        public async Task StartCommunicationAsync()
        {
            Directory.CreateDirectory("AutoExec");
            Directory.CreateDirectory("Workspace");
            Directory.CreateDirectory("Scripts");

            StopCommunication();

            decompilerProcess = new Process
            {
                StartInfo =
                {
                    FileName = "Bin\\Decompiler.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            decompilerProcess.Start();

            CommunicationTimer = new Timer(100);
            CommunicationTimer.Elapsed += (sender, e) =>
            {
                foreach (var pid in injected_pids.ToArray())
                {
                    if (!IsPidRunning(pid))
                    {
                        injected_pids.Remove(pid);
                    }
                }

                string message = "setworkspacefolder: " + Path.Combine(Directory.GetCurrentDirectory(), "Workspace");
                foreach (var pid in injected_pids)
                {
                    NamedPipes.LuaPipe(Base64Encode(message), pid);
                }
            };
            CommunicationTimer.Start();
        }

        public void StopCommunication()
        {
            CommunicationTimer?.Stop();
            CommunicationTimer = null;

            if (decompilerProcess != null)
            {
                decompilerProcess.Kill();
                decompilerProcess.Dispose();
                decompilerProcess = null;
            }

            injected_pids.Clear();
        }

        public bool IsAttached(int pid) => injected_pids.Contains(pid);

        public async Task<VelocityStates> AttachAsync(int pid)
        {
            if (!IsPidRunning(pid)) return VelocityStates.NoProcessFound;

            await DownloadVelocityAsync();

            var injectorPath = "Bin\\erto3e4rortoergn.exe";
            if (!File.Exists(injectorPath)) return VelocityStates.Error;

            var injectorProcess = new Process
            {
                StartInfo =
        {
            FileName = injectorPath,
            Arguments = pid.ToString(),
            UseShellExecute = false,
            CreateNoWindow = true
        }
            };
            injectorProcess.Start();

            await Task.Delay(500);

            if (!IsPidRunning(pid)) return VelocityStates.Error;

            injected_pids.Add(pid);

            return VelocityStates.Attached;
        }

        public VelocityStates Execute(string script)
        {
            if (injected_pids.Count == 0) return VelocityStates.NotAttached;

            foreach (var pid in injected_pids)
            {
                NamedPipes.LuaPipe(Base64Encode(script), pid);
            }

            return VelocityStates.Executed;
        }
    }

    public enum VelocityStates
    {
        Attaching,
        Attached,
        NotAttached,
        NoProcessFound,
        TamperDetected,
        Error,
        Executed
    }
}
