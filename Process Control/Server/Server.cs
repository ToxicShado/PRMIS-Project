using Process;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Server
    {
        private static OS OS = OS.getInstance();
        public static void Main(string[] args)
        {
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
                Socket acceptedSocket = InitialiseServersideCommunication();

                if (acceptedSocket == null)
                {
                    Console.WriteLine("[ERROR] Connection failed");
                    return;
                }

                byte[] acceptedBuffer;
                int receivedBytes;
                string receivedMessage;

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
                    retVal = ReceiveProcesses(acceptedSocket);
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
            Console.ReadKey();
        }

        public static int ReceiveProcesses(Socket acceptedSocket)
        {
            byte[] acceptedBuffer = new byte[4096];
            int receivedBytes;
            string receivedMessage;
            try
            {
                receivedBytes = acceptedSocket.Receive(acceptedBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to receive the message from the client."); // connection likely closed?
                Console.WriteLine($"[EXCEPTION] {e}");
                receivedBytes = 0;
                return -1;
            }

            if (receivedBytes > 0) // if there are no bytes to receive, then we cannot make a process
            {
                receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                if (receivedMessage == "END")
                {
                    Console.WriteLine($"[INFO] Received: {receivedMessage}");
                    Console.WriteLine("[STATUS] Connection closed by Client request successfully");
                    acceptedSocket.Close();
                    return 1;
                }
                else if (receivedMessage == "EXIT")
                {
                    Console.WriteLine($"[INFO] Received: {receivedMessage}");
                    Console.WriteLine("[STATUS] Connection closed by Client request successfully and the program should exit after you press any button.");
                    acceptedSocket.Close();
                    return 2;
                }
                else
                {
                    OSProcess process = OperationsOnOSProcess.toProcess(acceptedBuffer, receivedBytes);
                    Console.WriteLine($"\n[INFO] Received process details: {process}");
                    if (OS.IsTherePlaceForNewProcess(process))
                    {
                        OS.AddNewProcess(process);
                        acceptedSocket.Send(Encoding.UTF8.GetBytes("OK : Process added successfully"));
                        Console.WriteLine($"[STATUS] Process {process} added successfully");
                    }
                    else
                    {
                        Console.WriteLine("[INFO] Process cannot be added due to resource constraints");
                        acceptedSocket.Send(Encoding.UTF8.GetBytes("ERR_0 : Process cannot be added due to resource constraints"));
                    }
                    Console.WriteLine();
                }
                   
                
            }
            return 0;
        }


        public static Socket InitialiseServersideCommunication()
        {
            Socket initialConnection;
            IPEndPoint serverEP;

            Console.WriteLine();
            try
            {
                initialConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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
                serverEP = new IPEndPoint(IPAddress.Any, 25565);
                initialConnection.Bind(serverEP);
                Console.WriteLine("[STATUS] Server is ready and awaiting a new connection.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to bind the UDP socket to the endpoint (127.0.0.1:25565).");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            Socket tcpSocket;
            IPEndPoint tcpServerEP;

            try
            {
                tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("[STATUS] TCP socket created.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to create the TCP socket.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            try
            {
                tcpServerEP = new IPEndPoint(IPAddress.Any, 0);
                tcpSocket.Bind(tcpServerEP);
                Console.WriteLine("[STATUS] TCP socket bound to endpoint.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to bind the TCP socket to an endpoint.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            IPEndPoint tcpSocketEP;
            try
            {
                tcpSocketEP = tcpSocket.LocalEndPoint as IPEndPoint;
                string ip = (tcpSocketEP.Address.ToString() == "0.0.0.0") ? "127.0.0.1" : tcpSocketEP.Address.ToString();
                Console.WriteLine($"[INFO] TCP socket local endpoint: {ip}:{tcpSocketEP.Port}");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to retrieve the local endpoint of the TCP socket.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            byte[] buffer = new byte[4096];
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            int received;

            try
            {
                received = initialConnection.ReceiveFrom(buffer, ref clientEP);
                Console.WriteLine("[STATUS] Message received from client.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to receive the initial message from the client.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            string message;
            try
            {
                message = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"[INFO] Received: {message}");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to decode the received message.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            try
            {
                string response = $"{tcpSocketEP.Address},{tcpSocketEP.Port}";
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                initialConnection.SendTo(responseData, clientEP);
                initialConnection.Close();
                Console.WriteLine("[STATUS] Response sent to client and UDP socket closed.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to send the response back to the client.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }

            Socket acceptedSocket;
            try
            {
                tcpSocket.Listen();
                Console.WriteLine("[STATUS] TCP socket listening for client connection...");
                acceptedSocket = tcpSocket.Accept();
                Console.WriteLine("[STATUS] Client connection accepted.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Failed to accept the TCP connection from the client.");
                Console.WriteLine($"[EXCEPTION] {e}");
                return null;
            }
            Console.WriteLine();
            return acceptedSocket;
        }
    }
}
