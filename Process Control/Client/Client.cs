using OSProcesses;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public class Client
    {
        public static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.WriteLine("[STATUS] Initializing client-side connection...");
            Random random = new Random();

            Socket tcpSocket = ClientFunctions.InitialiseClientsideConnectionWithMultipleRetries();
            ClientFunctions.AutomaticallyCloseClient();
            if (tcpSocket == null)
            {
                Console.WriteLine("[ERROR] Connection failed");
                return;
            }

            List<OSProcess> processes = new List<OSProcess>();

            Console.WriteLine("Choose process creation mode:");
            Console.WriteLine("1 - Manually create each process");
            Console.WriteLine("2 - Automatically generate processes");
            Console.WriteLine("Or wait 10 seconds to automatically generate 10 processes.");

            int mode = 0;
            bool modeSelected = false;
            DateTime startTime = DateTime.Now;

            Console.Write("Enter 1 or 2: ");

            long num = 0;
            bool autoDefault = false;

            // Wait for user input for up to 10 seconds, otherwise default to automatic mode with 10 processes
            while (!modeSelected)
            {
                if (Console.KeyAvailable)
                {
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out mode) && (mode == 1 || mode == 2))
                    {
                        modeSelected = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 1 or 2.");
                    }
                }
                else if ((DateTime.Now - startTime).TotalSeconds >= 10)
                {
                    mode = 2;
                    num = 10;
                    modeSelected = true;
                    autoDefault = true;
                    Console.WriteLine("\n[INFO] No input detected. Automatically generating 10 processes.");
                    break;
                }
                Thread.Sleep(100); // Prevent busy waiting
            }

            if (!autoDefault)
            {
                Console.WriteLine("Enter the number of processes you want to create and send to the server:");
                bool read = false;
                do
                {
                    read = Int64.TryParse(Console.ReadLine(), out num);
                    if (read)
                    {
                        if (num <= 0)
                            Console.WriteLine("Please enter a positive number.");
                    }
                    else
                        Console.WriteLine("Invalid input. Please enter a positive integer.");
                }
                while (num <= 0 || !read);
            }

            if (mode == 1)
            {
                for (int i = 0; i < num; i++)
                {
                    OSProcess process = OperationsOnOSProcess.CreateProcess();
                    if (process != null)
                    {
                        processes.Add(process);
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Failed to create a process. Please try again.");
                        i--; // Decrement i to retry creating the process
                    }
                }
            }
            else
            {
                processes = OperationsOnOSProcess.GenerateNProcesses((int)num, true);
            }

            ClientFunctions.SendProcessesToServer(processes, tcpSocket);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPress any key (on the keyboard) to exit.");
            Console.ResetColor();

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo k = Console.ReadKey(true);
            }
            Console.ReadKey();
        }
    }
}
