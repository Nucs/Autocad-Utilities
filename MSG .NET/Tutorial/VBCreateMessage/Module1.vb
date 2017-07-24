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

            message.Subject = "Test"
            message.Body = "Body text"
            message.DisplayTo = "John Smith"
            message.DisplayCc = "Mary Smith"
            message.Recipients.Add(recipient1)
            message.Recipients.Add(recipient2)
            message.MessageFlags.Add(MessageFlag.Unsent)
            message.StoreSupportMasks.Add(StoreSupportMask.Create)

            message.Save("c:\temp\message.msg", True)

        End Sub
    End Class
End Namespace