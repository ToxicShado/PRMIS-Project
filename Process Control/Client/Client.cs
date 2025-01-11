using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using Process;


namespace Client
{
    public class Client
    {
        public static void Main(string[] args)
        {

            Socket tcpSocket = initialiseConnection();

            if (tcpSocket == null)
            {
                Console.WriteLine("Connection failed");
                return;
            }

            // Test the connection by sending a message to the server
            string message = "I Am Aliveeee";
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            tcpSocket.Send(messageData);

            OSProcess process = createProcess();

            Console.WriteLine(Encoding.UTF8.GetChars(process.toCSV()));


            tcpSocket.Send(process.toCSV());

            tcpSocket.Close();

            Console.ReadKey();
        }

        public static OSProcess createProcess()
        {
            string name = "";
            int timeToComplete = 0;
            int priority = 0;
            double memory = 0;
            double processor = 0;

            Console.WriteLine("Creating a new process. Please enter the following details:");

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter process name: ");
                name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    break;
                }
                Console.WriteLine("Process name cannot be empty. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter time to complete (positive integer): ");
                if (int.TryParse(Console.ReadLine(), out timeToComplete) && timeToComplete > 0)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Time to complete must be a positive integer. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter priority (positive integer): ");
                if (int.TryParse(Console.ReadLine(), out priority) && priority > 0)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Priority must be a positive integer. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter memory usage (positive double): ");
                if (double.TryParse(Console.ReadLine(), out memory) && memory > 0)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Memory usage must be a positive number. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter processor usage (positive double): ");
                if (double.TryParse(Console.ReadLine(), out processor) && processor > 0)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Processor usage must be a positive number. Please try again.");
            }

            Console.WriteLine("Process creation successful! (at last!)"); // maybe, just maybe, we should make some kind of an autofill
            // and boy, are these try catches making the code be unreadable
            return new OSProcess(name, timeToComplete, priority, memory, processor);
        }

        public static Socket initialiseConnection()
        {
            // Create a UDP socket for initiating a connection
            Socket udpSocket;
            IPEndPoint serverEP;
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                serverEP = new IPEndPoint(IPAddress.Loopback, 25565);
            }
            catch (Exception e)
            {
                Console.WriteLine($"For some reason we're unable to open a socket and make and EndPoint on IPAdress : 127.0.0.1 and Port 25565 ");
                Console.WriteLine($"Exception {e}");
                return null;
            }

            // Send a registration message to the server
            try
            {
                string registrationMessage = "Could You Please Connect?";
                byte[] registrationData = Encoding.UTF8.GetBytes(registrationMessage);
                udpSocket.SendTo(registrationData, serverEP);
            }
            catch (Exception e)
            {
                Console.WriteLine($"For some reason we're unable to send a message to the server with adress {serverEP.Address.ToString()} and port {serverEP.Port}");
                Console.WriteLine($"Exception {e}");
                return null;
            }

            // Create a TCP socket for the actual communication
            Socket tcpSocket;
            IPEndPoint tcpServerEP;

            try
            {
                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpServerEP = new IPEndPoint(IPAddress.Loopback, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"For some reason we're unable to open a socket and make and EndPoint");
                Console.WriteLine($"Exception {e}");
                return null;
            }


            // Receive the IP and port of the TCP socket from the server
            EndPoint serverResponseEndpoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] buffer = new byte[1024];
            int receivedBytes = 0;
            try
            {
                receivedBytes = udpSocket.ReceiveFrom(buffer, ref serverResponseEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine($"For some reason we're unable to receive a message from the server");
                Console.WriteLine($"Exception {e}");
                return null;
            }
            udpSocket.Close();

            // Parse the received data
            string serverResponse = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            string[] serverResponseParts = serverResponse.Split(',');

            int port = -1;

            IPAddress? iPAddress = null;

            bool saccess = IPAddress.TryParse(serverResponseParts[0] == "0.0.0.0" ? "127.0.0.1" : serverResponseParts[0], out iPAddress);

            if (!saccess || iPAddress == null)
            {
                Console.WriteLine("Failed to parse the IP address");
                return null;
            }
            tcpServerEP.Address = iPAddress;

            saccess = int.TryParse(serverResponseParts[1], out port);
            if (!saccess || port == -1)
            {
                Console.WriteLine("Failed to parse the port");
                return null;
            }
            tcpServerEP.Port = port;
            Console.WriteLine(serverResponse);

            // Connect to the server with the provided data
            try
            {
                tcpSocket.Connect(tcpServerEP);
            }
            catch (Exception e)
            {
                Console.WriteLine($"For some reason we're unable to connect to the server with adress {tcpServerEP.Address.ToString()} and port {tcpServerEP.Port}");
                Console.WriteLine($"Exception {e}");
                return null;
            }
            return tcpSocket;
        }
    }
}
