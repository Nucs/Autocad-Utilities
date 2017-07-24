Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim task As New Message("c:\temp\task.msg")

            Console.WriteLine("Subject: " & task.Subject)
            Console.WriteLine("StartDate: " & task.TaskStartDate)
            Console.WriteLine("EndTime: " & task.TaskDueDate)
            Console.WriteLine("Owner: " & task.Owner)
            Console.WriteLine("PercentComplete: " & task.PercentComplete)
            Console.WriteLine("TaskStatus: " & task.TaskStatus.ToString())
            Console.WriteLine("ActualWork: " & task.ActualWork)
            Console.WriteLine("TotalWork: " & task.TotalWork)
            Console.WriteLine("DateCompleted: " & task.DateCompleted)
            Console.WriteLine("Body: " & task.Body)

            Console.WriteLine("Press any key to exit.")
            Console.Read()

        End Sub
    End Class
End Namespace