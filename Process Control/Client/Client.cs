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
            int mode = 0;
            while (mode != 1 && mode != 2)
            {
                Console.Write("Enter 1 or 2: ");
                if (!int.TryParse(Console.ReadLine(), out mode) || (mode != 1 && mode != 2))
                {
                    Console.WriteLine("Invalid input. Please enter 1 or 2.");
                }
            }

            long num;
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
