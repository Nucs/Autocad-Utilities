Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim note As New Message()

            note.MessageClass = "IPM.StickyNote"
            note.Subject = "Test"
            note.Body = "Test"
            note.NoteColor = NoteColor.Green
            note.NoteTop = 200
            note.NoteLeft = 300
            note.NoteHeight = 200
            note.NoteWidth = 250

            note.Save("c:\temp\note.msg", True)

        End Sub
    End Class
End Namespace