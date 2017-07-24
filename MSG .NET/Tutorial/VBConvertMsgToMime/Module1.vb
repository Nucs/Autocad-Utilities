Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim msgMessage As Independentsoft.Msg.Message = New Independentsoft.Msg.Message("c:\\test\\test.msg")

            Dim mimeMessage As Independentsoft.Email.Mime.Message = msgMessage.ConvertToMimeMessage()

            mimeMessage.Save("c:\\test\\test.eml")

        End Sub
    End Class
End Namespace