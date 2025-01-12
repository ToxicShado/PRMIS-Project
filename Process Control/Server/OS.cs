using Process;
using System.Diagnostics;

namespace Server
{
    internal class OS
    {
        public double processorState { get; private set; }
        public double memoryState { get; private set; }
        public List<Tuple<OSProcess, DateTime>> RunningProcesses { get; private set; }
        private static OS instance = null;
        private readonly Mutex mutex;

        private OS()
        {
            processorState = 0;
            memoryState = 0;
            RunningProcesses = new List<Tuple<OSProcess, DateTime>>();
            mutex = new Mutex();

            Thread backgroundThread = new Thread(() =>
            {
                while (true)
                {
                    removeProcessIfFinished();
                    Thread.Sleep(50);
                }
            })
            { IsBackground = true };

            backgroundThread.Start();
        }

        public static OS getInstance()
        {
            if (instance == null)
            {
                instance = new OS();
            }
            return instance;
        }

        public bool isTherePlaceForNewProcess(OSProcess process)
        {
            bool result = false;
            try
            {
                mutex.WaitOne();
                if (processorState + process.processor <= 100 && memoryState + process.memory <= 100)
                {
                    result = true;
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return result;

        }

        public void AddNewProcess(OSProcess process)
        {
            try
            {
                mutex.WaitOne();
                RunningProcesses.Add(new Tuple<OSProcess, DateTime>(process, DateTime.Now));
                processorState += process.processor;
                memoryState += process.memory;
                Console.WriteLine($"[OS] Process {process.ToString()} has started running.");
                PrintCurrentlyRunningProcesses();
            }
            finally
            {
                mutex.ReleaseMutex();
            }

        }

        public void removeProcessIfFinished()
        {
            try
            {
                mutex.WaitOne();
                var ProccessesToRemove = new List<Tuple<OSProcess, DateTime>>();
                foreach (Tuple<OSProcess, DateTime> process in RunningProcesses)
                {
                    if (DateTime.Now - process.Item2 > TimeSpan.FromMilliseconds(process.Item1.timeToComplete))
                    {
                        ProccessesToRemove.Add(process);
                    }
                }
                foreach (Tuple<OSProcess, DateTime> process in ProccessesToRemove)
                {
                    RunningProcesses.Remove(process);
                    processorState -= process.Item1.processor;
                    memoryState -= process.Item1.memory;
                    Console.WriteLine($"[OS] Process {process.ToString()} has stopped running.");
                }
                if (ProccessesToRemove.Count > 0)
                    PrintCurrentlyRunningProcesses();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void PrintCurrentlyRunningProcesses()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                          THE TASK MANAGER                              _   \u25A1   ×   ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("╟════════════════════════════════════════════════════════════════════════════════════════════════════╢");
            Console.WriteLine("║                                  Currently Running Process List                                    ║");
            if (RunningProcesses.Count == 0)
            {
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║                                 No processes are currently running                                 ║");
                Console.WriteLine("║                          Tip: Add a process to make the system productive                          ║");
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");

            }
            else
            {
                Console.WriteLine("╠══════════════════════╤═════════════════╤══════════════════╤════════════╤════════════╤══════════════╣");
                // Print the table header
                Console.WriteLine(string.Format("║ {0,-20} │ {1,-15} │ {2,-16} │ {3,-10} │ {4,-10} │ {5,-12} ║",
                    "Name", "Added On", "Time to Complete", "Priority", "Memory", "Processor"));
                Console.WriteLine("╟──────────────────────┼─────────────────┼──────────────────┼────────────┼────────────┼──────────────╢");

                // Print each process as a row in the table
                foreach (var runningProcess in RunningProcesses)
                {
                    Console.WriteLine(string.Format("║ {0,-20} │ {1,-15} │ {2,-16} │ {3,-10} │ {4,-10} │ {5,-12} ║",
                        runningProcess.Item1.name,
                        runningProcess.Item2.ToLongTimeString(),
                        runningProcess.Item1.timeToComplete.ToString() + "ms",
                        runningProcess.Item1.priority,
                        runningProcess.Item1.memory.ToString() + "%",
                        runningProcess.Item1.processor.ToString() + "%"));
                }

                Console.WriteLine("╟══════════════════════╧═════════════════╧══════════════════╧════════════╧════════════╧══════════════╢");
            }

            // Print OS state summary
            Console.WriteLine("╟════════════════════════════════════════════════════════════════════════════════════════════════════╢");
            Console.WriteLine("║                                        The Current OS Usage                                        ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════════════╣");
            string processor = $"║ Processor Usage: [{new string('■', (int)(processorState / ((double)100 / 74)))}{new string('-', 74 - (int)(processorState / ((double)100 / 74)))}] {processorState}%"
                + (processorState < 10 ? "   ║" : (processorState == 100) ? " ║" : "  ║");
            Console.WriteLine(processor);
            string memory = $"║ Memory Usage   : [{new string('■', (int)(memoryState / ((double)100 / 74)))}{new string('-', 74 - (int)(memoryState / ((double)100 / 74)))}] {memoryState}%"
                + (memoryState < 10 ? "   ║" : (memoryState == 100) ? " ║" : "  ║");
            Console.WriteLine(memory);
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════════════════╝\n");
        }



    }
}


