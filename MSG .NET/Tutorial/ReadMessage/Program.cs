using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message message = new Message("c:\\temp\\message.msg");

            Console.WriteLine("Subject: " + message.Subject);
            Console.WriteLine("SenderName: " + message.SenderName);
            Console.WriteLine("SenderEmailAddress: " + message.SenderEmailAddress);
            Console.WriteLine("ReceivedByName: " + message.ReceivedByName);
            Console.WriteLine("ReceivedByEmailAddress: " + message.ReceivedByEmailAddress);
            Console.WriteLine("DisplayTo: " + message.DisplayTo);
            Console.WriteLine("DisplayCc: " + message.DisplayCc);
            Console.WriteLine("Body: " + message.Body);
            Console.WriteLine("-----------------------------------------------------------------------");
            Console.WriteLine("BodyHtmlText: " + message.BodyHtmlText);

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
