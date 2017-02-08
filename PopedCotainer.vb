Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Text
Imports System.Windows.Forms


Public Partial Class PopedCotainer
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub


	Protected Overrides Function ProcessDialogKey(keyData As Keys) As Boolean
		' Alt+F4 is to closing
		If (keyData And Keys.Alt) = Keys.Alt Then
			If (keyData And Keys.F4) = Keys.F4 Then
				Me.Parent.Hide()
				Return True
			End If
		End If

		If (keyData And Keys.Enter) = Keys.Enter Then
			If TypeOf Me.ActiveControl Is Button Then
				TryCast(Me.ActiveControl, Button).PerformClick()
				Return True
			End If
		End If

		Return MyBase.ProcessDialogKey(keyData)
	End Function



End Class
