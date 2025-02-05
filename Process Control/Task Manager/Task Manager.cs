using MemoryPack;
using OSProcesses;
using Server;
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
                try
                {
                    socket.Receive(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR] Failed to receive the message from the client.");
                    Console.WriteLine($"[EXCEPTION] {e}");
                    return;
                }

                string receivedMessage = Encoding.UTF8.GetString(message);
                Tuple<double, double, List<OSProcess>> data = null;

                try 
                { 
                    data = ConvertBytecodeToCSVandThenToString(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[EXCEPTION] Error converting to Tuple: {e.Message}");
                    return;
                }

                if (data == null)
                {
                    Console.WriteLine("[ERROR] Failed to convert the received data to a string.");
                    return;
                }
                PrintCurrentlyRunningProcesses(data);
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
                catch
                {
                    Environment.Exit(0);
                    return;
                }
                Thread.Sleep(100);
            }
        }

        // this function should run in background so that it does not impact the Task Manager output
        public static void SeeAllResultsOrRefreshConsole()
        {
            Console.WriteLine("Press any key within 5 seconds to see all results, without the console clearing.");
            Console.WriteLine("If you press nothing, the console will automatically refresh when there is new data.");
            Console.CursorVisible = false;

            Task<ConsoleKeyInfo> task = Task.Run(() => Console.ReadKey(true));

            if (task.Wait(5000))  // Waits up to 5 seconds
            {
                Console.WriteLine("We are now in the mode where the console will not clear.");
                return;
            }

            Console.WriteLine("The console will now refresh every time the data is received.");
            clear = true;
            return;
        }


        public static void PrintCurrentlyRunningProcesses(Tuple<double, double, List<OSProcess>> data)
        {
            if (data == null)
            {
                Console.WriteLine("[ERROR] Failed to print the currently running processes.");
                return;
            }

            if (clear)
                Console.Clear();

            double processorState = data.Item1;
            double memoryState = data.Item2;
            List<OSProcess> RunningProcesses = data.Item3;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                 THE TASK MANAGER                     _   □   ×   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                          Currently Running Process List                          ║");
            try
            {
                if (RunningProcesses.Count == 0)
                {
                    Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════╣");
                    Console.WriteLine("║                        No processes are currently running                        ║");
                    Console.WriteLine("║               ProTip: Add a process to make the system productive!               ║");
                    Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════╣");

                }
                else
                {
                    Console.WriteLine("╠══════════════════════╤══════════════════╤════════════╤════════════╤══════════════╣");
                    // Print the table header
                    Console.WriteLine(string.Format("║ {0,-20} │ {2,-16} │ {3,-10} │ {4,-10} │ {5,-12} ║",
                        "Process Name", "Total Time", "Time to Complete", "Priority", "Processor", "Memory"));
                    Console.WriteLine("╟──────────────────────┼──────────────────┼────────────┼────────────┼──────────────╢");

                    // Print each process as a row in the table
                    foreach (var runningProcess in RunningProcesses)
                    {
                        Console.WriteLine(string.Format("║ {0,-20} │ {2,-16} │ {3,-10} │ {4,-10} │ {5,-12} ║",
                            runningProcess.name,
                            runningProcess.name,
                            runningProcess.timeToComplete + "ms",
                            runningProcess.priority,
                            runningProcess.processor.ToString() + "%",
                            runningProcess.memory.ToString() + "%"));
                    }

                    Console.WriteLine("╠══════════════════════╧══════════════════╧════════════╧════════════╧══════════════╣");
                }

                // Print OS state summary
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║                               The Current OS Usage                               ║");
                Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════╣");

                Console.Write("║ ");
                if (processorState <= 50)
                    Console.ForegroundColor = ConsoleColor.Green; // Low usage
                else if (processorState <= 75)
                    Console.ForegroundColor = ConsoleColor.DarkYellow; // Medium usage
                else
                    Console.ForegroundColor = ConsoleColor.Red; // High usage
                string processor = $"Processor Usage: [{new string('■', (int)(processorState / ((double)100 / 56)))}{new string('-', 56 - (int)(processorState / ((double)100 / 56)))}] {processorState}%";
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

                string memory = $"Memory Usage   : [{new string('■', (int)(memoryState / ((double)100 / 56)))}{new string('-', 56 - (int)(memoryState / ((double)100 / 56)))}] {memoryState}%";
                Console.Write(memory);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(memoryState < 10 ? "   ║" : (memoryState == 100) ? " ║" : "  ║");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════╝\n");
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


        private static Tuple<double, double, List<OSProcess>> ConvertBytecodeToCSVandThenToString(byte[] data)
        {
            OS serializableOS = null;

            if (data == null)
            {
                Console.WriteLine("[ERROR] Data is empty?.");
                return null;
            }

            try 
            { 
                serializableOS = MemoryPackSerializer.Deserialize<OS>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[EXCEPTION] Error converting to Tuple: {e.Message}");
                return null;
            }

            if (serializableOS == null)
            {
                Console.WriteLine("[ERROR] Failed to convert the received data to a Tuple.");
                return null;
            }
            //Tuple<double, double,List<OSProcess>> ret = new Tuple<double, double, List<OSProcess>>(serializableOS.processorState, serializableOS.memoryState, serializableOS.RunningProcesses);
            return Tuple.Create(serializableOS.processorState, serializableOS.memoryState, serializableOS.RunningProcesses);
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

