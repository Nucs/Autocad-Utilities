using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message task = new Message("c:\\temp\\task.msg");

            Console.WriteLine("Subject: " + task.Subject);
            Console.WriteLine("StartDate: " + task.TaskStartDate);
            Console.WriteLine("EndTime: " + task.TaskDueDate);
            Console.WriteLine("Owner: " + task.Owner);
            Console.WriteLine("PercentComplete: " + task.PercentComplete);
            Console.WriteLine("TaskStatus: " + task.TaskStatus);
            Console.WriteLine("ActualWork: " + task.ActualWork);
            Console.WriteLine("TotalWork: " + task.TotalWork);
            Console.WriteLine("DateCompleted: " + task.DateCompleted);
            Console.WriteLine("Body: " + task.Body);

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
