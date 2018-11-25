Imports Microsoft.ProjectOxford.SpeakerRecognition
Imports Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification
Imports Newtonsoft.Json

Public Class listusers
    Inherits Page
    Dim serviceClient = New SpeakerIdentificationServiceClient("")
    Dim str As StringBuilder = New StringBuilder
    Dim writer As JsonWriter = New JsonTextWriter(New IO.StringWriter(str))

    Protected Async Sub Page_LoadAsync(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim profiles As Profile() = Await serviceClient.GetProfilesAsync
        writer.WriteStartObject()
        writer.WritePropertyName("Users")
        writer.WriteStartArray()
        For Each p In profiles
            writer.WriteStartObject()

            writer.WritePropertyName("ID")
            writer.WriteValue(p.ProfileId)
            writer.WritePropertyName("Locale")
            writer.WriteValue(p.Locale)
            writer.WritePropertyName("EnrollmentStatus")
            writer.WriteValue(p.EnrollmentStatus.ToString)

            writer.WriteEndObject()
        Next
        writer.WriteEndArray()
        writer.WriteEnd()
        Response.Write(str.ToString)
    End Sub

End Class