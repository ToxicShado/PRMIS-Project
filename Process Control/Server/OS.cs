using Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    Thread.Sleep(100);
                }
            }) { IsBackground = true };

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
                        processorState -= process.Item1.processor;
                        memoryState -= process.Item1.memory;
                        Console.WriteLine($"Process {process.ToString()} removed.");
                        Console.WriteLine($"[NEW] OS State => Processor : {processorState} Memory : {memoryState}");
                    }
                }
                foreach (Tuple<OSProcess, DateTime> process in ProccessesToRemove)
                {
                    RunningProcesses.Remove(process);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

    }
}


