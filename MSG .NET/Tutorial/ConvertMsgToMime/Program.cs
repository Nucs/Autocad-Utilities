using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Independentsoft.Msg.Message msgMessage = new Independentsoft.Msg.Message("c:\\test\\test.msg");

            Independentsoft.Email.Mime.Message mimeMessage = msgMessage.ConvertToMimeMessage();

            mimeMessage.Save("c:\\test\\test.eml");
        }
    }
}
