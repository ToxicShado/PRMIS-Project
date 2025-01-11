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
                Socket acceptedSocket = initialiseServersideCommunication();

                if (acceptedSocket == null)
                {
                    Console.WriteLine("Connection failed");
                    return;
                }
                else
                {
                    Console.WriteLine("Connection successful");
                }

                byte[] acceptedBuffer;
                int receivedBytes;
                string receivedMessage;

                // The communication may now ensue, this is just a test
                try { 
                    acceptedBuffer = new byte[4096];
                    receivedBytes = acceptedSocket.Receive(acceptedBuffer);
                    receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                    Console.WriteLine($"Received: {receivedMessage}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to receive the initial message from the client.");
                    Console.WriteLine($"Exception: {e}");
                    return;
                }


                while (true)
                {

                    receivedBytes = acceptedSocket.Receive(acceptedBuffer);

                    if (receivedBytes > 0) // if there are no bytes to receive, then we cannot make a process
                    {
                        receivedMessage = Encoding.UTF8.GetString(acceptedBuffer, 0, receivedBytes);
                        Console.WriteLine($"Received: {receivedMessage}");

                        if (receivedMessage != "END") // if the communication is stopped, we should just jump over it
                        {
                            OSProcess process = OperationsOnOSProcess.toProcess(acceptedBuffer, receivedBytes);
                            Console.WriteLine(process.ToString());
                            if (OS.isTherePlaceForNewProcess(process))
                            {
                                OS.AddNewProcess(process);
                                acceptedSocket.Send(Encoding.UTF8.GetBytes("OK : Process added successfully\n"));
                            }
                            else
                            {
                                Console.WriteLine("Process cannot be added due to resource constraints");
                                acceptedSocket.Send(Encoding.UTF8.GetBytes("ERR_0 : Process cannot be added due to resource constraints\n"));
                            }
                        }
                        else
                        {
                            Console.WriteLine("Connection closed by Client request successfully");
                            acceptedSocket.Close();
                            break;
                        }

                        receivedBytes = 0;
                        acceptedBuffer = new byte[4096];
                        receivedMessage = "";
                        
                    }

                    // theoretically implement exiting the code, if the client sends "ENDALL"
                }
            }
        }


        public static Socket initialiseServersideCommunication()
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
                Console.WriteLine("Server is ready and awaiting a new connection.");
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

            byte[] buffer = new byte[4096];
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
