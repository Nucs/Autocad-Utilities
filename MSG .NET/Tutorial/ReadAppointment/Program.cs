using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message appointment = new Message("c:\\temp\\appointment.msg");

            Console.WriteLine("Subject: " + appointment.Subject);
            Console.WriteLine("StartTime: " + appointment.AppointmentStartTime);
            Console.WriteLine("EndTime: " + appointment.AppointmentEndTime);
            Console.WriteLine("Location: " + appointment.Location);
            Console.WriteLine("IsReminderSet: " + appointment.IsReminderSet);
            Console.WriteLine("SenderName: " + appointment.SenderName);
            Console.WriteLine("SenderEmailAddress: " + appointment.SenderEmailAddress);
            Console.WriteLine("To: " + appointment.DisplayTo);
            Console.WriteLine("Cc: " + appointment.DisplayCc);
            Console.WriteLine("Body: " + appointment.Body);

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
