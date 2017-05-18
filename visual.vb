Imports System.IO
Imports GMap.NET
Imports GMap.NET.WindowsForms
Imports GMap.NET.WindowsForms.Markers
Imports GMap.NET.WindowsForms.ToolTips
Imports System.Text
Imports System.Net
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Collections.Generic
Imports System.Drawing

Public Class visualForm
    Public Shared IHeight As Integer
    Public Shared IWidth As Integer
    Public Shared bmOrig As Bitmap
    Public Shared stepInc As Integer
    Public Shared startalpha As Integer
    Public Shared CurrentImageURL As String = ""
    Public Shared pList As List(Of PhotoRef) = New List(Of PhotoRef)
    Public Shared pListCursor As Integer = 0
    Public RectP1 As PointLatLng = Nothing
    Public RectP2 As PointLatLng = Nothing

    'SelectionMarker
    Public isMouseDown As Boolean
    Public firstmarkerset As Boolean

    'Dim OriginOffset As Integer = 0
    Dim YList As New List(Of Double)
    Dim XList As New List(Of Double)
    Public Shared fullyloaded As Boolean = False
    Dim YDict As Dictionary(Of Double, Integer) = New Dictionary(Of Double, Integer)
    Dim XDict As Dictionary(Of Double, Integer) = New Dictionary(Of Double, Integer)
    'http://flickr.com/photo.gne?id=8897171833 id=8897171833
    Public Shared photoDict As Dictionary(Of GMap.NET.GPoint, PhotoRef) = New Dictionary(Of GMap.NET.GPoint, PhotoRef) 'Coordinate X/Y of Point in Graphics|Integer Value of Views|String of PhotoID|PhotoURL

    Public Shared datacolor As Color = Color.Black
    Public Shared equalizeImg As Boolean = False
    Dim bgColor As Color = Color.White

    <STAThread>
    Friend Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
    End Sub

    'Private m_popedContainerForButton As ContextMenuForButton
    'a user control that is derievd from PopedContainer; it can contain any type of controls and you design it as if you design a form!!!
    Private m_poperContainerForButton As PoperContainer
    'the container... which displays previous user control as a context poped up menu
    Private m_popedContainerForForm As ContextMenuForForm
    Private m_poperContainerForForm As PoperContainer

    Public Sub New()
        InitializeComponent()
        m_popedContainerForForm = New ContextMenuForForm()
        m_poperContainerForForm = New PoperContainer(m_popedContainerForForm)
    End Sub

    Sub mapcoords(ByVal latitude As Double, ByVal longitude As Double, ByRef fp As FastPix, Optional ByVal localtourist As Integer = Nothing, Optional ByVal views As Integer = Nothing, Optional ByVal photoid As Long = Nothing, Optional ByVal photourl As String = Nothing)
        Dim latcord As Double
        Dim longcord As Double
        If localtourist = 1 Then datacolor = Color.Turquoise
        If localtourist = 2 Then datacolor = Color.Red

        'Startgrayvalue:
        Dim xr As Integer = datacolor.R
        Dim xg As Integer = datacolor.G
        Dim xb As Integer = datacolor.B


        Dim modalpha As Integer
        If GMapControl1.CurrentViewArea.Contains(New PointLatLng(latitude, longitude)) Then
            Dim result As GMap.NET.GPoint = bestPixel(latitude, longitude)
            latcord = result.Y
            longcord = result.X
            If latcord > 0 AndAlso longcord > 0 AndAlso latcord < IHeight - 1 AndAlso longcord < IWidth - 1 Then
                'Update Photo Dict
                If Not views = Nothing AndAlso views > photoDict(result).views Then
                    photoDict(result).views = views
                    photoDict(result).photoid = photoid
                    photoDict(result).pUrl = photourl
                    photoDict(result).lat = latitude
                    photoDict(result).lng = longitude
                End If

                Dim myColor As Color = fp.GetPixel(longcord, latcord)
                If myColor.A = 0 Then
                    myColor = Color.FromArgb(startalpha, xr, xg, xb)
                Else
                    modalpha = myColor.A + stepInc
                    'ColorClassification Tourists Locals
                    If Not IsNothing(localtourist) Then
                        If (myColor.R > 127 AndAlso localtourist = 1) OrElse (myColor.R <= 127 AndAlso localtourist = 2) Then
                            myColor = Color.FromArgb(Math.Min(modalpha, 255), 0, 0, 0)
                        End If
                    End If

                    If modalpha > 255 Then
                        myColor = Color.FromArgb(255, myColor.R, myColor.G, myColor.B)
                    Else
                        myColor = Color.FromArgb(modalpha, myColor.R, myColor.G, myColor.B)
                    End If
                End If
                fp.SetPixel(longcord, latcord, myColor)
            End If
        End If
    End Sub

    Public Sub precalcValues(ByVal Height As Integer, ByVal Width As Integer)
        YList.Clear()
        YDict.Clear()
        XList.Clear()
        XDict.Clear()
        photoDict.Clear()
        'Precalc CoordinatesToPixelLocations
        For yy As Integer = 0 To Height
            Dim Cord As Double = GMapControl1.FromLocalToLatLng(0, yy).Lat
            YList.Add(Cord)
            YDict(Cord) = yy
        Next
        YList.Sort()

        For xx As Integer = 0 To Width
            Dim Cord As Double = GMapControl1.FromLocalToLatLng(xx, 0).Lng
            XList.Add(Cord)
            XDict(Cord) = xx
        Next
        XList.Sort()

        'Photocollection Index
        If ClipDataForm.CheckBox30.Checked = True Then
            For xx As Integer = 0 To Width
                For yy As Integer = 0 To Height
                    Dim pixel As GMap.NET.GPoint
                    Dim photoDat As PhotoRef = New PhotoRef
                    pixel.X = xx
                    pixel.Y = yy
                    photoDat.views = 0
                    photoDict.Add(pixel, photoDat)
                    photoDict(pixel).views = -1
                Next
            Next
        End If
    End Sub

    Function bestPixel(ByVal searchValueLat As Double, ByVal searchValuelng As Double) As GMap.NET.GPoint
        Dim indexY As Long = YList.BinarySearch(searchValueLat)
        Dim indexX As Long = XList.BinarySearch(searchValuelng)
        If indexY < 0 Then
            indexY = indexY Xor -1
        End If
        If indexX < 0 Then
            indexX = indexX Xor -1
        End If
        bestPixel.Y = YDict.Item(YList.Item(indexY))
        bestPixel.X = XDict.Item(XList.Item(indexX))
    End Function

    Private Sub visualForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Dim items As Array

        ComboBox1.Items.Add(MapType.ArcGIS_World_Topo_Map.ToString)
        ComboBox1.Items.Add(MapType.ArcGIS_World_Terrain_Base.ToString)
        ComboBox1.Items.Add(MapType.ArcGIS_World_Street_Map.ToString)
        ComboBox1.Items.Add(MapType.ArcGIS_World_Shaded_Relief.ToString)
        ComboBox1.Items.Add(MapType.ArcGIS_World_Physical_Map.ToString)
        ComboBox1.Items.Add(MapType.BingHybrid.ToString)
        ComboBox1.Items.Add(MapType.BingMap.ToString)
        ComboBox1.Items.Add(MapType.BingMap_New.ToString)
        ComboBox1.Items.Add(MapType.BingSatellite.ToString)
        ComboBox1.Items.Add(MapType.OpenCycleMap.ToString)
        ComboBox1.Items.Add(MapType.OpenStreetMap.ToString)
        'ComboBox1.Items.Add(MapType.ArcGIS_Imagery_World_2D.ToString)
        'ComboBox1.Items.Add(MapType.OviMap.ToString)
        'ComboBox1.Items.Add(MapType.YahooMap.ToString)
        'ComboBox1.Items.Add(MapType.YahooSatellite.ToString)
        ComboBox1.SelectedIndex = 0
        IHeight = PictureBox1.Height
        IWidth = PictureBox1.Width

        Dim overlayOne As New GMapOverlay(GMapControl1, "OverlayOne")
        Dim MarkerOverlay As New GMapOverlay(GMapControl1, "Marker")
        GMapControl1.Overlays.Add(MarkerOverlay)
        Dim MarkerRectOverlay As New GMapOverlay(GMapControl1, "RectMarker")
        GMapControl1.Overlays.Add(MarkerRectOverlay)
        GMapControl1.Overlays.Add(overlayOne)
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim x As String = ComboBox1.SelectedItem.ToString()
        Dim items As Array
        items = System.Enum.GetValues(GetType(GMap.NET.MapType))
        For Each y As MapType In items
            If y.ToString = x Then
                GMapControl1.MapType = y
                Exit For
            End If
        Next

    End Sub

    Private Sub Plot_Click(sender As Object, e As EventArgs) Handles Plot.Click
        If Not PictureBox1.Image Is Nothing Then
            Dim bm As New Bitmap(PictureBox1.Image)
            Dim intTransparent As Integer = Color.Transparent.ToArgb
            Using fp As New FastPix(bm)
                Dim pixels As Integer() = fp.PixelArray
                For i As Integer = 0 To pixels.Length - 1
                    If Not pixels(i) = Color.Transparent.ToArgb Then 'Pink = transparent
                        pixels(i) = pixels(i) Xor &HFFFFFF
                    End If
                Next
            End Using
            bm.MakeTransparent(Color.Pink)
            PictureBox1.Image = bm
        Else
            'Dim color As System.Drawing.Color = System.Drawing.Color.FromArgb(red, green, blue)
            'Dim hue As Single = datacolor.GetHue()
            'hue = 360 - hue 'Inverts color, see http://stackoverflow.com/questions/1165107/how-do-i-invert-a-colour-color
            'datacolor = Color.Cyan
            datacolor = Color.FromArgb(datacolor.ToArgb() Xor &HFFFFFF)
        End If
    End Sub


    Private Sub visualForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ClipDataForm.CheckBox15.Checked = False
        ClipDataForm.Button7.Enabled = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim cDialog As New ColorDialog()
        cDialog.Color = bgColor ' initial selection is current color.
        Dim bgImage As New Bitmap(Me.PictureBox1.Width, Me.PictureBox1.Height)
        Dim BGgrap As Graphics = Graphics.FromImage(bgImage)
        If (cDialog.ShowDialog() = DialogResult.OK) Then
            BGgrap.Clear(cDialog.Color)
            PictureBox1.BackgroundImage = bgImage
            bgColor = cDialog.Color
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.PictureBox1.BackgroundImage = Nothing
        Me.Refresh()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        'Standard behaviour: Export with Background/Map
        If PictureBox1.Visible = True AndAlso Not PictureBox1.Image Is Nothing Then
            'CaptureImage First (before Dialog appears)
            Dim sp As System.Drawing.Point = GMapControl1.PointToScreen(New Point(0, 0)) 'Absolute Position of GMapControl1
            Dim ds As System.Drawing.Size = GMapControl1.Size
            Dim sr As New System.Drawing.Rectangle(sp, ds)
            'Convert the Image to a PNG
            Dim tmpImage As System.Drawing.Image
            tmpImage = CaptureImage(sp, System.Drawing.Point.Empty, sr, "")
            Dim AppPath As String = Application.StartupPath() & "\"
            Dim visMap As New Bitmap(PictureBox1.Image)
            SaveFileDialog1.Filter = "PNG Files (*.png*)|*.png"
            If Not (Directory.Exists(AppPath & "Output\04_MapVis")) Then
                Directory.CreateDirectory(AppPath & "Output\04_MapVis")
            End If
            SaveFileDialog1.InitialDirectory = AppPath & "Output\04_MapVis"
            If Not ClipDataForm.TextBox6.Text = "" Then

                SaveFileDialog1.FileName = "visMap_" & Val(ClipDataForm.TextBox5.Text) & "_" & Val(ClipDataForm.TextBox6.Text) & "_" & Val(ClipDataForm.TextBox2.Text) & "_" & Val(ClipDataForm.TextBox3.Text)
            Else
                SaveFileDialog1.FileName = "visMap_01"
            End If
            If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK _
            Then
                tmpImage.Save(SaveFileDialog1.FileName, ImageFormat.Png)
            End If
        Else
            MsgBox("No Image available.")
        End If
    End Sub

    Sub Contextmenustrip1_Click(sender As Object, e As EventArgs) Handles ContextMenuStrip1.ItemClicked
        Dim MenuItemSelected As ToolStripItemClickedEventArgs = e
        Dim Menu_Text = MenuItemSelected.ClickedItem.Text
        If Not Menu_Text = "Export as PNG with transparent background." Then
            Me.Button3_Click(sender, e)
        Else
            If PictureBox1.Visible = True AndAlso Not PictureBox1.Image Is Nothing Then
                Dim AppPath As String = Application.StartupPath() & "\"
                Dim visMap As New Bitmap(PictureBox1.Image)
                SaveFileDialog1.Filter = "PNG Files (*.png*)|*.png"
                If Not (Directory.Exists(AppPath & "Output\04_MapVis")) Then
                    Directory.CreateDirectory(AppPath & "Output\04_MapVis")
                End If
                SaveFileDialog1.InitialDirectory = AppPath & "Output\04_MapVis"
                If Not ClipDataForm.TextBox6.Text = "" Then

                    SaveFileDialog1.FileName = "visMap_" & Val(ClipDataForm.TextBox5.Text) & "_" & Val(ClipDataForm.TextBox6.Text) & "_" & Val(ClipDataForm.TextBox2.Text) & "_" & Val(ClipDataForm.TextBox3.Text)
                Else
                    SaveFileDialog1.FileName = "visMap_01"
                End If
                If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK _
                Then
                    visMap.Save(SaveFileDialog1.FileName, ImageFormat.Png)
                End If
            Else
                MsgBox("No Image available.")
            End If
        End If
    End Sub

    Public Function CaptureImage(SourcePoint As System.Drawing.Point, DestinationPoint As System.Drawing.Point, SelectionRectangle As System.Drawing.Rectangle, FilePath As String) As System.Drawing.Image
        Dim tmpImage As System.Drawing.Image
        Using bitmap As New Bitmap(SelectionRectangle.Width, SelectionRectangle.Height)
            Using g As Graphics = Graphics.FromImage(bitmap)
                g.CopyFromScreen(SourcePoint, DestinationPoint, SelectionRectangle.Size)
            End Using
            'Convert the Image to a PNG
            Dim ms As New MemoryStream()
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
            tmpImage = System.Drawing.Image.FromStream(ms)
            ms.Dispose()
            Return tmpImage
        End Using
        tmpImage.Dispose()
    End Function

    'Equalize
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If equalizeImg = False Then equalizeImg = True
        If Not PictureBox1.Image Is Nothing Then
            Dim bm As New Bitmap(PictureBox1.Image)
            equalize(bm)
            PictureBox1.Image = bm
        End If
    End Sub

    Shared Function equalize(bm As Bitmap) As Bitmap
        'Set all Alpha to 255
        Using fp As New FastPix(bm, True)
            Dim bytes As Byte() = fp.ColorByteArray
            'NOTE: the bytes are in BRGA order, so the first Alpha byte is at index = 3. Example usage:
            For i As Integer = 3 To bytes.Length - 1 Step 4 'Don't Change Alpha (=4)
                If Not bytes(i) = 0 Then
                    bytes(i) = 255
                End If
            Next
            Return bm
        End Using
    End Function

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        PictureBox1.Image = bmOrig
        datacolor = Color.Black
        PictureBox2.BorderStyle = BorderStyle.Fixed3D
        PictureBox3.BorderStyle = BorderStyle.FixedSingle
        PictureBox4.BorderStyle = BorderStyle.FixedSingle
        PictureBox5.BorderStyle = BorderStyle.FixedSingle
        equalizeImg = False
    End Sub

    Private Sub PictureBox2_Click(sender As Object, e As EventArgs) Handles PictureBox2.Click
        datacolor = Color.Black
        PictureBox2.BorderStyle = BorderStyle.Fixed3D
        PictureBox3.BorderStyle = BorderStyle.FixedSingle
        PictureBox4.BorderStyle = BorderStyle.FixedSingle
        PictureBox5.BorderStyle = BorderStyle.FixedSingle
        'recolor existing image
        If Not PictureBox1.Image Is Nothing Then
            Dim bm As New Bitmap(PictureBox1.Image)
            Dim intTransparent As Integer = Color.Transparent.ToArgb
            Using fp As New FastPix(bm, True)
                Dim bytes As Byte() = fp.ColorByteArray
                'NOTE: the bytes are in BRGA order, so the first Alpha byte is at index = 3. Example usage:
                For i As Integer = 3 To bytes.Length - 1 Step 4 'Don't Change Alpha (=4)
                    bytes(i - 3) = datacolor.B
                    bytes(i - 2) = datacolor.G
                    bytes(i - 1) = datacolor.R
                Next
            End Using
            bm.MakeTransparent(Color.Pink)
            PictureBox1.Image = bm
        End If
    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles PictureBox3.Click
        datacolor = Color.Red
        PictureBox2.BorderStyle = BorderStyle.FixedSingle
        PictureBox3.BorderStyle = BorderStyle.Fixed3D
        PictureBox4.BorderStyle = BorderStyle.FixedSingle
        PictureBox5.BorderStyle = BorderStyle.FixedSingle
        'recolor existing image
        If Not PictureBox1.Image Is Nothing Then
            Dim bm As New Bitmap(PictureBox1.Image)
            Dim intTransparent As Integer = Color.Transparent.ToArgb

            Using fp As New FastPix(bm, True)
                Dim bytes As Byte() = fp.ColorByteArray
                'NOTE: the bytes are in BRGA order, so the first Alpha byte is at index = 3. Example usage:
                For i As Integer = 3 To bytes.Length - 1 Step 4 'Don't Change Alpha (=4)
                    bytes(i - 3) = datacolor.B
                    bytes(i - 2) = datacolor.G
                    bytes(i - 1) = datacolor.R
                Next
            End Using
            bm.MakeTransparent(Color.Pink)
            PictureBox1.Image = bm
        End If
    End Sub

    Private Sub PictureBox4_Click(sender As Object, e As EventArgs) Handles PictureBox4.Click
        datacolor = Color.Blue
        PictureBox2.BorderStyle = BorderStyle.FixedSingle
        PictureBox3.BorderStyle = BorderStyle.FixedSingle
        PictureBox4.BorderStyle = BorderStyle.Fixed3D
        PictureBox5.BorderStyle = BorderStyle.FixedSingle
        'recolor existing image
        If Not PictureBox1.Image Is Nothing Then
            Dim bm As New Bitmap(PictureBox1.Image)
            Dim intTransparent As Integer = Color.Transparent.ToArgb

            Using fp As New FastPix(bm, True)
                Dim bytes As Byte() = fp.ColorByteArray
                'NOTE: the bytes are in BRGA order, so the first Alpha byte is at index = 3. Example usage:
                For i As Integer = 3 To bytes.Length - 1 Step 4 'Don't Change Alpha (=4)
                    bytes(i - 3) = datacolor.B
                    bytes(i - 2) = datacolor.G
                    bytes(i - 1) = datacolor.R
                Next
            End Using
            bm.MakeTransparent(Color.Pink)
            PictureBox1.Image = bm
        End If
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        datacolor = Color.Green
        PictureBox2.BorderStyle = BorderStyle.FixedSingle
        PictureBox3.BorderStyle = BorderStyle.FixedSingle
        PictureBox4.BorderStyle = BorderStyle.FixedSingle
        PictureBox5.BorderStyle = BorderStyle.Fixed3D
        'recolor existing image
        If Not PictureBox1.Image Is Nothing Then
            If Not PictureBox1.Image Is Nothing Then
                Dim bm As New Bitmap(PictureBox1.Image)
                Dim intTransparent As Integer = Color.Transparent.ToArgb

                Using fp As New FastPix(bm, True)
                    Dim bytes As Byte() = fp.ColorByteArray
                    'NOTE: the bytes are in BRGA order, so the first Alpha byte is at index = 3. Example usage:
                    For i As Integer = 3 To bytes.Length - 1 Step 4 'Don't Change Alpha (=4)
                        bytes(i - 3) = datacolor.B
                        bytes(i - 2) = datacolor.G
                        bytes(i - 1) = datacolor.R
                    Next
                End Using
                bm.MakeTransparent(Color.Pink)
                PictureBox1.Image = bm
            End If
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If Val(TextBox1.Text) < 1 Then
            TextBox1.Text = 1
        ElseIf Val(TextBox1.Text) > 255 Then
            TextBox1.Text = 255
        End If
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click

        If Button6.Text = "Stats On" Then
            PictureBox6.Visible = False
            Button6.Text = "Stats Off"
        Else
            PictureBox6.Visible = True
            Button6.Text = "Stats On"
        End If
    End Sub

    Private Sub GMapControl1_OnCurrentPositionChanged(point As PointLatLng) Handles GMapControl1.OnCurrentPositionChanged
        If fullyloaded = True Then
            ClipDataForm.syncMaps()
        End If
    End Sub

    Private Sub GMapControl1_OnMapZoomChanged() Handles GMapControl1.OnMapZoomChanged
        If fullyloaded = True Then
            ClipDataForm.syncMaps()
        End If
    End Sub

    Private Sub visualForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        fullyloaded = True
        ClipDataForm.syncMaps(True)
    End Sub

    Private Sub visualForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        PictureBox1.Height = Me.Height - 77
        PictureBox1.Width = Me.Width - 28
        PictureBox6.Height = Me.Height - 77
        PictureBox6.Width = Me.Width - 28
        GMapControl1.Height = Me.Height - 89
        GMapControl1.Width = Me.Width - 40
        IHeight = PictureBox1.Height
        IWidth = PictureBox1.Width
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If ClipDataForm.CheckBox30.Checked = True Then
            RectP1 = Nothing
            RectP2 = Nothing
            If m_poperContainerForForm.Visible = True Then m_poperContainerForForm.Close()
            InitializePhotoView()
            If pList.Count > 0 Then
                ContextMenuForForm.isfirstpaint = True
                UpdatePhotoView()
            End If
        Else
            MsgBox("No photos were collected on map generation. Check 'Photocollection'-Box prior to generating the map.")
        End If
    End Sub

    Private Sub InitializePhotoView()
        Dim count As Integer = 0
        Dim upperleftcorner As GMap.NET.GPoint
        Dim lowerrightcorner As GMap.NET.GPoint
        Dim queryall As Boolean = False
        If Not RectP1 = Nothing AndAlso Not RectP2 = Nothing Then
            If RectP1.Lat <= RectP2.Lat Then 'dimensions are measure inverse, from topleft corner! (y = negative)
                upperleftcorner.Y = GMapControl1.FromLatLngToLocal(RectP1).Y
                lowerrightcorner.Y = GMapControl1.FromLatLngToLocal(RectP2).Y
            Else
                upperleftcorner.Y = GMapControl1.FromLatLngToLocal(RectP2).Y
                lowerrightcorner.Y = GMapControl1.FromLatLngToLocal(RectP1).Y
            End If

            If RectP1.Lng <= RectP2.Lng Then
                upperleftcorner.X = GMapControl1.FromLatLngToLocal(RectP1).X
                lowerrightcorner.X = GMapControl1.FromLatLngToLocal(RectP2).X
            Else
                upperleftcorner.X = GMapControl1.FromLatLngToLocal(RectP2).X
                lowerrightcorner.X = GMapControl1.FromLatLngToLocal(RectP1).X
            End If
        Else
            queryall = True
        End If
        pList.Clear()
        For Each entry As KeyValuePair(Of GMap.NET.GPoint, PhotoRef) In photoDict
            If queryall = True OrElse (entry.Key.Y < upperleftcorner.Y AndAlso entry.Key.Y > lowerrightcorner.Y AndAlso entry.Key.X < lowerrightcorner.X AndAlso entry.Key.X > upperleftcorner.X) Then
                If Not entry.Value.views = -1 Then
                    count = count + 1
                    pList.Add(entry.Value)
                End If
            End If
        Next
        pList.Sort(Function(x, y) x.views.CompareTo(y.views))
        pListCursor = pList.Count - 1
    End Sub

    Public Shared Sub UpdatePhotoView()
        If pList.Count > 0 Then
            visualForm.GMapControl1.Overlays(0).Markers.Clear()
            Dim PPoint As GMap.NET.PointLatLng
            PPoint.Lat = pList(visualForm.pListCursor).lat
            PPoint.Lng = pList(visualForm.pListCursor).lng
            Dim SPoint As System.Drawing.Point
            SPoint.Y = visualForm.GMapControl1.FromLatLngToLocal(PPoint).Y + visualForm.Location.Y + 40
            SPoint.X = visualForm.GMapControl1.FromLatLngToLocal(PPoint).X + visualForm.Location.X + 25
            visualForm.m_poperContainerForForm.Show(SPoint)
            visualForm.GMapControl1.Overlays(0).Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(PPoint))
        End If
    End Sub

    Private Sub Picturebox6_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox6.MouseDown
        isMouseDown = True
    End Sub

    Private Sub Picturebox6_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox6.MouseMove
        Dim rectbottomleft As PointLatLng
        Dim recttopright As PointLatLng
        If ClipDataForm.CheckBox30.Checked = True AndAlso (e.Button = MouseButtons.Right) AndAlso isMouseDown = True Then
            If firstmarkerset = False Then
                RectP1 = Nothing
                RectP2 = Nothing
                RectP1 = New PointLatLng(GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat, GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng)
                firstmarkerset = True
            End If
            RectP2 = New PointLatLng(GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat, GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng)
            GMapControl1.Overlays(2).Polygons.Clear()
            rectbottomleft.Lat = RectP1.Lat
            rectbottomleft.Lng = RectP1.Lng
            recttopright.Lat = RectP2.Lat
            recttopright.Lng = RectP2.Lng

            Dim pointsRect As New List(Of PointLatLng)
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))

            Dim polygon2 As New GMapPolygon(pointsRect, "RectangleSel")
            polygon2.Fill = New SolidBrush(Color.FromArgb(0, Color.Red))
            polygon2.Stroke = New Pen(Color.Red, 0.25)
            GMapControl1.Overlays(2).Polygons.Add(polygon2)
        End If
    End Sub

    Private Sub Picturebox6_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox6.MouseUp
        isMouseDown = False
        If firstmarkerset = True Then
            firstmarkerset = False
            InitializePhotoView()
            If pList.Count > 0 Then
                ContextMenuForForm.isfirstpaint = True
                UpdatePhotoView()
            Else
                GMapControl1.Overlays(2).Polygons.Clear()
                GMapControl1.Overlays(0).Markers.Clear()
            End If
        End If
    End Sub

    Private Sub Picturebox6_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox6.MouseLeave
        isMouseDown = False
        firstmarkerset = False
    End Sub

    Private Sub PictureBox6_Click(sender As Object, e As EventArgs) Handles PictureBox6.Click
        If pList.Count > 0 Then
            If ContextMenuForForm.isfirstpaint = False Then

                ContextMenuForForm.isfirstpaint = True
                UpdatePhotoView()
            End If

        End If
    End Sub

    Public Shared Sub ColorToHSV(color As Color, ByRef hue As Double, ByRef saturation As Double, ByRef value As Double)
        Dim max As Integer = Math.Max(color.R, Math.Max(color.G, color.B))
        Dim min As Integer = Math.Min(color.R, Math.Min(color.G, color.B))

        hue = color.GetHue()
        saturation = If((max = 0), 0, 1.0 - (1.0 * min / max))
        value = max / 255.0
    End Sub
