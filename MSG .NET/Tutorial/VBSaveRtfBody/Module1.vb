Imports System
Imports System.IO
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim message As New Message("c:\temp\message.msg")

            Dim file As New FileStream("c:\temp\body.rtf", FileMode.CreateNew)
            file.Write(message.BodyRtf, 0, message.BodyRtf.Length)
            file.Close()

        End Sub
    End Class
End Namespace