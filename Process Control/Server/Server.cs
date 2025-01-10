using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server
{
    public class Server
    {
        public static void Main(string[] args)
        {
            double procesorState = 0;
            double memoryState = 0;

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

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Round Robin");
                    break;
                case 2:
                    Console.WriteLine("Priority");
                    break;
                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }

            while (true) { 
                Socket acceptedSocket = initialiseCommunication();

                // The communication may now ensue, this is just a test
                byte[] acceptedBuffer = new byte[1024];
                int receivedBytes = acceptedSocket.Receive(acceptedBuffer);
                string receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                Console.WriteLine($"Received: {receivedMessage}");
                acceptedSocket.Close();
            }



            Console.ReadKey();
        }


        public static Socket initialiseCommunication()
        {
            // Create a UDP socket for initiating a connection
            Socket initialConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 25565);
            initialConnection.Bind(serverEP);

            // Create a TCP socket for the actual communication
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint tcpServerEP = new IPEndPoint(IPAddress.Any, 0);
            tcpSocket.Bind(tcpServerEP);

            // Get the IP and port of the TCP socket so that we can forward it to the client
            IPEndPoint iPEndPoint = (IPEndPoint)tcpSocket.LocalEndPoint;

            // Receive the initial message from the client
            byte[] buffer = new byte[1024];
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            int received = initialConnection.ReceiveFrom(buffer, ref clientEP);
            string message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Received: {message}");

            // Assemble the IP and port of the TCP socket and send it back to the client
            string response = $"{((IPEndPoint)iPEndPoint).Address.ToString()},{((IPEndPoint)iPEndPoint).Port}";
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            initialConnection.SendTo(responseData, clientEP);
            initialConnection.Close();

            // Accept the TCP connection from the client
            tcpSocket.Listen();
            Socket acceptedSocket = tcpSocket.Accept();

            return acceptedSocket;
        }
    }
}