End Class

Public Class FastPix

    '(c) Vic Joseph (Boops Boops) 2009. Correction for UseByteData applied in October 2013.

    'The FastPix class provides fast pixel processing of bitmaps. It encapsulates the LockBits/UnlockBits methods.
    'FastPix is designed to deal only with the default GDI+ pixel format 32bppArgb and the format 32bppPArgb,
    'but it includes a method for converting bitmaps.

    'To use FastPix, unzip it to your hard drive. 
    'To add it to your project in VisualStudio, select the Project Menu then Add Existing Item...
    'Then navigate to fastpix.vb on your drive.

    'The main uses of FastPix are as follows.
    '1. To convert a bitmap to a 32bppPArgb (32 bit premultiplied) format, use:
    '   FastPix.Convert(myBitmap, PixelFormat.Format32bppPArgb)
    '   Note: for the canonical format 32bppArgb, it may be more convenient to do something like this:
    '   Dim myBitmap as New Bitmap("D:\Pictures\ExistingImage.jpg")

    '1. A fast substitute for Bitmap.GetPixel and Bitmap.SetPixel. Example usage:

    '   Using fp as New FastPix(myBitmap)
    '       Dim myColor As Color =  fp.GetPixel(x, y)
    '       fp.SetPixel(x, y, Color.Orange)
    '   End Using

    '2. The class provides a PixelArray property, which is a representation of the bitmap as an array of integers.
    '   (Actual performance will naturally also depend on other aspects of your processing loop.) Example usage:

    '   Using fp as New FastPix(myBitmap)
    '       'Make a local reference to the array; it is roughly 4x as fast as direct references to fp.PixelArray:
    '       Dim pixels as Integer() = fp.PixelArray

    '           For i as integer = 0 to pixels.Length - 1

    '               'example: substitute a color
    '               if pixels(i) = Color.Red.ToArgb then pixels(i) = Color.Blue.ToArgb

    '               'example: invert the color
    '               pixels(i) = pixels(i) AND $HFFFFFF

    '           Next
    '
    '   End Using

    '3. The class also provides an optional Byte array property, which represents the bitmap as a 1D array of bytes.
    'It may be useful when individual color bytes have to be modified. 
    'NOTE: the bytes are in BRGA order, so the first Alpha byte is at index = 3. Example usage:

    '    Using fp As New FastPix(myBitmap, True)
    '       Dim bytes As byte() = fp.ColorByteArray

    '       'Modify the Alpha bytes to make the bitmap 50% transparent:
    '       For i As Integer = 3 to bytes.Length - 1 Step 4    
    '           bytes(i) = 127
    '       Next

    '   End Using

    Implements IDisposable

    Private _bmp As Bitmap
    Private _w, _h As Integer
    Private _disposed As Boolean = False
    Private _bmpData As BitmapData
    Private _PixelData As Integer()
    Private _ByteData As Byte()
    Private _UseByteArray As Boolean = False

    'An array of integers representing the pixel data of a bitmap.
    Public Property PixelArray() As Integer()
        Get
            Return _PixelData
        End Get
        Set(ByVal value As Integer())
            _PixelData = value
        End Set
    End Property

    'An array of bytes representing the pixel data of a bitmap.
    Public Property ColorByteArray() As Byte()
        Get
            Return _ByteData
        End Get
        Set(ByVal value As Byte())
            _ByteData = value
        End Set
    End Property

