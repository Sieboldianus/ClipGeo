Partial Class PopedCotainer

	Private components As System.ComponentModel.IContainer = Nothing

	Protected Overrides Sub Dispose(disposing As Boolean)
		If disposing AndAlso (components IsNot Nothing) Then
			components.Dispose()
		End If
		MyBase.Dispose(disposing)
	End Sub

	#Region "Component Designer generated code"
	Private Sub InitializeComponent()
		Me.SuspendLayout()
		' 
		' PopedCotainer
		' 
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit
		Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.Font = New System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(238))
		Me.Margin = New System.Windows.Forms.Padding(0)
		Me.Name = "PopedCotainer"
		Me.Padding = New System.Windows.Forms.Padding(9)
		Me.Size = New System.Drawing.Size(40, 32)
		Me.ResumeLayout(False)

	End Sub

	#End Region

End Class
