using Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class OS
    {
        public double processorState { get; private set; }
        public double memoryState { get; private set; }
        public List<Tuple<OSProcess, DateTime>> RunningProcesses { get; private set; }

        public OS()
        {
            processorState = 0;
            memoryState = 0;
            RunningProcesses = new List<Tuple<OSProcess, DateTime>>();
        }

        public bool isTherePlaceForNewProcess(OSProcess process)
        {
            if (processorState + process.processor <= 100 && memoryState + process.memory <= 100)
            {
                return true;
            }
            return false;
        }

        public void AddNewProcess(OSProcess process)
        {
            RunningProcesses.Add(new Tuple<OSProcess, DateTime>(process, DateTime.Now));
            processorState += process.processor;
            memoryState += process.memory;
        }

        public void removeProcessIfFinished()
        {
            foreach (Tuple<OSProcess, DateTime> process in RunningProcesses)
            {
                if (DateTime.Now - process.Item2 > TimeSpan.FromMilliseconds(process.Item1.timeToComplete))
                {
                    RunningProcesses.Remove(process);
                    processorState -= process.Item1.processor;
                    memoryState -= process.Item1.memory;
                }
            }
        }
    }
}