#Region "Constructors/Destructors"

    'The New Sub contains the LockBits+Marshall.Copy code.
    Public Sub New(ByRef bmp As Bitmap, Optional ByVal UseByteArray As Boolean = False)
        _UseByteArray = UseByteArray

        Dim pFSize As Integer = Bitmap.GetPixelFormatSize(bmp.PixelFormat)

        If pFSize <> 32 OrElse bmp.PixelFormat = PixelFormat.Indexed Then
            Throw New FormatException _
                ("FastPix is designed to deal only with 32-bit pixel non-indexed formats. Your bitmap has " _
                & pFSize & "-bit pixels. You can convert it using FastPix.ConvertFormat.")
        Else
            'Convert the bitap to a 1 dimensional array of pixel data:
            _w = bmp.Width
            _h = bmp.Height
            _bmp = bmp
            Dim bmpRect As New Rectangle(0, 0, _w, _h)

            _bmpData = _bmp.LockBits(bmpRect, ImageLockMode.ReadWrite, _bmp.PixelFormat)

            If UseByteArray Then
                ReDim _ByteData(_w * _h * 4 - 1)
                Marshal.Copy(_bmpData.Scan0, _ByteData, 0, _ByteData.Length)
            Else
                ReDim _PixelData(_w * _h - 1)
                Marshal.Copy(_bmpData.Scan0, _PixelData, 0, _PixelData.Length)
            End If
        End If
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    'The Dispose Sub contains the Marshal.Copy+UnlockBits code.
    Protected Overloads Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            If _UseByteArray Then
                If _ByteData IsNot Nothing Then Marshal.Copy(_ByteData, 0, _bmpData.Scan0, _ByteData.Length)
            Else
                If _PixelData IsNot Nothing Then Marshal.Copy(_PixelData, 0, _bmpData.Scan0, _PixelData.Length)
            End If
            _bmp.UnlockBits(_bmpData)
            _ByteData = Nothing
            _PixelData = Nothing
            _bmpData = Nothing
            _disposed = True
        End If
    End Sub

