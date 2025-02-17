﻿using MemoryPack;
using System.Text;


namespace OSProcesses
{
    [MemoryPackable]
    public partial class OSProcess
    {
        public string name { get; private set; } = "";
        public int timeToComplete { get; set; } = 0;
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

        [MemoryPackConstructor]
        public OSProcess()
        {
            name = "EMPTY";
            timeToComplete = -1;
            priority = -1;
            memory = -1;
            processor = -1;
        }
        public byte[] ToMemoryPack()
        {
            return MemoryPackSerializer.Serialize(this);
        }

        public static OSProcess FromMemoryPack(byte[] data)
        {
            return MemoryPackSerializer.Deserialize<OSProcess>(data);
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
