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
            Console.WriteLine("\n==================================================================================================");

            if (RunningProcesses.Count == 0)
            {
                Console.WriteLine("| No processes are currently running.                                                            |");
                Console.WriteLine("==================================================================================================");
            }
            else
            {
                Console.WriteLine("|                                 Currently Running Process List                                 |");
                Console.WriteLine("==================================================================================================");

                // Print the table header
                Console.WriteLine(string.Format("| {0,-20} | {1,-15} | {2,-16} | {3,-10} | {4,-10} | {5,-8}|",
            "Name", "Added On", "Time to Complete", "Priority", "Memory", "Processor"));
                Console.WriteLine("--------------------------------------------------------------------------------------------------");

                // Print each process as a row in the table
                foreach (var runningProcess in RunningProcesses)
                {
                    Console.WriteLine(string.Format("| {0,-20} | {1,-15} | {2,-16} | {3,-10} | {4,-10} | {5,-9}|",
                        runningProcess.Item1.name,
                        runningProcess.Item2.ToShortTimeString(),
                        runningProcess.Item1.timeToComplete,
                        runningProcess.Item1.priority,
                        runningProcess.Item1.memory,
                        runningProcess.Item1.processor));
                }

                Console.WriteLine("==================================================================================================");
            }

            // Print OS state summary
            Console.WriteLine(string.Format("| {0,-94} |", "OS State => Processor: " + processorState + "% , Memory: " + memoryState + "%"));
            Console.WriteLine("==================================================================================================\n");
        }



    }
}


