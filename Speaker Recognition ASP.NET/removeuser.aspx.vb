Imports Microsoft.ProjectOxford.SpeakerRecognition
Imports Microsoft.ProjectOxford.SpeakerRecognition.Contract
Imports Newtonsoft.Json

Public Class removeuser
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
        Try
            Await serviceClient.DeleteProfileAsync(New Guid(Request.QueryString("userID")))
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Success")
            writer.WritePropertyName("response")
            writer.WriteValue("user(" & Request.QueryString("userID") & ") was removed")
            writer.WriteEnd()
            Response.Write(str.ToString)
        Catch ex As DeleteProfileException
            writer.WriteStartObject()
            writer.WritePropertyName("Status")
            writer.WriteValue("Failed")
            writer.WritePropertyName("response")
            writer.WriteValue("Failed to remove user(" & Request.QueryString("userID") & ")")
            writer.WriteEnd()
            Response.Write(str.ToString)
        End Try
    End Sub

End Class