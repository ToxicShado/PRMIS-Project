using System.Diagnostics;
using System.Text;


namespace Process
{
    public class OSProcess
    {
        public string name { get; private set; } = "";
        public int timeToComplete { get; private set; } = 0;
        public int priority { get; private set; } = 0;
        public double memory { get; private set; } = 0;
        public double processor { get; private set; } = 0;

        public OSProcess(string name, int timeToComplete, int priority, double memory, double processor)
        {
            this.name = name;
            this.timeToComplete = timeToComplete;
            this.priority = priority;
            this.memory = memory;
            this.processor = processor;
        }

        public OSProcess()
        {
            name = "EMPTY";
            timeToComplete = -1;
            priority = -1;
            memory = -1;
            processor = -1;
        }

        public static List<OSProcess> GenerateNProcesses(int n,bool writeThemOut)
        {
            Random random = new Random();
            List<OSProcess> processes = new List<OSProcess>();
            for (int i = 0; i < n; i++)
            {
                processes.Add(new OSProcess($"Test{i}", random.Next(500, 5000), random.Next(0, 9), random.Next(1, 100), random.Next(1, 100)));
                if (writeThemOut && i == 0)
                {
                    Console.WriteLine("\n================================================================================================");
                    Console.WriteLine("[INFO] Sending the following processes:");
                }
                if (writeThemOut)
                {
                    Console.WriteLine(processes[i]);
                }
                if (writeThemOut && i == n - 1)
                {
                    Console.WriteLine("================================================================================================\n");
                }
            }
            return processes;
        }

        public byte[] ConvertProcessTotoBytecodeCSV()
        {
            string csv = $"{name},{timeToComplete},{priority},{memory},{processor}";
            try
            {
                return Encoding.UTF8.GetBytes(csv);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[EXCPETION] Error converting to CSV: {e.Message}");
                return null;
            }
        }

        public override string ToString()
        {
            return $"Name : {name}, Time To Complete: {timeToComplete}, Priority: {priority}, Memory Usage: {memory}, Processor Usage: {processor}";
        }
    }
}
