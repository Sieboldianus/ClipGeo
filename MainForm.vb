Public Class MainForm
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button4.Click, Button3.Click, Button5.Click, Button2.Click
        'MsgBox("Version: " & Environment.Version.ToString())
        ClipDataForm.Text = "ClipGeo v." & Application.ProductVersion
        ClipDataForm.Show()
    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs)
        'MsgBox("Version: " & Environment.Version.ToString())
        'OptionsDataForm.Show()
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "ClipGeo Tools v." & Application.ProductVersion
    End Sub
End Class

