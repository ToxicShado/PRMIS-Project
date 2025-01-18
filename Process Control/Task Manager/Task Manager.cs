using OSProcesses;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Task_Manager
{
    public class TaskManager
    {
        static int PID = -1;
        static bool clear = false;
        static void Main(string[] args)
        {
            Console.Title = "Task Manager";

            Socket socket = InitialiseServersideCommunication();
            if (socket == null)
            {
                Console.WriteLine("[ERROR] Failed to initialise the server-side communication.");
                return;
            }

            AutomaticallyCloseTaskManager(socket);


            Thread thread2 = new Thread(() => SeeAllResultsOrRefreshConsole()) { IsBackground = true };
            thread2.Start();


            while (true)
            {
                byte[] message = new byte[4096];
                socket.Receive(message);
                string receivedMessage = Encoding.UTF8.GetString(message);

                PrintCurrentlyRunningProcesses(ConvertBytecodeToCSVandThenToString(message), clear);
            }
        }

        // This is just unnecessary, but it's fun
        //static void TypeText(string text, int delay = 10)
        //{
        //    foreach (char c in text)
        //    {
        //        Console.Write(c);
        //        System.Threading.Thread.Sleep(delay);
        //    }
        //    Console.WriteLine();
        //}

        public static void AutomaticallyCloseTaskManager(Socket socket)
        {
            byte[] rawPID = new byte[4096];
            socket.Receive(rawPID);
            int.TryParse(Encoding.UTF8.GetString(rawPID), out PID);
            Thread thread = new Thread(() => CloseTaskManager()) { IsBackground = true };
            thread.Start();
        }

        public static void CloseTaskManager()
        {
            Thread.Sleep(1000);
            while (true)
            {
                try
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(PID);
                }
                catch (Exception e)
                {
                    Environment.Exit(0);
                    return;
                }
                Thread.Sleep(100);
            }
        }

        // this function should run in background so that it does not impact the Task Manager output
        public static bool SeeAllResultsOrRefreshConsole()
        {
            Console.WriteLine("Press any key within 5 seconds to see all results, without the console clearing.");
            Console.WriteLine("If you press nothing, the console will automatically refresh.");
            Console.CursorVisible = false;

            Task<ConsoleKeyInfo> task = Task.Run(() => Console.ReadKey(true));

            if (task.Wait(5000))  // Waits up to 5 seconds
            {
                Console.WriteLine("We are now in the mode where the console will not clear.");
                return false;
            }

            Console.Clear();
            Console.WriteLine("The console will now refresh every time the data is received.");
            return true;
        }


        public static void PrintCurrentlyRunningProcesses(Tuple<double, double, List<Tuple<OSProcess, DateTime>>> data, bool clear)
        {
            if (clear)
                Console.Clear();

            double processorState = data.Item1;
            double memoryState = data.Item2;
            List<Tuple<OSProcess, DateTime>> RunningProcesses = data.Item3;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                          THE TASK MANAGER                              _   □   ×   ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                  Currently Running Process List                                    ║");
            try
            {
                if (RunningProcesses.Count == 0)
                {
                    Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
                    Console.WriteLine("║                                 No processes are currently running                                 ║");
                    Console.WriteLine("║                        ProTip: Add a process to make the system productive!                        ║");
                    Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");

                }
                else
                {
                    Console.WriteLine("╠══════════════════════╤═════════════════╤══════════════════╤════════════╤════════════╤══════════════╣");
                    // Print the table header
                    Console.WriteLine(string.Format("║ {0,-20} │ {1,-15} │ {2,-16} │ {3,-10} │ {4,-10} │ {5,-12} ║",
                        "Name", "Added On", "Time to Complete", "Priority", "Memory", "Processor"));
                    Console.WriteLine("╟──────────────────────┼─────────────────┼──────────────────┼────────────┼────────────┼──────────────╢");

                    // Print each process as a row in the table
                    foreach (var runningProcess in RunningProcesses)
                    {
                        Console.WriteLine(string.Format("║ {0,-20} │ {1,-15} │ {2,-16} │ {3,-10} │ {4,-10} │ {5,-12} ║",
                            runningProcess.Item1.name,
                            runningProcess.Item2.ToString("HH:mm:ss.fff"),
                            runningProcess.Item1.timeToComplete + "ms",
                            runningProcess.Item1.priority,
                            runningProcess.Item1.memory.ToString() + "%",
                            runningProcess.Item1.processor.ToString() + "%"));
                    }

                    Console.WriteLine("╠══════════════════════╧═════════════════╧══════════════════╧════════════╧════════════╧══════════════╣");
                }

                // Print OS state summary
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║                                        The Current OS Usage                                        ║");
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");

                Console.Write("║ ");
                if (processorState <= 50)
                    Console.ForegroundColor = ConsoleColor.Green; // Low usage
                else if (processorState <= 75)
                    Console.ForegroundColor = ConsoleColor.DarkYellow; // Medium usage
                else
                    Console.ForegroundColor = ConsoleColor.Red; // High usage
                string processor = $"Processor Usage: [{new string('■', (int)(processorState / ((double)100 / 74)))}{new string('-', 74 - (int)(processorState / ((double)100 / 74)))}] {processorState}%";
                Console.Write(processor);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(processorState < 10 ? "   ║" : (processorState == 100) ? " ║" : "  ║");

                Console.Write("║ ");
                if (memoryState <= 50)
                    Console.ForegroundColor = ConsoleColor.Green; // Low usage
                else if (memoryState <= 75)
                    Console.ForegroundColor = ConsoleColor.DarkYellow; // Medium usage
                else
                    Console.ForegroundColor = ConsoleColor.Red; // High usage

                string memory = $"Memory Usage   : [{new string('■', (int)(memoryState / ((double)100 / 74)))}{new string('-', 74 - (int)(memoryState / ((double)100 / 74)))}] {memoryState}%";
                Console.Write(memory);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(memoryState < 10 ? "   ║" : (memoryState == 100) ? " ║" : "  ║");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════════════════╝\n");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.WriteLine($"[EXCEPTION] Error printing currently running processes: {e.Message}");
            }
            finally
            {
                Console.ResetColor();
            }
        }


        private static Tuple<double, double, List<Tuple<OSProcess, DateTime>>> ConvertBytecodeToCSVandThenToString(byte[] data)
        {
            string raw = Encoding.UTF8.GetString(data);
            string retVal = "";
            double processorState = 0;
            double memoryState = 0;
            List<Tuple<OSProcess, DateTime>> RunningProcesses = new List<Tuple<OSProcess, DateTime>>();

            string[] lines = raw.Split(',');

            if (lines.Length < 3)
            {
                Console.WriteLine("[ERROR] Invalid data received from the client.");
                //return null;
            }
            else if (lines.Length == 3)
            {
                bool saccess = double.TryParse(lines[0], out processorState);
                bool saccess1 = double.TryParse(lines[1], out memoryState);
                if (!saccess || !saccess1)
                {
                    Console.WriteLine("[ERROR] Invalid data received from the client. ==3");
                    return null;
                }
                return new Tuple<double, double, List<Tuple<OSProcess, DateTime>>>(processorState, memoryState, RunningProcesses);
            }
            else
            {

                processorState = double.Parse(lines[0]);
                memoryState = double.Parse(lines[1]);
                for (int i = 2; i < lines.Length; i += 6)
                {
                    if (i + 5 >= lines.Length)
                    {
                        Console.WriteLine("[ERROR] Invalid data received from the client. there are more lines than expected");
                        return null;
                    }
                    string name = lines[i];
                    int timeToComplete;
                    int priority;
                    double memory;
                    double processor;
                    DateTime addedOn;
                    bool one = int.TryParse(lines[i + 1], out timeToComplete);
                    bool two = int.TryParse(lines[i + 2], out priority);
                    bool three = double.TryParse(lines[i + 3], out memory);
                    bool four = double.TryParse(lines[i + 4], out processor);
                    bool five = DateTime.TryParse(lines[i + 5], out addedOn);
                    if (!String.IsNullOrEmpty(name) && one && two && three && four && five)
                    {
                        RunningProcesses.Add(new Tuple<OSProcess, DateTime>(new OSProcess(name, timeToComplete, priority, memory, processor), addedOn));
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Invalid data received from the client. all?");
                        return null;
                    }
                }

            }
            return new Tuple<double, double, List<Tuple<OSProcess, DateTime>>>(processorState, memoryState, RunningProcesses);
        }



        //comunication

        public static Socket InitialiseServersideCommunication()
        {
            Socket udpSocket;
            IPEndPoint serverEP;

            Console.WriteLine();
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Console.WriteLine("[STATUS] UDP socket created.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to create the UDP socket.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            try
            {
                serverEP = new IPEndPoint(IPAddress.Any, 25566);
                udpSocket.Bind(serverEP);
                Console.WriteLine("[STATUS] Server is ready and awaiting a new connection.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to bind the UDP socket to the endpoint (127.0.0.1:25566).");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            return udpSocket;
        }
    }
}

