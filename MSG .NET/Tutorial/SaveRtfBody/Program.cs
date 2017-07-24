using System;
using System.IO;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message message = new Message("c:\\temp\\message.msg");

            FileStream file = new FileStream("c:\\temp\\body.rtf", FileMode.CreateNew);
            file.Write(message.BodyRtf, 0, message.BodyRtf.Length);
            file.Close();

        }
    }
}
