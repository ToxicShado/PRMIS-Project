using Process;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class OS
    {
        public double processorState { get; private set; }
        public double memoryState { get; private set; }
        public List<Tuple<OSProcess, DateTime>> RunningProcesses { get; private set; }
        private static OS instance = null;
        private readonly Mutex mutex;

        private static Socket TaskManagerSocket = null;
        private static IPEndPoint TaskManagerEP = null;
        private OS()
        {
            processorState = 0;
            memoryState = 0;
            RunningProcesses = new List<Tuple<OSProcess, DateTime>>();
            mutex = new Mutex();

            if (TaskManagerSocket == null)
            {
                Console.WriteLine("[ERROR] Connection failed");
                return; //what does this return exactly? the whole server?
            }

            Thread backgroundThread = new Thread(() =>
                                      {
                                          while (true)
                                          {
                                              RemoveProcessIfFinished();
                                              Thread.Sleep(50);
                                          }
                                      }
                                      )
            { IsBackground = true };

            backgroundThread.Start();

        }

        public static OS getInstance()
        {
            if (instance == null)
            {
                instance = new OS();
            }
            return instance;
        }

        public static bool OpenTaskManagerAndConnectToIt()
        {
            string currentPath = Directory.GetCurrentDirectory();

            // Navigate up three levels to the root directory (Process Control)
            string projectRoot = Directory.GetParent(currentPath).Parent.Parent.Parent.FullName;

            // Assemble the path to Task Manager.exe
            string taskManagerPath = Path.Combine(projectRoot, "Task Manager", "bin", "Debug", "net9.0", "Task Manager.exe");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = taskManagerPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal  // Can be Minimized, Maximized, Hidden
            };

            System.Diagnostics.Process taskManagerProcess = System.Diagnostics.Process.Start(psi);

            Thread.Sleep(1000); // Wait for the Task Manager to start

            TaskManagerEP = new IPEndPoint(IPAddress.Loopback, 25566);
            TaskManagerSocket = InitialiseConnectionWithTaskManager();

            if (TaskManagerSocket == null)
            {
                Console.WriteLine("[ERROR] Connection failed");
                return false;
            }

            //taskManagerProcess.WaitForExitAsync();
            //Console.WriteLine("[Server] Task Manager.exe has exited. Closing Server...");

            //// Step 6: Exit the Server process
            //Environment.Exit(0);

            return true;
        }

        public bool IsTherePlaceForNewProcess(OSProcess process)
        {
            bool result = false;
            try
            {
                mutex.WaitOne();
                if (processorState + process.processor <= 100 && memoryState + process.memory <= 100)
                {
                    result = true;
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return result;

        }

        public void AddNewProcess(OSProcess process)
        {
            try
            {
                mutex.WaitOne();
                RunningProcesses.Add(new Tuple<OSProcess, DateTime>(process, DateTime.Now));
                processorState += process.processor;
                memoryState += process.memory;
                Console.WriteLine($"[OS] Process {process.ToString()} has started running.");
                PrintCurrentlyRunningProcesses();
            }
            finally
            {
                mutex.ReleaseMutex();
            }

        }

        public void RemoveProcessIfFinished()
        {
            try
            {
                mutex.WaitOne();
                var ProccessesToRemove = new List<Tuple<OSProcess, DateTime>>();
                foreach (Tuple<OSProcess, DateTime> process in RunningProcesses)
                {
                    if (DateTime.Now - process.Item2 > TimeSpan.FromMilliseconds(process.Item1.timeToComplete))
                    {
                        ProccessesToRemove.Add(process);
                    }
                }
                foreach (Tuple<OSProcess, DateTime> process in ProccessesToRemove)
                {
                    RunningProcesses.Remove(process);
                    processorState -= process.Item1.processor;
                    memoryState -= process.Item1.memory;
                    Console.WriteLine($"[OS] Process {process.ToString()} has stopped running.");
                }
                if (ProccessesToRemove.Count > 0)
                    PrintCurrentlyRunningProcesses();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void PrintCurrentlyRunningProcesses()
        {
            byte[] messageData = ConvertOSToCSVAndThenToBytecode();
            TaskManagerSocket.SendTo(messageData, TaskManagerEP);
        }

        private byte[] ConvertOSToCSVAndThenToBytecode()
        {
            string csv = "";
            csv += $"{processorState},{memoryState},";
            if (RunningProcesses.Any())
            {
                foreach (Tuple<OSProcess, DateTime> process in RunningProcesses)
                {
                    csv += $"{process.Item1.name},{process.Item1.timeToComplete},{process.Item1.priority},{process.Item1.memory},{process.Item1.processor},{process.Item2},";
                }
                csv = csv.TrimEnd(',');
            }
            csv += "\n";
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }


        private static Socket InitialiseConnectionWithTaskManager()
        {
            // Create a UDP socket for initiating a connection
            Socket udpSocket;
            IPEndPoint serverEP;
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                serverEP = new IPEndPoint(IPAddress.Loopback, 25566);
                Console.WriteLine("[STATUS] UDP socket created.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Unable to open a UDP socket on IPAddress: 127.0.0.1 and Port: 25566.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            // Send a registration message to the server
            try
            {
                string registrationMessage = "Could You Please Connect?";
                byte[] registrationData = System.Text.Encoding.UTF8.GetBytes(registrationMessage);
                udpSocket.SendTo(registrationData, serverEP);
                Console.WriteLine("[STATUS] Sent registration message to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Unable to send registration message to server at {serverEP.Address}:{serverEP.Port}.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }
            return udpSocket;
        }


    }
}


