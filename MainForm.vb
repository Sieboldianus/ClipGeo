Public Class MainForm
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button4.Click, Button3.Click
        'MsgBox("Version: " & Environment.Version.ToString())
        ClipDataForm.Text = "GetGeo v." & Application.ProductVersion
        ClipDataForm.Show()
    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        'MsgBox("Version: " & Environment.Version.ToString())
        'OptionsDataForm.Show()
    End Sub

End Class

