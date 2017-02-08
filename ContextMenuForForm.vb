Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Text
Imports System.Windows.Forms
'For Image Web Load:
Imports System.IO
Imports System.Net


Partial Public Class ContextMenuForForm

    Inherits UserControl

    Public Shared isfirstpaint As Boolean = True

    Public Sub New()
        InitializeComponent()

    End Sub
    Friend WithEvents PreviousB As System.Windows.Forms.Button
    Friend WithEvents NextB As System.Windows.Forms.Button

    ''' <summary> 
    ''' Required designer variable.
    ''' </summary>
    Private components As System.ComponentModel.IContainer = Nothing

    ''' <summary> 
    ''' Clean up any resources being used.
    ''' </summary>
    ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso (components IsNot Nothing) Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

#Region "Component Designer generated code"
    ' <summary> 
    ' Required method for Designer support - do not modify 
    ' the contents of this method with the code editor.
    ' </summary>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.label3 = New System.Windows.Forms.Label()
        Me.label1 = New System.Windows.Forms.Label()
        Me.pictureBox1 = New System.Windows.Forms.PictureBox()
        Me.toolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.PreviousB = New System.Windows.Forms.Button()
        Me.NextB = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.label2 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.label3.Location = New System.Drawing.Point(127, 54)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(44, 13)
        Me.label3.TabIndex = 3
        Me.label3.Text = "Views:"
        '
        'label1
        '
        Me.label1.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.label1.Location = New System.Drawing.Point(0, 0)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(272, 23)
        Me.label1.TabIndex = 0
        Me.label1.Text = "Photoview (sorted by popularity)"
        Me.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pictureBox1
        '
        Me.pictureBox1.Location = New System.Drawing.Point(17, 76)
        Me.pictureBox1.Name = "pictureBox1"
        Me.pictureBox1.Size = New System.Drawing.Size(240, 240)
        Me.pictureBox1.TabIndex = 31
        Me.pictureBox1.TabStop = False
        '
        'PreviousB
        '
        Me.PreviousB.Location = New System.Drawing.Point(17, 340)
        Me.PreviousB.Name = "PreviousB"
        Me.PreviousB.Size = New System.Drawing.Size(60, 23)
        Me.PreviousB.TabIndex = 32
        Me.PreviousB.Text = "Previous"
        Me.PreviousB.UseVisualStyleBackColor = True
        '
        'NextB
        '
        Me.NextB.Location = New System.Drawing.Point(197, 340)
        Me.NextB.Name = "NextB"
        Me.NextB.Size = New System.Drawing.Size(60, 23)
        Me.NextB.TabIndex = 32
        Me.NextB.Text = "Next"
        Me.NextB.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label4.Location = New System.Drawing.Point(182, 36)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(75, 13)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "ID0"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label5
        '
        Me.Label5.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(137, 54)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(120, 13)
        Me.Label5.TabIndex = 1
        Me.Label5.Text = "0"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(127, 36)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(49, 13)
        Me.label2.TabIndex = 1
        Me.label2.Text = "PhotoID:"
        '
        'Label6
        '
        Me.Label6.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label6.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label6.Location = New System.Drawing.Point(17, 319)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(240, 15)
        Me.Label6.TabIndex = 1
        Me.Label6.Text = "Click image to open web address."
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(14, 36)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(25, 13)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "Lat:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(14, 54)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(28, 13)
        Me.Label8.TabIndex = 3
        Me.Label8.Text = "Lng:"
        '
        'Label9
        '
        Me.Label9.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label9.Location = New System.Drawing.Point(46, 36)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(75, 13)
        Me.Label9.TabIndex = 1
        Me.Label9.Text = "0"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label10
        '
        Me.Label10.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label10.Location = New System.Drawing.Point(49, 54)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(72, 13)
        Me.Label10.TabIndex = 1
        Me.Label10.Text = "0"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label11
        '
        Me.Label11.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.Label11.Location = New System.Drawing.Point(83, 345)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(108, 18)
        Me.Label11.TabIndex = 1
        Me.Label11.Text = "x of x"
        Me.Label11.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'ContextMenuForForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.NextB)
        Me.Controls.Add(Me.PreviousB)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.label3)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.label2)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.pictureBox1)
        Me.Name = "ContextMenuForForm"
        Me.Size = New System.Drawing.Size(272, 372)
        CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region
    Private label3 As System.Windows.Forms.Label
    Private label1 As System.Windows.Forms.Label
    'Private pictureBox1 As System.Windows.Forms.PictureBox
    Private toolTip1 As System.Windows.Forms.ToolTip

    Private Sub ContextMenuForForm_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        If isfirstpaint Then
            Dim url As String
            pictureBox1.Image = Nothing
            url = visualForm.pList(visualForm.pListCursor).pUrl
            Label4.Text = visualForm.pList(visualForm.pListCursor).photoid
            Label5.Text = visualForm.pList(visualForm.pListCursor).views
            Label9.Text = visualForm.pList(visualForm.pListCursor).lat
            Label10.Text = visualForm.pList(visualForm.pListCursor).lng
            Label11.Text = visualForm.pList.Count - visualForm.pListCursor & " of " & visualForm.pList.Count
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage
            pictureBox1.WaitOnLoad = False
            pictureBox1.LoadAsync(url)
            isfirstpaint = False
        End If
    End Sub

    Private Sub NextB_Click(sender As Object, e As EventArgs) Handles NextB.Click
        If Not visualForm.pListCursor = 0 Then

            visualForm.pListCursor = visualForm.pListCursor - 1
            isfirstpaint = True
            visualForm.UpdatePhotoView()
        End If

    End Sub

    Private Sub PreviousB_Click(sender As Object, e As EventArgs) Handles PreviousB.Click
        If Not visualForm.pListCursor = visualForm.pList.Count - 1 Then

            visualForm.pListCursor = visualForm.pListCursor + 1
            isfirstpaint = True
            visualForm.UpdatePhotoView()
        End If
    End Sub

    Public Shared Sub openAdress()
        Dim webAddress As String = "http://flickr.com/photo.gne?id=" & visualForm.pList(visualForm.pListCursor).photoid
        Process.Start(webAddress)
    End Sub

    Private Sub pictureBox1_Click(sender As Object, e As EventArgs) Handles pictureBox1.Click
        openAdress()
    End Sub
End Class
