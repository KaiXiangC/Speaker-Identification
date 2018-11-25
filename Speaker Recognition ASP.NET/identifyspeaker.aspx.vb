Imports System.Threading.Tasks
Imports Microsoft.ProjectOxford.SpeakerRecognition
Imports Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification
Imports Newtonsoft.Json

Public Class identifyspeaker
    Inherits Page
    Dim serviceClient = New SpeakerIdentificationServiceClient("")
    Dim str As StringBuilder = New StringBuilder
    Dim writer As JsonWriter = New JsonTextWriter(New IO.StringWriter(str))

    Protected Async Sub Page_LoadAsync(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
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
            processPollingLocation As OperationLocation,
            users As List(Of Guid) = New List(Of Guid),
            identificationResponse As IdentificationOperation = Nothing,
            numOfRetries As Integer = 10,
            timeBetweenRetries As TimeSpan = TimeSpan.FromSeconds(5.0)

        Try
            Dim profiles As Profile() = Await serviceClient.GetProfilesAsync
            For i = 0 To profiles.Length - 1
                users.Add(New Guid(profiles(i).ProfileId.ToString))
            Next
        Catch ex As Exception
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Failed")
            writer.WritePropertyName("response")
            writer.WriteValue("Failed to load user IDs.")
            writer.WriteEnd()
            Response.Write(str.ToString)
            Return
        End Try

        Try
            stream = IO.File.OpenRead(Request.QueryString("filePath"))
            processPollingLocation = Await serviceClient.IdentifyAsync(stream, users.ToArray, True)
            While numOfRetries > 0
                Await Task.Delay(timeBetweenRetries)
                identificationResponse = Await serviceClient.CheckIdentificationStatusAsync(processPollingLocation)
                If identificationResponse.Status = Status.Succeeded Then
                    Exit While
                ElseIf identificationResponse.Status = Status.Failed Then
                    writer.WriteStartObject()
                    writer.WritePropertyName("Status")
                    writer.WriteValue("Failed")
                    writer.WritePropertyName("response")
                    writer.WriteValue(identificationResponse.Message)
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
                writer.WriteValue("認證動作逾時")
                writer.WriteEnd()
                Response.Write(str.ToString)
                Return
            End If

            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Success")
            writer.WritePropertyName("ID")
            writer.WriteValue(identificationResponse.ProcessingResult.IdentifiedProfileId.ToString())
            writer.WritePropertyName("Confidence")
            writer.WriteValue(identificationResponse.ProcessingResult.Confidence.ToString())
            writer.WriteEnd()
            Response.Write(str.ToString)
            Return
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
    End Sub

End Class