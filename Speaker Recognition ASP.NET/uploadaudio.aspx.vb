Imports System.Threading.Tasks
Imports Microsoft.ProjectOxford.SpeakerRecognition
Imports Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification
Imports Newtonsoft.Json

Public Class uploadaudio
    Inherits Page
    Dim serviceClient = New SpeakerIdentificationServiceClient("")
    Dim str As StringBuilder = New StringBuilder
    Dim writer As JsonWriter = New JsonTextWriter(New IO.StringWriter(str))

    Protected Async Sub Page_LoadAsync(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If String.IsNullOrEmpty(Request.QueryString("userID")) Then
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Failed")
            writer.WritePropertyName("response")
            writer.WriteValue("userID argument is not provided.")
            writer.WriteEnd()
            Response.Write(str.ToString)
            Return
        End If

        If String.IsNullOrEmpty(Request.QueryString("filePath")) Then
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Failed")
            writer.WritePropertyName("response")
            writer.WriteValue("filePath argument is not provided.")
            writer.WriteEnd()
            Response.Write(str.ToString)
            Return
        End If

        Dim stream As IO.Stream,
            processPollingLocation As OperationLocation

        Try
            stream = IO.File.OpenRead(Request.QueryString("filePath"))
            processPollingLocation = Await serviceClient.EnrollAsync(stream, New Guid(Request.QueryString("userID")), True)
        Catch ex As Exception
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Failed")
            writer.WritePropertyName("response")
            writer.WriteValue(ex.Message)
            writer.WriteEnd()
            Response.Write(str.ToString)
            Return
        End Try
        Dim enrollmentResult As EnrollmentOperation,
                numOfRetries As Integer = 10,
                timeBetweenRetries As TimeSpan = TimeSpan.FromSeconds(5.0)
        While numOfRetries > 0
            Await Task.Delay(timeBetweenRetries)
            enrollmentResult = Await serviceClient.CheckEnrollmentStatusAsync(processPollingLocation)
            If enrollmentResult.Status = Status.Succeeded Then
                Exit While
            ElseIf enrollmentResult.Status = Status.Failed Then
                writer.WriteStartObject()
                writer.WritePropertyName("Status")
                writer.WriteValue("Failed")
                writer.WritePropertyName("response")
                writer.WriteValue(enrollmentResult.Message)
                writer.WriteEnd()
                Response.Write(str.ToString)
                Return
            End If
            numOfRetries -= 1
        End While
        If numOfRetries <= 0 Then
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Failed")
            writer.WritePropertyName("response")
            writer.WriteValue("嘗試新增辨識音檔失敗 (10次timeout)")
            writer.WriteEnd()
            Response.Write(str.ToString)
            Return
        End If
        writer.WriteStartObject()
        writer.WritePropertyName("Status")
        writer.WriteValue("Success")
        writer.WritePropertyName("response")
        writer.WriteValue("已經幫User(" & Request.QueryString("userID") & ")新增辨別音檔")
        writer.WriteEnd()
        Response.Write(str.ToString)
    End Sub

End Class