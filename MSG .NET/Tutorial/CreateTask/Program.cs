using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message task = new Message();

            task.MessageClass = "IPM.Task";
            task.Subject = "Test";
            task.Body = "Body text";
            task.Owner = "John";
            task.TaskStatus = TaskStatus.NotStarted;
            task.Priority = Priority.High;
            task.TaskStartDate = DateTime.Today.AddDays(1);
            task.TaskDueDate = DateTime.Today.AddDays(5);

            task.Save("c:\\temp\\task.msg", true);
        }
    }
}
