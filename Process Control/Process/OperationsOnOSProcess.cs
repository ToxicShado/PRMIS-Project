using System.Text;


namespace OSProcesses
{
    public class OperationsOnOSProcess
    {
        public static OSProcess toProcess(byte[] csv, int size)
        {
            if (size == 0)
            {
                Console.WriteLine("Empty CSV received");
                return null;
            }
            if (csv == null)
            {
                Console.WriteLine("Null CSV received");
                return null;
            }

            string csvChars = Encoding.UTF8.GetString(csv, 0, size);
            string[] parts = csvChars.Split(',');
            if (parts.Length != 5)
            {
                Console.WriteLine("Invalid CSV format");
                Console.WriteLine($"Expected 5 parts, got {parts.Length}");
                Console.WriteLine($"size {size} string{csvChars}");
                return null;
            }

            string name = parts[0];
            int timeToComplete;
            int priority;
            double memory;
            double processor;

            bool one = int.TryParse(parts[1], out timeToComplete);
            bool two = int.TryParse(parts[2], out priority);
            bool three = double.TryParse(parts[3], out memory);
            bool four = double.TryParse(parts[4], out processor);

            if (!String.IsNullOrEmpty(name) && one && two && three && four)
            {
                return new OSProcess(name, timeToComplete, priority, memory, processor);
            }
            else
            {
                Console.WriteLine("Invalid format passed, please try sending only with an OSProcess");
                return null;
            }
        }

        public static List<OSProcess> GenerateNProcesses(int n, bool writeThemOut)
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

        public static OSProcess CreateProcess()
        {
            string name = "EMPTY";
            int timeToComplete = -1;
            int priority = -1;
            double memory = -1;
            double processor = -1;

            Console.WriteLine("Creating a new process. Please enter the following details:");

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter process name: ");
                name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    break;
                }
                Console.WriteLine("Process name cannot be empty. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter time to complete (positive integer): ");
                if (int.TryParse(Console.ReadLine(), out timeToComplete) && timeToComplete > 0)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Time to complete must be a positive integer. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter priority (positive integer): ");
                if (int.TryParse(Console.ReadLine(), out priority) && priority > 0)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Priority must be a positive integer. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter memory usage (positive double): ");
                if (double.TryParse(Console.ReadLine(), out memory) && memory > 0 && memory <= 100)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Memory usage must be a positive number. Please try again.");
            }

            // correct input must be gotten. we know users are not to be trusted
            while (true)
            {
                Console.Write("Enter processor usage (positive double): ");
                if (double.TryParse(Console.ReadLine(), out processor) && processor > 0 && processor <= 100)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Processor usage must be a positive number. Please try again.");
            }

            Console.WriteLine("Process creation successful! (at last!)");
            // and boy, are these try catches making the code be unreadable
            return new OSProcess(name, timeToComplete, priority, memory, processor);
        }
    }
}