#End Region

#Region "Public Methods"

    'Return the color of any pixel in the bitmap.
    Public Function GetPixel(ByVal x As Integer, ByVal y As Integer) As Color
        Return Color.FromArgb(_PixelData(y * _w + x))
    End Function

    'Set the color of any pixel in the bitmap.
    Public Sub SetPixel(ByVal x As Integer, ByVal y As Integer, ByVal clr As Color)
        _PixelData(y * _w + x) = clr.ToArgb
    End Sub

    'Standardize the bitmap format for use with FastPix.
    Public Shared Sub ConvertFormat(ByRef bmp As Bitmap, _
                            Optional ByVal TargetFormat As PixelFormat = PixelFormat.Format32bppArgb)
        Try
            Dim bmp2 As New Bitmap(bmp.Width, bmp.Height, TargetFormat)
            Using g As Graphics = Graphics.FromImage(bmp2)
                bmp.SetResolution(96, 96)
                g.DrawImageUnscaled(bmp, Point.Empty)
            End Using
            bmp = bmp2
        Catch
            Throw New FormatException("FastPix could not convert the bitmap to the standard format.")
        End Try
    End Sub


#End Region

End Class



'Definition of advanced photo data (still smaller than Flickr.Net.Photo)
Public Class PhotoRef
    Private _photoid As Long
    Public Property photoid() As Long
        Get
            Return _photoid
        End Get
        Set(value As Long)
            _photoid = value
        End Set
    End Property

    Private _views As Integer
    Public Property views() As Integer
        Get
            Return _views
        End Get
        Set(value As Integer)
            _views = value
        End Set
    End Property

    Private _pUrl As String
    Public Property pUrl() As String
        Get
            Return _pUrl
        End Get
        Set(value As String)
            _pUrl = value
        End Set
    End Property
    Private _lat As Double
    Public Property lat() As Double
        Get
            Return _lat
        End Get
        Set(value As Double)
            _lat = value
        End Set
    End Property
    Private _lng As Double
    Public Property lng() As Double
        Get
            Return _lng
        End Get
        Set(value As Double)
            _lng = value
        End Set
    End Property
End Class

