Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim message As New Message()

            Dim recipient1 As New Recipient()
            recipient1.AddressType = "SMTP"
            recipient1.DisplayType = DisplayType.MailUser
            recipient1.ObjectType = ObjectType.MailUser
            recipient1.DisplayName = "John Smith"
            recipient1.EmailAddress = "John@domain.com"
            recipient1.RecipientType = RecipientType.[To]

            Dim recipient2 As New Recipient()
            recipient2.AddressType = "SMTP"
            recipient2.DisplayType = DisplayType.MailUser
            recipient2.ObjectType = ObjectType.MailUser
            recipient2.DisplayName = "Mary Smith"
            recipient2.EmailAddress = "Mary@domain.com"
            recipient2.RecipientType = RecipientType.Cc

            Dim htmlBody As String = "<html><body><b>Hello World bold html text</b></body></html>"
            Dim rtfBody As Byte() = System.Text.Encoding.UTF8.GetBytes("{\rtf1\ansi\ansicpg1252\fromhtml1 \htmlrtf0  " & htmlBody & "}")

            message.Subject = "Html message"
            message.DisplayTo = "John Smith"
            message.DisplayCc = "Mary Smith"
            message.Recipients.Add(recipient1)
            message.Recipients.Add(recipient2)
            message.BodyHtmlText = htmlBody
            message.BodyRtf = rtfBody
            message.MessageFlags.Add(MessageFlag.Unsent)
            message.StoreSupportMasks.Add(StoreSupportMask.Create)

            message.Save("c:\temp\message.msg", True)

        End Sub
    End Class
End Namespace