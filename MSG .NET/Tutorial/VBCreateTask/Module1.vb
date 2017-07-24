Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim task As New Message()

            task.MessageClass = "IPM.Task"
            task.Subject = "Test"
            task.Body = "Body text"
            task.Owner = "John"
            task.TaskStatus = TaskStatus.NotStarted
            task.Priority = Priority.High
            task.TaskStartDate = DateTime.Today.AddDays(1)
            task.TaskDueDate = DateTime.Today.AddDays(5)

            task.Save("c:\temp\task.msg", True)

        End Sub
    End Class
End Namespace