using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Independentsoft.Email.Mime.Message mimeMessage = new Independentsoft.Email.Mime.Message("c:\\test\\test.eml");

            Independentsoft.Msg.Message msgMessage = new Independentsoft.Msg.Message(mimeMessage);

            msgMessage.Save("c:\\test\\test.msg");
        }
    }
}
