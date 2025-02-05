using MemoryPack;
using OSProcesses;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    [MemoryPackable]
    public partial class OS
    {
        public double processorState { get; private set; }
        public double memoryState { get; private set; }
        public List<OSProcess> RunningProcesses { get; private set; }
        [MemoryPackIgnore] private static OS instance = null;
        [MemoryPackIgnore] private readonly Mutex mutex;

        [MemoryPackIgnore] private static Socket TaskManagerSocket = null;
        [MemoryPackIgnore] private static IPEndPoint TaskManagerEP = null;
        [MemoryPackIgnore] private const int cpuQuant = 200;


        // // In case the thingy below doesn't work, this is the backup constructor
        // // Honestly, it would not surprise me if it's needed.
        //[MemoryPackConstructor]
        //private OS(double ProcessorState, double MemoryState, List<OSProcess> RunningProcesses)
        //{
        //    processorState = ProcessorState;
        //    memoryState = MemoryState; 
        //    this.RunningProcesses = RunningProcesses ?? new List<OSProcess>();
        //}

        [MemoryPackConstructor]
        private OS()
        {
            processorState = 0;
            memoryState = 0;
            RunningProcesses = new List<OSProcess>();
            mutex = new Mutex();
        }

        public void PickScheduling()
        {
            // Here is the logic to determine .
            Console.WriteLine("Pick whether you wish to use Round Robin (1) or to sort by priority (2)");
            int choice = -1;

            // Keep asking for input until it's valid and either 1 or 2.
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out choice) && (choice == 1 || choice == 2))
                {
                    break;
                }
                Console.WriteLine("Invalid input, please try again");
            }

            Console.WriteLine($"Choice {choice}");

            Thread scheduler;
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Round Robin Scheduling Activated!");
                    scheduler = new Thread(() =>
                    {
                        RoundRobinScheduling();
                    }
                                      )
                    { IsBackground = true };
                    scheduler.Start();
                    break;
                case 2:
                    Console.WriteLine("Priority Scheduling Activated!");
                    break;
                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }

        public static OS getInstance()
        {
            if (instance == null)
            {
                instance = new OS();
            }
            return instance;
        }

        public bool OpenTaskManagerAndConnectToIt()
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

            Process taskManagerProcess = Process.Start(psi);

            Thread.Sleep(1000); // Wait for the Task Manager to start

            TaskManagerEP = new IPEndPoint(IPAddress.Loopback, 25566);
            TaskManagerSocket = InitialiseConnectionWithTaskManager();

            if (TaskManagerSocket == null || TaskManagerEP == null)
            {
                Console.WriteLine("[ERROR] Connection failed with task manager");
                return false;
            }

            int PID = Process.GetCurrentProcess().Id;
            TaskManagerSocket.SendTo(Encoding.UTF8.GetBytes(PID.ToString()), TaskManagerEP);

            return true;
        }

        public static bool areThereRunningProcesses()
        {
            return instance.RunningProcesses.Any();
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
                RunningProcesses.Add(process);
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

        public void PrintCurrentlyRunningProcesses()
        {
            if (TaskManagerSocket == null || TaskManagerEP == null)
            {
                Console.WriteLine("[ERROR] Task Manager Socket or Endpoint is null. Cannot send data.");
                return;
            }

            if (RunningProcesses == null)
            {
                Console.WriteLine("[WARNING] RunningProcesses is null. Initializing an empty list.");
                RunningProcesses = new List<OSProcess>();
            }

            try
            {
                byte[] serializedData = MemoryPackSerializer.Serialize(this);

                if (serializedData == null || serializedData.Length == 0)
                {
                    Console.WriteLine("[ERROR] Serialization returned empty data.");
                    return;
                }

                TaskManagerSocket.SendTo(serializedData, TaskManagerEP);
                //Console.WriteLine("[INFO] Successfully sent OS data to Task Manager.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[EXCEPTION] Failed to serialize and send OS data: {e.Message}");
            }
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

            return udpSocket;
        }

        /*
            RoundRobinScheduling()
            This function goes through the list of processes and simulates
            the CPU processing them by decrementing the time to complete
            and removing them if they are done.
            Also, it sleeps for a certain amount of time to simulate the time
            it takes to process them.
         */
        public void RoundRobinScheduling()
        {
            char[] loadingSymbols = { '\\', '|', '/', '-' };
            int animationIndex = 0;
            while (true)
            {
                lock (RunningProcesses) // Ensure thread safety if accessed from multiple threads
                {
                    for (int i = RunningProcesses.Count - 1; i >= 0; i--) // Iterate in reverse to remove safely
                    {
                        var processTuple = RunningProcesses[i];
                        OSProcess process = processTuple;

                        process.timeToComplete -= cpuQuant;

                        // Clear the entire line before printing new output
                        Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
                        Console.Write($"{loadingSymbols[animationIndex]} Process name: {process.name} Remaining Time: {Math.Max(process.timeToComplete, 0)}");
                        animationIndex = (animationIndex + 1) % loadingSymbols.Length; // Rotate symbols

                        Thread.Sleep(cpuQuant); // For smoother transitions 

                        if (process.timeToComplete <= 0)
                        {
                            Console.WriteLine($"\nProcess named {process.name} completed and removed.");
                            RunningProcesses.RemoveAt(i);

                            processorState -= process.processor;
                            memoryState -= process.memory;

                            Thread.Sleep(200); // Adjust delay for smoother transition
                        }
                        PrintCurrentlyRunningProcesses();
                    }
                }
                Thread.Sleep(cpuQuant); // Adjust sleep duration as needed
            }
        }
    }
}


