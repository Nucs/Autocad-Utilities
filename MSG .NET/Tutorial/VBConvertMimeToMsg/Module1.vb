Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim mimeMessage As Independentsoft.Email.Mime.Message = New Independentsoft.Email.Mime.Message("c:\\test\\test.eml")

            Dim msgMessage As Independentsoft.Msg.Message = New Independentsoft.Msg.Message(mimeMessage)

            msgMessage.Save("c:\\test\\test.msg")

        End Sub
    End Class
End Namespace