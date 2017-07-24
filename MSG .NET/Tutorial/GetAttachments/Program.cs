using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message message = new Message("c:\\temp\\test.msg");

            foreach (Attachment attachment in message.Attachments)
            {
                attachment.Save("c:\\temp\\" + attachment.DisplayName);
            }
        }
    }
}
