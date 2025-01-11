using System.Text;

namespace Process
{
    public class OSProcess
    {
        protected string name { get; private set; } = "";
        protected int timeToComplete { get; private set; } = 0;
        protected int priority { get; private set; } = 0;
        protected double memory { get; private set; } = 0;
        protected double processor { get; private set; } = 0;

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
            name = "";
            timeToComplete = 0;
            priority = 0;
            memory = 0;
            processor = 0;
        }

        public byte[] toCSV()
        {
            string csv = $"{name},{timeToComplete},{priority},{memory},{processor}";
            try
            {
                return Encoding.UTF8.GetBytes(csv);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error converting to CSV: {e.Message}");
                return null;
            }
        }

        public OSProcess toProcess(byte[] csv, int size)
        {
            string csvChars = Encoding.UTF8.GetString(csv, 0, size);
            string[] parts = csvChars.Split(',');
            if (parts.Length != 5)
            {
                Console.WriteLine("Invalid CSV format");
                Console.WriteLine($"Expected 5 parts, got {parts.Length}");
                Console.WriteLine($"size {size} string{csvChars}");
                return null;
            }

            OSProcess process = new OSProcess();

            process.name = parts[0];
            process.timeToComplete = int.Parse(parts[1]);
            process.priority = int.Parse(parts[2]);
            process.memory = double.Parse(parts[3]);
            process.processor = double.Parse(parts[4]);
            return process;
        }

        public override string ToString()
        {
            return $"Name: {name}, Time to complete: {timeToComplete}, Priority: {priority}, Memory: {memory}, Processor: {processor}";
        }
    }
}
