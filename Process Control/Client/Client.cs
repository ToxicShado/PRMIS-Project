using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    public class Client
    {
        public static void Main(string[] args)
        {
            // Create a UDP socket for initiating a connection
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 25565);

            // Send a registration message to the server
            byte[] buffer = new byte[1024];
            string registrationMessage = "CouldYouPleaseConnect";
            byte[] registrationData = Encoding.UTF8.GetBytes(registrationMessage);
            udpSocket.SendTo(registrationData, serverEP);

            // Create a TCP socket for the actual communication
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint tcpServerEP = new IPEndPoint(IPAddress.Loopback, 0);

            // Receive the IP and port of the TCP socket from the server
            EndPoint serverResponseEndpoint = new IPEndPoint(IPAddress.Any, 0);
            int receivedBytes = udpSocket.ReceiveFrom(buffer, ref serverResponseEndpoint);

            udpSocket.Close();

            // Parse the received data
            string serverResponse = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            string[] serverResponseParts = serverResponse.Split(',');
            tcpServerEP.Address = IPAddress.Parse(serverResponseParts[0] == "0.0.0.0" ? "127.0.0.1" : serverResponseParts[0]);
            tcpServerEP.Port = int.Parse(serverResponseParts[1]);
            Console.WriteLine(serverResponse);

            // Connect to the server with the provided data
            tcpSocket.Connect(tcpServerEP);

            // Test the connection by sending a message to the server
            string message = "Hello from the client";
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            tcpSocket.Send(messageData);
            tcpSocket.Close();

            Console.ReadKey();
        }

    }
}
