namespace Process
{
    public class Process
    {
        protected string name { get; private set; } = "";
        protected int timeToComplete { get; private set; } = 0;
        protected int priority { get; private set; } = 0;
        protected double memory { get; private set; } = 0;
        protected double processor { get; private set; } = 0;

        public Process(string name, int timeToComplete, int priority, double memory, double processor)
        {
            this.name = name;
            this.timeToComplete = timeToComplete;
            this.priority = priority;
            this.memory = memory;
            this.processor = processor;
        }
    }
}
