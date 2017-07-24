using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message appointment = new Message();

            appointment.MessageClass = "IPM.Appointment";
            appointment.Subject = "Test";
            appointment.Body = "Body text";
            appointment.Location = "My Office";
            appointment.AppointmentStartTime = DateTime.Today.AddHours(16);
            appointment.AppointmentEndTime = DateTime.Today.AddHours(17);
            
            appointment.Save("c:\\temp\\appointment.msg", true);
        }
    }
}
