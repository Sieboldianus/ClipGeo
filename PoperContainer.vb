Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms


Public Partial Class PoperContainer
	Inherits ToolStripDropDown
	Private m_popedContainer As Control

	Private m_host As ToolStripControlHost

	Private m_fade As Boolean = True




	Public Sub New(popedControl As Control)
		InitializeComponent()

		If popedControl Is Nothing Then
			Throw New ArgumentNullException("content")
		End If

		Me.m_popedContainer = popedControl

		Me.m_fade = SystemInformation.IsMenuAnimationEnabled AndAlso SystemInformation.IsMenuFadeEnabled

		Me.m_host = New ToolStripControlHost(popedControl)
		m_host.AutoSize = False
		'make it take the same room as the poped control
		Padding = InlineAssignHelper(Margin, InlineAssignHelper(m_host.Padding, InlineAssignHelper(m_host.Margin, Padding.Empty)))

		popedControl.Location = Point.Empty

		Me.Items.Add(m_host)

		AddHandler popedControl.Disposed, Sub(sender As Object, e As EventArgs) 
		popedControl = Nothing
			' this popup container will be disposed immediately after disposion of the contained control
		Dispose(True)

End Sub
	End Sub




	Protected Overrides Function ProcessDialogKey(keyData As Keys) As Boolean
		'prevent alt from closing it and allow alt+menumonic to work
		If (keyData And Keys.Alt) = Keys.Alt Then
			Return False
		End If

		Return MyBase.ProcessDialogKey(keyData)
	End Function






	Public Overloads Sub Show(control As Control)
		If control Is Nothing Then
			Throw New ArgumentNullException("control")
		End If

		Show(control, control.ClientRectangle)
	End Sub

	Public Overloads Sub Show(f As Form, p As Point)
		Me.Show(f, New Rectangle(p, New Size(0, 0)))

	End Sub

	Private Overloads Sub Show(control As Control, area As Rectangle)
		If control Is Nothing Then
			Throw New ArgumentNullException("control")
		End If


		Dim location As Point = control.PointToScreen(New Point(area.Left, area.Top + area.Height))

		Dim screen__1 As Rectangle = Screen.FromControl(control).WorkingArea

		If location.X + Size.Width > (screen__1.Left + screen__1.Width) Then
			location.X = (screen__1.Left + screen__1.Width) - Size.Width
		End If

		If location.Y + Size.Height > (screen__1.Top + screen__1.Height) Then
			location.Y -= Size.Height + area.Height
		End If

		location = control.PointToClient(location)

		Show(control, location, ToolStripDropDownDirection.BelowRight)
	End Sub




	Private Const frames As Integer = 5
	Private Const totalduration As Integer = 100
	Private Const frameduration As Integer = totalduration \ frames

	Protected Overrides Sub SetVisibleCore(visible As Boolean)
		Dim opacity__1 As Double = Opacity
		If visible AndAlso m_fade Then
			Opacity = 0
		End If
		MyBase.SetVisibleCore(visible)
		If Not visible OrElse Not m_fade Then
			Return
		End If
		For i As Integer = 1 To frames
			If i > 1 Then
				System.Threading.Thread.Sleep(frameduration)
			End If
			Opacity = opacity__1 * CDbl(i) / CDbl(frames)
		Next
		Opacity = opacity__1
	End Sub




	Protected Overrides Sub OnOpening(e As CancelEventArgs)
		If m_popedContainer.IsDisposed OrElse m_popedContainer.Disposing Then
			e.Cancel = True
			Return
		End If
		MyBase.OnOpening(e)
	End Sub

	Protected Overrides Sub OnOpened(e As EventArgs)
		m_popedContainer.Focus()

		MyBase.OnOpened(e)
	End Sub
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function





End Class
