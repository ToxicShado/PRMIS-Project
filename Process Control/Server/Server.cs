using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        private static List<Socket> clientSockets = new List<Socket>();
        private static object socketListLock = new object();

        private static volatile bool isRunning = true;

        private static DateTime lastActivity = DateTime.Now;
        const int SecondsToWaitForActivity = 30;

        public static void Main(string[] args)
        {
            // Open an instance of Task Manager, and connect to it.

            Console.Title = "Server";

            OS OS = OS.getInstance();
            Console.WriteLine("[STATUS] Opening Task Manager...");

            if (OS.OpenTaskManagerAndConnectToIt() == false)
            {
                Console.WriteLine("[ERROR] Failed to open Task Manager.");
                return;
            }
            Console.WriteLine();

            OS.PickScheduling();

            Console.WriteLine();

            Console.WriteLine("[STATUS] Starting server...");

            // Thread 1: Accept new clients
            Thread acceptThread = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        Socket acceptedSocket = ServerFunctions.InitialiseServersideCommunication();

                        acceptedSocket.Blocking = false;
                        lock (socketListLock)
                        {
                            clientSockets.Add(acceptedSocket);
                        }
                        Console.WriteLine("[INFO] New client connected.");
                    }
                    catch (SocketException ex)
                    {
                        // WouldBlock means no pending connections, just continue
                        if (ex.SocketErrorCode != SocketError.WouldBlock)
                        {
                            Console.WriteLine("[ERROR] Accept failed: " + ex.Message);
                        }
                        Thread.Sleep(100); // Avoid busy loop
                    }
                }
            })
            { IsBackground = true };
            acceptThread.Start();

            // Thread 2: Listen for data on all clients
            Thread listenThread = new Thread(() =>
            {
                int timeToComplete = -1;

                while (isRunning)
                {
                    List<Socket> readSockets;
                    lock (socketListLock)
                    {
                        readSockets = clientSockets.Where(s => s.Connected).ToList();
                    }

                    if (readSockets.Count == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    // Use Select to find sockets with data
                    Socket.Select(readSockets, null, null, 1000000); // 1 second timeout

                    if (readSockets.Count > 0)
                    {
                        lastActivity = DateTime.Now.AddMilliseconds(timeToComplete); // Update last activity time with offset
                    }

                    foreach (var clientSocket in readSockets)
                    {
                        int retVal = ServerFunctions.ReceiveProcesses(clientSocket);
                        if (retVal == -1 || retVal == -10 || retVal == -20)
                        {
                            // Remove closed/disconnected socket
                            lock (socketListLock)
                            {
                                clientSockets.Remove(clientSocket);
                            }
                        }
                        else
                        {
                            timeToComplete = retVal;
                        }
                    }
                }
            })
            { IsBackground = true };
            listenThread.Start();

            // Thread 3: Start N clients
            int numberOfClients = 1; // Or any N you want
            Thread clientThread = new Thread(() => ServerFunctions.StartNClients(numberOfClients)) { IsBackground = true };
            clientThread.Start();

            // Thread 4: Monitor inactivity
            Thread monitorThread = new Thread(() =>
            {
                while (isRunning)
                {
                    if ((DateTime.Now - lastActivity).TotalSeconds > SecondsToWaitForActivity)
                    {
                        isRunning = false;
                        break;
                    }
                    Thread.Sleep(500);
                }
                Console.WriteLine("[INFO] No activity for " + SecondsToWaitForActivity + " seconds. Shutting down server...");
                Thread.Sleep(2000);
                //Console.Clear();
                Console.WriteLine("[STATUS] Printing statistics...");
                Thread.Sleep(2000);
                //Console.Clear();
                Console.WriteLine("All processing took " + OS.totalProcessDuration / 1000.0f + " seconds.");

                Environment.Exit(0);

            })
            { IsBackground = true };
            monitorThread.Start();

            acceptThread.Join();
            listenThread.Join();
            monitorThread.Join();

        }

    }
}
