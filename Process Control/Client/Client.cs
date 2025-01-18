using OSProcesses;
using System.Net.Sockets;


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

            if (tcpSocket == null)
            {
                Console.WriteLine("[ERROR] Connection failed");
                return;
            }

            // Test the connection by sending a message to the server
            //string message = "I Am Aliveeee";
            //byte[] messageData = new byte[4096];
            //messageData = Encoding.UTF8.GetBytes(message);
            //tcpSocket.Send(messageData);
            //Console.WriteLine("[INFO] Sent initial message to server.");


            //OSProcess process = OperationsOnOSProcess.CreateProcess(); // This should be the way of  creating a process, but i dont feel like typing all the data in the console every. single. time.
            // so i will just create a processes like this "for now".
            List<OSProcess> processes = OperationsOnOSProcess.GenerateNProcesses(5, true);

            ClientFunctions.SendProcessesToServer(processes, tcpSocket);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPress any key (on the keyboard) to exit.");
            Console.ResetColor();

            //WHY THE HELL IS THIS NECESSARY AND FLUSHING THE CONSOLE DOESNT WORK
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo k = Console.ReadKey(true);
                //Console.WriteLine($"Key that has been flushed {k}");
            }
            Console.ReadKey();
        }
    }
}
