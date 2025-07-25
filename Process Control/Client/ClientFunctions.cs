﻿using OSProcesses;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class ClientFunctions
    {
        public static int ServerPID { get; private set; } = -1;
        public static bool SendProcessesToServer(List<OSProcess> processes, Socket tcpSocket)
        {
            Random random = new Random();

            tcpSocket.Send(processes[0].ToMemoryPack());
            Console.WriteLine($"[STATUS] Sent first process {processes[0]} to server.");

            byte[] messageData = new byte[4096];
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
                    tcpSocket.Send(processes[0].ToMemoryPack());
                    Console.WriteLine($"[STATUS] Retrying to send process {processes[0]}.");

                    messageData = new byte[4096];
                    tcpSocket.Receive(messageData);

                    receivedMessage = "";
                    receivedMessage = Encoding.UTF8.GetString(messageData);
                }
                else if (receivedMessage.StartsWith("NEXT"))
                {
                    tcpSocket.Send(processes[0].ToMemoryPack());
                    Console.WriteLine($"[STATUS] Sending next process {processes[0]}.");

                    messageData = new byte[4096];
                    tcpSocket.Receive(messageData);

                    receivedMessage = "";
                    receivedMessage = Encoding.UTF8.GetString(messageData);
                }
                Console.WriteLine();
                Task.Delay(500).Wait();
            } while (processes.Count > 0);

            tcpSocket.Send(Encoding.UTF8.GetBytes("END"));
            Console.WriteLine("[STATUS] All processes sent. Connection should be closed by Server.");
            //tcpSocket.Close();

            return true;
        }

        public static Socket InitialiseClientsideConnectionWithMultipleRetries()
        {
            Random random = new Random();
            Socket tcpSocket = null;

            int i = 0;

            while (tcpSocket == null)
            {
                tcpSocket = InitialiseClientsideConnection();
                if (tcpSocket == null)
                {
                    Console.WriteLine("[ERROR] Connection failed. Retrying...");
                    if (i < 5)
                    {
                        Task.Delay(random.Next(100, 2000)).Wait();
                    }
                    else if (i < 8)
                    {
                        Task.Delay(random.Next(2000, 5000)).Wait();
                    }
                    else if (i < 15)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("At this point, a connection seems unlikely.. So, click Escape to exit the program");
                        Console.ResetColor();

                        while (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                Console.WriteLine("Escape pressed. Exiting...");
                                return null;
                            }
                        }
                        Task.Delay(random.Next(5000, 10000)).Wait();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("At this point, a connection seems unlikely.. So, click Escape to exit the program");
                        Console.ResetColor();
                        while (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                Console.WriteLine("Escape pressed. Exiting...");
                                return null;
                            }
                        }
                        Task.Delay(random.Next(5000, 100000)).Wait();
                    }
                }
                else
                {
                    return tcpSocket;
                }
                i++;
            }

            return null;
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

            byte[] messageData = new byte[4096];
            tcpSocket.Receive(messageData);
            ServerPID = int.Parse(Encoding.UTF8.GetString(messageData));

            return tcpSocket;
        }

        public static void AutomaticallyCloseClient()
        {
            if (ServerPID == -1)
            {
                return;
            }
            Thread thread = new Thread(() => CloseClient()) { IsBackground = true };
            thread.Start();
        }

        public static void CloseClient()
        {
            Thread.Sleep(1000);
            while (true)
            {
                try
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(ServerPID);
                }
                catch
                {
                    Environment.Exit(0);
                    return;
                }
                Thread.Sleep(100);
            }
        }
    }
}
