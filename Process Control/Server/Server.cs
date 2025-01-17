using Process;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        public double processorState { get; private set; }
        public double memoryState { get; private set; }
        public List<Tuple<OSProcess, DateTime>> RunningProcesses { get; private set; }
        public static void Main(string[] args)
        {
            // Open an instance of Task Manager, and connect to it.

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[STATUS] Opening Task Manager...");
            if (OS.OpenTaskManagerAndConnectToIt() == false)
            {
                Console.WriteLine("[ERROR] Failed to open Task Manager.");
                return;
            }
            Console.ResetColor();

            Console.WriteLine("[STATUS] Starting server...");

            //This will be used to determine the scheduling algorithm, when we need it. Until then, it's commented out.

            //Console.WriteLine("Pick whether you wish to use Round Robin (1) or to sort by priority (2)");
            //int choice = -1;

            //// Keep asking for input until it's valid and either 1 or 2.
            //while (true)
            //{
            //    string input = Console.ReadLine();
            //    if (int.TryParse(input, out choice) && (choice == 1 || choice == 2))
            //    {
            //        break;
            //    }
            //    Console.WriteLine("Invalid input, please try again");
            //}

            //Console.WriteLine($"Choice {choice}");

            //switch (choice)
            //{
            //    case 1:
            //        Console.WriteLine("Round Robin");
            //        break;
            //    case 2:
            //        Console.WriteLine("Priority");
            //        break;
            //    default:
            //        Console.WriteLine("Invalid choice");
            //        break;
            //}

            while (true)
            {
                Socket acceptedSocket = ServerFunctions.InitialiseServersideCommunication();

                if (acceptedSocket == null)
                {
                    Console.WriteLine("[ERROR] Connection failed");
                    return;
                }

                //byte[] acceptedBuffer;
                //int receivedBytes;
                //string receivedMessage;

                // The communication may now ensue, this is just a test
                //try
                //{
                //    acceptedBuffer = new byte[4096];
                //    receivedBytes = acceptedSocket.Receive(acceptedBuffer);
                //    receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                //    Console.WriteLine($"[INFO] Received: {receivedMessage}");
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("[ERROR] Failed to receive the initial message from the client.");
                //    Console.WriteLine($"[EXCEPTION] {e}");
                //    return;
                //}

                int retVal = -2;

                while (true)
                {
                    retVal = ServerFunctions.ReceiveProcesses(acceptedSocket);
                    if (retVal == -1 || retVal == 2 || retVal == 1)
                    {
                        break;
                    }
                }

                if (retVal == 2)
                {
                    break;
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press any key (on the keyboard) to exit.");
            Console.ResetColor();

            //i am baffled by the stupidity of this
            //but either it's my fault or the fault of the console
            //that it doesn't flush the keys properly
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            Console.ReadKey();
        }

    }
}
