using Process;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Client
{
    public class Client
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("[STATUS] Initializing client-side connection...");
            Socket tcpSocket = InitialiseClientsideConnection();

            if (tcpSocket == null)
            {
                Console.WriteLine("[ERROR] Connection failed");
                return;
            }

            // Test the connection by sending a message to the server
            string message = "I Am Aliveeee";
            byte[] messageData = new byte[4096];
            messageData = Encoding.UTF8.GetBytes(message);
            tcpSocket.Send(messageData);
            Console.WriteLine("[INFO] Sent initial message to server.");



            //OSProcess process = OperationsOnOSProcess.CreateProcess(); // This should be the way of  creating a process, but i dont feel like typing all the data in the console every. single. time.

            // so i will just create a processes like this "for now".
            List<OSProcess> processes = OSProcess.GenerateNProcesses(5, true);

            SendProcessesToServer(processes, tcpSocket);


            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPress any key (on the keyboard) to exit.");
            Console.ResetColor();
            Console.ReadKey();
        }


        public static bool SendProcessesToServer(List<OSProcess> processes, Socket tcpSocket)
        {
            Random random = new Random();

            tcpSocket.Send(processes[0].ConvertProcessTotoBytecodeCSV());
            Console.WriteLine($"[STATUS] Sent first process {processes[0]} to server.");

            byte [] messageData = new byte[4096];
            tcpSocket.Receive(messageData);
            string receivedMessage = Encoding.UTF8.GetString(messageData);
            do
            {
                Console.WriteLine();
                Console.WriteLine($"[INFO] Received: {receivedMessage}");
                if (receivedMessage.StartsWith("OK"))
                {
                    Console.WriteLine($"[STATUS] Process {processes[0]} sent successfully");
                    processes.Remove(processes[0]);
                    receivedMessage = "NEXT";
                }
                else if (receivedMessage.StartsWith("ERR_0"))
                {
                    Console.WriteLine($"[ERROR] Failed to send process {processes[0]}");
                    Task.Delay(random.Next(100, 2000)).Wait();
                    tcpSocket.Send(processes[0].ConvertProcessTotoBytecodeCSV());
                    Console.WriteLine($"[STATUS] Retrying to send process {processes[0]}.");

                    messageData = new byte[4096];
                    tcpSocket.Receive(messageData);

                    receivedMessage = "";
                    receivedMessage = Encoding.UTF8.GetString(messageData);
                }
                else if (receivedMessage.StartsWith("NEXT"))
                {
                    tcpSocket.Send(processes[0].ConvertProcessTotoBytecodeCSV());
                    Console.WriteLine($"[STATUS] Sending next process {processes[0]}.");

                    messageData = new byte[4096];
                    tcpSocket.Receive(messageData);

                    receivedMessage = "";
                    receivedMessage = Encoding.UTF8.GetString(messageData);
                }
                Console.WriteLine();
                Task.Delay(500).Wait();
            } while (processes.Count > 0);

            tcpSocket.Send(Encoding.UTF8.GetBytes("EXIT"));
            Console.WriteLine("[STATUS] All processes sent. Connection should be closed by Server.");
            //tcpSocket.Close();

            return true;
        }    

        public static Socket InitialiseClientsideConnection()
        {
            // Create a UDP socket for initiating a connection
            Socket udpSocket;
            IPEndPoint serverEP;
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                serverEP = new IPEndPoint(IPAddress.Loopback, 25565);
                Console.WriteLine("[STATUS] UDP socket created.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Unable to open a UDP socket on IPAddress: 127.0.0.1 and Port: 25565.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            // Send a registration message to the server
            try
            {
                string registrationMessage = "Could You Please Connect?";
                byte[] registrationData = Encoding.UTF8.GetBytes(registrationMessage);
                udpSocket.SendTo(registrationData, serverEP);
                Console.WriteLine("[STATUS] Sent registration message to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Unable to send registration message to server at {serverEP.Address}:{serverEP.Port}.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            // Create a TCP socket for the actual communication
            Socket tcpSocket;
            IPEndPoint tcpServerEP;

            try
            {
                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpServerEP = new IPEndPoint(IPAddress.Loopback, 0);
                Console.WriteLine("[STATUS] TCP socket created.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Unable to open a TCP socket.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            // Receive the IP and port of the TCP socket from the server
            EndPoint serverResponseEndpoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] buffer = new byte[4096];
            int receivedBytes = 0;
            try
            {
                receivedBytes = udpSocket.ReceiveFrom(buffer, ref serverResponseEndpoint);
                Console.WriteLine("[STATUS] Received server response.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Unable to receive response from the server.");
                Console.WriteLine($"[EXCEPTION] {e}");
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
                Console.WriteLine("[ERROR] Failed to parse the IP address.");
                return null;
            }
            tcpServerEP.Address = iPAddress;

            saccess = int.TryParse(serverResponseParts[1], out port);
            if (!saccess || port == -1)
            {
                Console.WriteLine("[ERROR] Failed to parse the port.");
                return null;
            }
            tcpServerEP.Port = port;
            Console.WriteLine($"[INFO] Server response parsed. Address: {tcpServerEP.Address}, Port: {tcpServerEP.Port}");

            // Connect to the server with the provided data
            try
            {
                tcpSocket.Connect(tcpServerEP);
                Console.WriteLine("[STATUS] Successfully connected to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Unable to connect to server at {tcpServerEP.Address}:{tcpServerEP.Port}.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            return tcpSocket;
        }
    }
}
