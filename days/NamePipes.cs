using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace coms
{
    internal class NamedPipes
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WaitNamedPipe(string name, int timeout);

        public static string LuaPipeName = "uoQcySKXSUxxJNpVQyatpHQwYoGfhcbh";

        public static bool NamedPipeExist(string pipeName)
        {
            try
            {
                if (!WaitNamedPipe("\\\\.\\pipe\\" + pipeName, 0))
                {
                    int err = Marshal.GetLastWin32Error();
                    return err != 0 && err != 2;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void LuaPipe(string script, int pid)
        {
            string pipeName = LuaPipeName + "_" + pid;

            if (!NamedPipeExist(pipeName))
                return;

            new Thread(() =>
            {
                try
                {
                    using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                    {
                        pipeClient.Connect(500);
                        using (var writer = new StreamWriter(pipeClient, Encoding.UTF8) { AutoFlush = true })
                        {
                            writer.Write(script);
                        }
                    }
                }
                catch (IOException)
                {

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LuaPipe error: " + ex.Message);
                }
            }).Start();
        }
    }
}
