Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim message As New Message("c:\temp\test.msg")

            For Each attachment As Attachment In message.Attachments
                attachment.Save("c:\temp\" & attachment.DisplayName)
            Next

        End Sub
    End Class
End Namespace