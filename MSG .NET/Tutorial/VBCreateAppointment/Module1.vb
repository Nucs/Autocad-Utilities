Imports System
Imports Independentsoft.Msg

Namespace Sample
    Class Module1
        Shared Sub Main(ByVal args As String())

            Dim appointment As New Message()

            appointment.MessageClass = "IPM.Appointment"
            appointment.Subject = "Test"
            appointment.Body = "Body text"
            appointment.Location = "My Office"
            appointment.AppointmentStartTime = DateTime.Today.AddHours(16)
            appointment.AppointmentEndTime = DateTime.Today.AddHours(17)

            appointment.Save("c:\temp\appointment.msg", True)

        End Sub
    End Class
End Namespace