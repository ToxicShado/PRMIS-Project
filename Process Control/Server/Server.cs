using System.Net.Sockets;
using System.Net;
using System.Text;
using Process;

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

            while (true)
            {
                Socket acceptedSocket = initialiseCommunication();

                if (acceptedSocket == null)
                {
                    Console.WriteLine("Connection failed");
                    return;
                }

                // The communication may now ensue, this is just a test
                byte[] acceptedBuffer = new byte[1024];
                int receivedBytes = acceptedSocket.Receive(acceptedBuffer);
                string receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                Console.WriteLine($"Received: {receivedMessage}");

                while(receivedMessage != "END") { 
                    receivedBytes = acceptedSocket.Receive(acceptedBuffer);

                    Console.WriteLine($"bytes? : {receivedBytes}");

                    receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                    Console.WriteLine($"Received: {receivedMessage}");

                    OSProcess process = new OSProcess();
                    process = process.toProcess(acceptedBuffer, receivedBytes);
                    Console.WriteLine(process.ToString());
                }

                acceptedSocket.Close();
            }



            Console.ReadKey();
        }


        public static Socket initialiseCommunication()
        {
            Socket initialConnection;
            IPEndPoint serverEP;

            try
            {
                initialConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create the UDP socket.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            try
            {
                serverEP = new IPEndPoint(IPAddress.Any, 25565);
                initialConnection.Bind(serverEP);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to bind the UDP socket to the endpoint (0.0.0.0:25565).");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            Socket tcpSocket;
            IPEndPoint tcpServerEP;

            try
            {
                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create the TCP socket.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            try
            {
                tcpServerEP = new IPEndPoint(IPAddress.Any, 0);
                tcpSocket.Bind(tcpServerEP);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to bind the TCP socket to an endpoint.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            IPEndPoint tcpSocketEP;
            try
            {
                tcpSocketEP = (IPEndPoint)tcpSocket.LocalEndPoint;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to retrieve the local endpoint of the TCP socket.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            byte[] buffer = new byte[1024];
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            int received;

            try
            {
                received = initialConnection.ReceiveFrom(buffer, ref clientEP);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to receive the initial message from the client.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            string message;
            try
            {
                message = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"Received: {message}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to decode the received message.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            try
            {
                string response = $"{tcpSocketEP.Address.ToString()},{tcpSocketEP.Port}";
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                initialConnection.SendTo(responseData, clientEP);
                initialConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send the response back to the client.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            Socket acceptedSocket;
            try
            {
                tcpSocket.Listen();
                acceptedSocket = tcpSocket.Accept();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to accept the TCP connection from the client.");
                Console.WriteLine($"Exception: {e}");
                return null;
            }

            return acceptedSocket;
        }
    }
}
