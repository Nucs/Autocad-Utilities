using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message message = new Message();

            Recipient recipient1 = new Recipient();
            recipient1.AddressType = "SMTP";
            recipient1.DisplayType = DisplayType.MailUser;
            recipient1.ObjectType = ObjectType.MailUser;
            recipient1.DisplayName = "John Smith";
            recipient1.EmailAddress = "John@domain.com";
            recipient1.RecipientType = RecipientType.To;

            Recipient recipient2 = new Recipient();
            recipient2.AddressType = "SMTP";
            recipient2.DisplayType = DisplayType.MailUser;
            recipient2.ObjectType = ObjectType.MailUser;
            recipient2.DisplayName = "Mary Smith";
            recipient2.EmailAddress = "Mary@domain.com";
            recipient2.RecipientType = RecipientType.Cc;

            string htmlBody = "<html><body><b>Hello World bold html text</b></body></html>";
            byte[] rtfBody = System.Text.Encoding.UTF8.GetBytes("{\\rtf1\\ansi\\ansicpg1252\\fromhtml1 \\htmlrtf0 " + htmlBody + "}");

            message.Subject = "Html message";
            message.DisplayTo = "John Smith";
            message.DisplayCc = "Mary Smith";
            message.Recipients.Add(recipient1);
            message.Recipients.Add(recipient2);
            message.BodyHtmlText = htmlBody;
            message.BodyRtf = rtfBody;
            message.MessageFlags.Add(MessageFlag.Unsent);
            message.StoreSupportMasks.Add(StoreSupportMask.Create);

            message.Save("c:\\temp\\message.msg", true);
        }
    }
}
