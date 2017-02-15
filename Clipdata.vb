Imports System.IO
Imports GMap.NET
Imports GMap.NET.WindowsForms
Imports GMap.NET.WindowsForms.Markers
Imports GMap.NET.WindowsForms.ToolTips
Imports System.Text
Imports System.Net
Imports DotSpatial.Data
Imports DotSpatial.Topology


Public Class ClipDataForm
    Public AppPath As String = Application.StartupPath() & "\"
    Public strPath As String = ""
    Public Event updatemap(sender As System.Object, e As System.EventArgs)
    Public selectedmarker As GMapMarker = Nothing
    Public markerIsSelected As Boolean = False
    Public isMouseDown As Boolean
    Public firstmarkerset As Boolean
    Public filename_data As New List(Of SourceData)
    Public MarkerVisible As Boolean = True 'Red Cross Marker GMapNet
    Public maploaded As Boolean = False
    Public startDataset As SourceData = Nothing
    Public SetStartDatasetName As String = "Greater Toronto Area"
    Public formfullyloaded As Boolean = False 'set to true after form is fully loaded
    Public dataselall As Boolean = True '..true, if all datasets are to be exported, false if user made selection
    Public hash As HashSet(Of Long) = New HashSet(Of Long)
    Public hashUser As HashSet(Of String) = New HashSet(Of String)
    Public hashTags As HashSet(Of String) = New HashSet(Of String)
    'Public values for data filter
    Public Operator1, Operator2 As Integer
    Public SearchTitle1, SearchMtags1, SearchTags1, SearchTitle2, SearchMtags2, SearchTags2, SearchTitle3, SearchMtags3, SearchTags3 As Boolean
    Public searchFullWords As Boolean = True
    Public DataFiltering As Boolean = False

    'global variables for speed in point-in-polygon test
    '  int    polyCorners  =  how many corners the polygon has (no repeats)
    '  float  polyX[]      =  horizontal coordinates of corners
    '  float  polyY[]      =  vertical coordinates of corners
    '  float  x, y         =  point to be tested
    Public polyCorners As Integer 'Contains all point coordinates of selected shapefile(s)
    Public polyX() As Double
    Public polyY() As Double
    Public xP As Double
    Public yP As Double
    Public b_xMax, b_xMin, b_yMax, b_yMin As Double

    Dim featuresShape As New List(Of List(Of PointLatLng)) 'Feature-List with Point-Lists
    Public ShapefilePoly As IFeatureSet = Nothing
    Public Shapecount As Integer = 1
    Public Singleextract As Boolean = False
    '  float  constant[] = storage for precalculated constants (same size as polyX)
    '  float  multiple[] = storage for precalculated multipliers (same size as polyX)
    Public constant() As Double
    Public multiple() As Double

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        maploaded = False
        MarkerVisible = True
        Dim strFileName As String = ""
        Dim folderDialogBox As New FolderBrowserDialog()
        filename_data.Clear()
        strPath = Me.TextBox1.Text.Replace("...", Application.StartupPath())
        If Not (Directory.Exists(strPath)) Then
            MsgBox("Folder does not exist.")
            Exit Sub
        Else
            If Not strPath.EndsWith("\") Then
                strPath = strPath & "\"
            End If
        End If
        Me.Refresh()

        'Load files from subdirectories
        Dim files As String()
        Dim filename As String
        Dim di As New DirectoryInfo(strPath)
        Dim filenamepath As String
        ' Get a reference to each directory in that directory.
        Dim diArr As DirectoryInfo() = di.GetDirectories()
        Dim leftpart As String
        ' Populate Combobox.
        Dim dri As DirectoryInfo
        Dim currentSourceData As New SourceData

        'Load Multifolder
        For Each dri In diArr
            currentSourceData = New SourceData
            If File.Exists(strPath & dri.Name & "\settings.txt") Then
                currentSourceData.TotalPhotos = 0
                currentSourceData.filename = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "profilename")
                currentSourceData.bottomlat = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "bottomlat")
                currentSourceData.leftlong = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "leftlong")
                currentSourceData.toplat = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "toplat")
                currentSourceData.rightlong = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "rightlong")
                If HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "querytype") = "upload_time" Then
                    currentSourceData.fromDate = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "minuploaddate")
                    currentSourceData.toDate = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "maxuploaddate")
                Else
                    currentSourceData.fromDate = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "mintakendate")
                    currentSourceData.toDate = HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "maxtakendate")
                End If
                currentSourceData.photosPerFile = Val(HelperFunctions.GetSettingItem(strPath & dri.Name & "\settings.txt", "maxquery"))
                files = Directory.GetFiles(strPath & dri.Name & "\")

                'Set StartItem to Greater Toronto Area, if exists
                If currentSourceData.filename = SetStartDatasetName Then startDataset = currentSourceData
                For Each filename In files
                    filenamepath = filename
                    filename = filename.Substring(filename.LastIndexOf("\"c) + 1)
                    If filename.Length > 13 Then
                        If filename.Substring(filename.Length - 13) = "_settings.txt" Then
                            filename = "settings.txt"
                        End If
                    End If
                    If Not (filename = "GridCoordinates.txt") And Not (filename.Substring(0, 3) = "log") And Not filename.Contains("settings") Then
                        currentSourceData.datafiles.Add(filenamepath)
                        If InStr(currentSourceData.photosPerFile.ToString, filename) Then
                            currentSourceData.TotalPhotos = currentSourceData.TotalPhotos + currentSourceData.photosPerFile
                        Else
                            leftpart = Strings.Left(filename, filename.LastIndexOf("_"))
                            currentSourceData.TotalPhotos = currentSourceData.TotalPhotos + Val(Strings.Right(leftpart, Strings.Len(leftpart) - leftpart.LastIndexOf("_") - 1))
                        End If
                    End If
                Next
                filename_data.Add(currentSourceData)
            End If
        Next dri
        If startDataset Is Nothing Then
            startDataset = filename_data(0)
        End If
        'Load Single Folder (if no subfolders are found)
        If File.Exists(strPath & "settings.txt") And diArr.Length = 0 Then
            currentSourceData = New SourceData
            currentSourceData.filename = HelperFunctions.GetSettingItem(strPath & "settings.txt", "profilename")
            currentSourceData.bottomlat = HelperFunctions.GetSettingItem(strPath & "settings.txt", "bottomlat")
            currentSourceData.leftlong = HelperFunctions.GetSettingItem(strPath & "settings.txt", "leftlong")
            currentSourceData.toplat = HelperFunctions.GetSettingItem(strPath & "settings.txt", "toplat")
            currentSourceData.rightlong = HelperFunctions.GetSettingItem(strPath & "settings.txt", "rightlong")
            files = Directory.GetFiles(strPath)
            For Each filename In files
                filenamepath = filename
                filename = filename.Substring(filename.LastIndexOf("\"c) + 1)
                If filename.Length > 13 Then
                    If filename.Substring(filename.Length - 13) = "_settings.txt" Then
                        filename = "settings.txt"
                    End If
                End If
                If Not (filename = "GridCoordinates.txt") And Not (filename.Substring(0, 3) = "log") And Not (filename = "settings.txt") Then
                    currentSourceData.datafiles.Add(filenamepath)
                End If
            Next
            filename_data.Add(currentSourceData)
        End If

        Dim countFiles As Integer = 0
        Dim countPhotos As Long = 0
        For Each datafilesList As SourceData In filename_data
            countFiles = countFiles + datafilesList.datafiles.Count
            countPhotos = countPhotos + datafilesList.TotalPhotos
        Next

        If Not countFiles = 0 Then
            Me.Label1.Text = "Number of files: " & countFiles & " (" & Math.Round(countPhotos, 0).ToString("N0") & " Photos)"
            Me.Refresh()
            If startDataset IsNot Nothing Then
                Me.TextBox6.Text = startDataset.bottomlat
                Me.TextBox3.Text = startDataset.leftlong
                Me.TextBox5.Text = startDataset.toplat
                Me.TextBox2.Text = startDataset.rightlong
                Me.TextBox9.Text = startDataset.filename & "_Clip"
            End If
            RaiseEvent updatemap(Button3, System.EventArgs.Empty)
        End If
        maploaded = True
        savesettings()
    End Sub

    Private Sub ClipDataForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.RadioButton1.Checked = True
        Me.min_date.Enabled = False
        Me.max_date.Enabled = False
        Label15.ForeColor = SystemColors.InactiveCaptionText
        Label24.ForeColor = SystemColors.InactiveCaptionText
        RadioButton3.ForeColor = SystemColors.InactiveCaptionText
        RadioButton4.ForeColor = SystemColors.InactiveCaptionText
        RadioButton3.Checked = False
        RadioButton4.Checked = False
    End Sub



    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Me.updatemap
        Dim centerlat, centerlong, toplat, bottomlat, leftlong, rightlong As Double

        If TextBox6.Text = "" Or CheckBox2.Checked = True Then
            toplat = 50.8475729536539
            bottomlat = 10.8333059836425
            rightlong = -63.6328125
            leftlong = -129.375
            centerlong = Math.Min(leftlong + (rightlong - leftlong) / 2, 180) 'Add half the distance to leftlong
            centerlat = Math.Min(bottomlat + (toplat - bottomlat) / 2, 90) 'Add half the distance to bottomlat
        Else
            toplat = Val(Replace(TextBox5.Text, ",", "."))
            bottomlat = Val(Replace(TextBox6.Text, ",", "."))
            rightlong = Val(Replace(TextBox2.Text, ",", "."))
            leftlong = Val(Replace(TextBox3.Text, ",", "."))
            TextBox5.Text = toplat
            TextBox6.Text = bottomlat
            TextBox2.Text = rightlong
            TextBox3.Text = leftlong
            centerlong = Math.Min(leftlong + (rightlong - leftlong) / 2, 180)
            centerlat = Math.Min(bottomlat + (toplat - bottomlat) / 2, 90)
        End If


        With GMapControl1
            '.SetCurrentPositionByKeywords("USA")
            .MapType = GMap.NET.MapType.BingMap
            .MinZoom = 1
            .MaxZoom = 25
            .Zoom = 10
            .Manager.Mode = GMap.NET.AccessMode.ServerAndCache
            .Position = New GMap.NET.PointLatLng(centerlat, centerlong)
            .DragButton = MouseButtons.Left
            .CanDragMap = True
        End With

        'Reset Map and Draw Selection Rectangle
        GMapControl1.Overlays.Clear()
        Dim overlayOne As New GMapOverlay(GMapControl1, "OverlayOne")
        Dim MarkerOverlay As New GMapOverlay(GMapControl1, "Marker")
        Dim points As New List(Of PointLatLng)
        Dim polygon_red As New GMapPolygon(points, "Rectangle")
        HelperFunctions.InitialDrawMarkers(centerlat, centerlong, toplat, bottomlat, leftlong, rightlong, points, polygon_red, startDataset, overlayOne, MarkerOverlay)


        'Draw all other markers / Draws rectangles for each loaded dataset
        Dim i As Integer = 0
        For Each x As SourceData In filename_data
            'First Draw red & grey rectangles
            If x.toDate.Length >= 4 And Val(x.toDate.Substring(x.toDate.Length - 4)) = 2015 Then
                centerlong = Math.Min(x.leftlong + (x.rightlong - x.leftlong) / 2, 180) 'Addiere halben Abstand zu leftlong
                centerlat = Math.Min(x.bottomlat + (x.toplat - x.bottomlat) / 2, 90) 'Addiere halben Abstand zu bottomlat

                i = i + 1
                toplat = x.toplat
                bottomlat = x.bottomlat
                rightlong = x.rightlong
                leftlong = x.leftlong

                points = New List(Of PointLatLng)
                points.Add(New PointLatLng(toplat, leftlong))
                points.Add(New PointLatLng(toplat, rightlong))
                points.Add(New PointLatLng(bottomlat, rightlong))
                points.Add(New PointLatLng(bottomlat, leftlong))
                points.Add(New PointLatLng(toplat, leftlong))

                Dim polygon_grey As GMapPolygon = Nothing
                If x.toDate.Length >= 4 And Val(x.toDate.Substring(x.toDate.Length - 4)) = 2015 Then
                    polygon_red = New GMapPolygon(points, x.filename)
                    polygon_red.Fill = New SolidBrush(Color.FromArgb(0, Color.White))
                    polygon_red.Stroke = New Pen(Color.Red, 0.25)
                Else
                    polygon_grey = New GMapPolygon(points, x.filename)
                    polygon_grey.Fill = New SolidBrush(Color.FromArgb(0, Color.White))
                    polygon_grey.Stroke = New Pen(Color.DarkGray, 0.25)
                End If

                MarkerOverlay.Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerCross(New PointLatLng(centerlat, centerlong)))
                MarkerOverlay.Markers.Item(MarkerOverlay.Markers.Count - 1).ToolTip = New GMapToolTip(MarkerOverlay.Markers.Item(MarkerOverlay.Markers.Count - 1))
                MarkerOverlay.Markers.Item(MarkerOverlay.Markers.Count - 1).ToolTipText = x.filename & Environment.NewLine & "Period: " & x.fromDate & " to " & x.toDate & Environment.NewLine & Math.Round(x.TotalPhotos, 0).ToString("N0") & " Photos"
                If Not IsNothing(polygon_grey) Then overlayOne.Polygons.Add(polygon_grey)
                If Not IsNothing(polygon_red) Then overlayOne.Polygons.Add(polygon_red)
                GMapControl1.Overlays.Add(MarkerOverlay)
            End If
        Next
        For Each x As SourceData In filename_data
            'then Draw all blue rectangles
            If x.toDate.Length >= 4 And Val(x.toDate.Substring(x.toDate.Length - 4)) = 2017 Then
                centerlong = Math.Min(x.leftlong + (x.rightlong - x.leftlong) / 2, 180) 'Addiere halben Abstand zu leftlong
                centerlat = Math.Min(x.bottomlat + (x.toplat - x.bottomlat) / 2, 90) 'Addiere halben Abstand zu bottomlat

                'tooltip = New GMapToolTip(Markers)
                i = i + 1
                toplat = x.toplat
                bottomlat = x.bottomlat
                rightlong = x.rightlong
                leftlong = x.leftlong

                points = New List(Of PointLatLng)
                points.Add(New PointLatLng(toplat, leftlong))
                points.Add(New PointLatLng(toplat, rightlong))
                points.Add(New PointLatLng(bottomlat, rightlong))
                points.Add(New PointLatLng(bottomlat, leftlong))
                points.Add(New PointLatLng(toplat, leftlong))


                Dim polygon_blue As GMapPolygon = Nothing
                polygon_blue = New GMapPolygon(points, x.filename)
                polygon_blue.Fill = New SolidBrush(Color.FromArgb(0, Color.White))
                polygon_blue.Stroke = New Pen(Color.Blue, 0.25)

                MarkerOverlay.Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerCross(New PointLatLng(centerlat, centerlong)))
                MarkerOverlay.Markers.Item(MarkerOverlay.Markers.Count - 1).ToolTip = New GMapToolTip(MarkerOverlay.Markers.Item(MarkerOverlay.Markers.Count - 1))
                MarkerOverlay.Markers.Item(MarkerOverlay.Markers.Count - 1).ToolTipText = x.filename & Environment.NewLine & "Period: " & x.fromDate & " to " & x.toDate & Environment.NewLine & Math.Round(x.TotalPhotos, 0).ToString("N0") & " Photos"
                If Not IsNothing(polygon_blue) Then overlayOne.Polygons.Add(polygon_blue)
                GMapControl1.Overlays.Add(MarkerOverlay)
            End If
        Next

    End Sub

    Private Sub GMapControl1_MouseDown(sender As Object, e As MouseEventArgs) Handles GMapControl1.MouseDown
        isMouseDown = True
        For Each m As GMapMarker In GMapControl1.Overlays(1).Markers
            If m.IsMouseOver = True Then
                selectedmarker = m
                markerIsSelected = True
            End If
        Next
        If markerIsSelected = False Then selectedmarker = Nothing

    End Sub

    Private Sub GMapControl1_MouseMove(sender As Object, e As MouseEventArgs) Handles GMapControl1.MouseMove
        Dim rectbottomleft As PointLatLng
        Dim recttopright As PointLatLng
        Dim latDist, longDist As Double

        If (e.Button = MouseButtons.Left) And isMouseDown = True And markerIsSelected = True Then
            selectedmarker.Position = GMapControl1.FromLocalToLatLng(e.X, e.Y)

            GMapControl1.Overlays(2).Polygons.Clear()

            rectbottomleft.Lat = GMapControl1.Overlays(1).Markers.Item(0).Position.Lat
            rectbottomleft.Lng = GMapControl1.Overlays(1).Markers.Item(0).Position.Lng
            recttopright.Lat = GMapControl1.Overlays(1).Markers.Item(1).Position.Lat
            recttopright.Lng = GMapControl1.Overlays(1).Markers.Item(1).Position.Lng

            Dim pointsRect As New List(Of PointLatLng)
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))

            Dim polygon2 As New GMapPolygon(pointsRect, "RectangleSel")
            polygon2.Fill = New SolidBrush(Color.FromArgb(50, Color.Red))
            polygon2.Stroke = New Pen(Color.Red, 0.25)
            GMapControl1.Overlays(2).Polygons.Add(polygon2)

            If Val(rectbottomleft.Lat) < Val(recttopright.Lat) Then
                TextBox6.Text = rectbottomleft.Lat
                TextBox5.Text = recttopright.Lat
            Else
                TextBox6.Text = recttopright.Lat
                TextBox5.Text = rectbottomleft.Lat
            End If

            If Val(rectbottomleft.Lng) < Val(recttopright.Lng) Then
                TextBox3.Text = rectbottomleft.Lng
                TextBox2.Text = recttopright.Lng
            Else
                TextBox3.Text = recttopright.Lng
                TextBox2.Text = rectbottomleft.Lng
            End If
            latDist = ((Val(TextBox5.Text) - Val(TextBox6.Text)) * 60 * 1852) / 1000 'Distanz zwischen den zwei Lat-Werten in km
            longDist = ((Val(TextBox2.Text) - Val(TextBox3.Text)) * 48 * 1852) / 1000 'Distanz zwischen den zwei Long-Werten in km
            TextBox7.Text = Math.Round(longDist, 2).ToString & " km"
            TextBox8.Text = Math.Round(latDist, 2).ToString & " km"

        ElseIf (e.Button = MouseButtons.Right) And isMouseDown = True And markerIsSelected = False Then
            If firstmarkerset = False Then
                GMapControl1.Overlays(1).Markers.Clear()
                GMapControl1.Overlays(1).Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(New PointLatLng(GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat, GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng)))
                GMapControl1.Overlays(1).Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(New PointLatLng(GMapControl1.FromLocalToLatLng(e.X, e.Y).Lat, GMapControl1.FromLocalToLatLng(e.X, e.Y).Lng)))
                firstmarkerset = True
            End If

            GMapControl1.Overlays(1).Markers.Item(1).Position = GMapControl1.FromLocalToLatLng(e.X, e.Y)
            GMapControl1.Overlays(2).Polygons.Clear()

            rectbottomleft.Lat = GMapControl1.Overlays(1).Markers.Item(0).Position.Lat
            rectbottomleft.Lng = GMapControl1.Overlays(1).Markers.Item(0).Position.Lng
            recttopright.Lat = GMapControl1.Overlays(1).Markers.Item(1).Position.Lat
            recttopright.Lng = GMapControl1.Overlays(1).Markers.Item(1).Position.Lng

            Dim pointsRect As New List(Of PointLatLng)
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))

            Dim polygon2 As New GMapPolygon(pointsRect, "RectangleSel")
            polygon2.Fill = New SolidBrush(Color.FromArgb(50, Color.Red))
            polygon2.Stroke = New Pen(Color.Red, 0.25)
            GMapControl1.Overlays(2).Polygons.Add(polygon2)

            If Val(rectbottomleft.Lat) < Val(recttopright.Lat) Then
                TextBox6.Text = rectbottomleft.Lat
                TextBox5.Text = recttopright.Lat
            Else
                TextBox6.Text = recttopright.Lat
                TextBox5.Text = rectbottomleft.Lat
            End If

            If Val(rectbottomleft.Lng) < Val(recttopright.Lng) Then
                TextBox3.Text = rectbottomleft.Lng
                TextBox2.Text = recttopright.Lng
            Else
                TextBox3.Text = recttopright.Lng
                TextBox2.Text = rectbottomleft.Lng
            End If
            latDist = Math.Round(((Val(TextBox5.Text) - Val(TextBox6.Text)) * 60 * 1852) / 1000) 'Distance between two lat-values (simple calculation, not accurate. Should substitute with GetDistanceBetweenPoints (Haversine Formula)
            longDist = Math.Round(((Val(TextBox2.Text) - Val(TextBox3.Text)) * 48 * 1852) / 1000) 'Distance between two lat-values (simple calculation, not accurate. Should substitute with GetDistanceBetweenPoints (Haversine Formula)
            TextBox7.Text = Math.Round(longDist, 2).ToString & " km"
            TextBox8.Text = Math.Round(latDist, 2).ToString & " km"
        End If
    End Sub

    Private Sub GMapControl1_MouseUp(sender As Object, e As MouseEventArgs) Handles GMapControl1.MouseUp
        isMouseDown = False
        selectedmarker = Nothing
        markerIsSelected = False
        firstmarkerset = False

    End Sub

    Private Sub GMapControl1_MouseLeave(sender As Object, e As EventArgs) Handles GMapControl1.MouseLeave
        isMouseDown = False
        selectedmarker = Nothing
        markerIsSelected = False
        firstmarkerset = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        Dim rectbottomleft As PointLatLng
        Dim recttopright As PointLatLng
        GMapControl1.Overlays(2).Polygons.Clear()

        rectbottomleft.Lat = GMapControl1.FromLocalToLatLng(GMapControl1.DisplayRectangle.Left, GMapControl1.DisplayRectangle.Bottom).Lat
        rectbottomleft.Lng = GMapControl1.FromLocalToLatLng(GMapControl1.DisplayRectangle.Left, GMapControl1.DisplayRectangle.Bottom).Lng
        recttopright.Lat = GMapControl1.FromLocalToLatLng(GMapControl1.DisplayRectangle.Top, GMapControl1.DisplayRectangle.Right).Lat
        recttopright.Lng = GMapControl1.FromLocalToLatLng(GMapControl1.DisplayRectangle.Top, GMapControl1.DisplayRectangle.Right).Lng

        Dim pointsRect As New List(Of PointLatLng)
        pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))
        pointsRect.Add(New PointLatLng(recttopright.Lat, recttopright.Lng))
        pointsRect.Add(New PointLatLng(rectbottomleft.Lat, recttopright.Lng))
        pointsRect.Add(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng))
        pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))

        Dim polygon2 As New GMapPolygon(pointsRect, "RectangleSel")
        polygon2.Fill = New SolidBrush(Color.FromArgb(50, Color.Red))
        polygon2.Stroke = New Pen(Color.Red, 0.25)
        GMapControl1.Overlays(2).Polygons.Add(polygon2)
    End Sub

    Private Sub TextBox3_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs)
        'Add Validating
    End Sub

    Private Sub TextBox2_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs)
        'Add Validating
    End Sub

    Private Sub TextBox6_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs)
        'Add Validating
    End Sub

    Private Sub TextBox5_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs)
        'Add Validating
    End Sub

    Private Sub searchexport(export As Boolean)
        Dim InputDir As String = TextBox1.Text.Replace("...", Application.StartupPath())
        Dim outputname As String = TextBox9.Text
        Dim outputdir As String = AppPath & "Output\03_ClippedData\" & outputname & "\"
        Dim filenamepath As String
        Dim newfilenamepath As String = Nothing
        Dim visMap As New Bitmap(visualForm.PictureBox1.Width, visualForm.PictureBox1.Height)
        Dim photocollection As Boolean = CheckBox30.Checked
        Dim userOriginExport As Boolean = CheckBox29.Checked
        Dim UserLocationGeocodeDict As Dictionary(Of String, KeyValuePair(Of Double, Double)) = New Dictionary(Of String, KeyValuePair(Of Double, Double))(System.StringComparer.OrdinalIgnoreCase) 'Dictionary of String-Location to lat/lng values
        Dim maptouristslocals As Boolean = CheckBox32.Checked
        Dim mapPhotosFromLocals As Boolean = CheckBox33.Checked

        Dim PhotoIDc As Long = 0
        Dim Tagsc As Long = 0
        Dim UserIDc As String = Nothing
        Dim Views As Integer = 0
        Dim PhotoURL As String = ""
        Dim NoClip As Boolean = CheckBox2.Checked
        Dim NoStatistics As Boolean = CheckBox16.Checked
        Dim filtertext1 As String = ""
        Dim filtertext2 As String = ""
        Dim filtertext3 As String = ""
        Dim retainfolderstructure As Boolean = CheckBox27.Checked
        Dim estimateUnique As Boolean = False
        Dim estPhotosBase As Long = 0
        Dim estHashtagBase As Long = 0
        Dim rectbottomleft, recttopright As PointLatLng
        Dim outputfile As System.IO.TextWriter = Nothing
        Dim countlines As Long = 0
        Dim countlines_sich As Long = 0
        Dim countnewfiles As Integer = 0
        Dim header_line As String
        Dim header_line_written As Boolean = False
        Dim minDate As System.DateTime = min_date.Value
        Dim maxDate As System.DateTime = max_date.Value
        Dim PDate As System.DateTime = Nothing



        'Initialize Graphics/Point Map
        Dim grap As Drawing.Graphics = Drawing.Graphics.FromImage(visMap)
        grap.Clear(Drawing.Color.Pink)
        visMap.MakeTransparent(Color.Pink)
        visualForm.PictureBox1.Visible = True
        visualForm.ComboBox2.Enabled = False
        Label5.Visible = True
        If CheckBox15.Checked Then
            'Precalc Coordinates for Selected Area
            Label5.Text = "Precalculating index ..."
            Me.Refresh()
            visualForm.precalcValues(visualForm.PictureBox1.Height, visualForm.PictureBox1.Width)
            Me.Refresh()
        End If
        visualForm.startalpha = Val(visualForm.TextBox1.Text)
        visualForm.stepInc = visualForm.NumericUpDown1.Value
        visualForm.TextBox1.Enabled = False
        visualForm.NumericUpDown1.Enabled = False
        visualForm.ComboBox2.Enabled = False
        visualForm.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedToolWindow

        'Prepare list of datasets to be exported, based on the users selection
        'This is the List of Row-Names, update if row names have changed!
        Dim dataSelList As New List(Of String)
        If Not dataselall = True Then
            If CheckBox3.Checked = True Then
                dataSelList.Add("Latitude")
                dataSelList.Add("Longitude")
            End If
            If CheckBox4.Checked = True Then dataSelList.Add("NAME")
            If CheckBox5.Checked = True Then dataSelList.Add("URL")
            If CheckBox6.Checked = True Then dataSelList.Add("PhotoID")
            If CheckBox7.Checked = True Then dataSelList.Add("Owner")
            If CheckBox8.Checked = True Then dataSelList.Add("UserID")
            If CheckBox9.Checked = True Then dataSelList.Add("DateTaken")
            If CheckBox10.Checked = True Then dataSelList.Add("UploadDate")
            If CheckBox11.Checked = True Then dataSelList.Add("Views")
            If CheckBox12.Checked = True Then dataSelList.Add("Tags")
            If CheckBox13.Checked = True Then dataSelList.Add("MTags")
        End If

        'userExport preparations
        UserLocationGeocodeDict.Clear()
        Dim localusercount As Long = 0
        If userOriginExport = True Or CheckBox31.Checked = True Or maptouristslocals = True Or mapPhotosFromLocals = True Then
            HelperFunctions.LoadUserLocationGeocodeIndex()
        End If

        'Check for Advanced Filter Criteria
        If Not CheckBox26.Checked = True And Not TextBox10.Text = String.Empty Then
            DataFiltering = True
            searchFullWords = CheckBox24.Checked

            filtertext1 = TextBox10.Text.ToLower
            SearchTags1 = CheckBox17.Checked
            SearchTitle1 = CheckBox18.Checked
            SearchMtags1 = CheckBox19.Checked
            If Not TextBox11.Text = String.Empty Then
                filtertext2 = TextBox11.Text.ToLower
                SearchTags2 = CheckBox20.Checked
                SearchTitle2 = CheckBox21.Checked
                SearchMtags2 = CheckBox22.Checked
                If RadioButton2.Checked = True Then
                    Operator1 = 1 'And
                ElseIf RadioButton5.Checked = True Then
                    Operator1 = 2 'or
                Else
                    Operator1 = 3 'not
                End If
                If Not TextBox12.Text = String.Empty Then
                    filtertext3 = TextBox12.Text.ToLower
                    SearchTags3 = CheckBox23.Checked
                    SearchTitle3 = CheckBox24.Checked
                    SearchMtags3 = CheckBox25.Checked
                    If RadioButton9.Checked = True Then
                        Operator1 = 1 'And
                    ElseIf RadioButton7.Checked = True Then
                        Operator1 = 2 'or
                    Else
                        Operator1 = 3 'not
                    End If

                End If
            End If
        End If

        hash.Clear()
        hashUser.Clear()
        hashTags.Clear()
        Label5.Text = ""
        Label6.Text = ""
        Label13.Visible = False
        Label14.Visible = False

        rectbottomleft.Lat = Val(TextBox6.Text)
        rectbottomleft.Lng = Val(TextBox3.Text)
        recttopright.Lat = Val(TextBox5.Text)
        recttopright.Lng = Val(TextBox2.Text)

        'Prepare Shapefile-Search (optional)
        Dim ShapefileSearch As Boolean = False
        Dim ShapefilePoly As Shapefile = Nothing
        If Not TextBox4.Text = "" Then
            Try
                ShapefilePoly = Shapefile.OpenFile(TextBox4.Text)
            Catch e As System.Exception
                MsgBox(e.Message)
                Exit Sub
            End Try
            ShapefileSearch = True
        End If

        'Prepare Multiple iterations for multiple features in shapefile (optional)
        ' 1 feature: For shapenumber As Integer = 0 To 0 --> (no repetition)
        Dim repeat As Integer = Shapecount - 1
        If Shapecount > 1 Then Label44.Visible = True

        'Run at least once, multiple times for multiple shapes in feature
        For shapenumber As Integer = 0 To repeat
            Dim ProgressText As String = ""
            If repeat > 0 Then
                Label44.Text = "Shape " & shapenumber + 1 & " of " & Shapecount
            End If

            Me.Refresh()
            If ShapefileSearch = True Then
                polyY = Nothing
                polyX = Nothing
                constant = Nothing
                multiple = Nothing
                Dim x As Long = 0
                'add coordinates of current shape to List for precalculation of raycasting/dotspatial.intersect
                For Each pointLatLng As PointLatLng In featuresShape(shapenumber)
                    ReDim Preserve polyY(x)
                    ReDim Preserve polyX(x)
                    polyY(x) = pointLatLng.Lat
                    polyX(x) = pointLatLng.Lng
                    x = x + 1
                Next

                'ReDim PolyCorners Constant for size of Polycorners
                ReDim constant(x)
                ReDim multiple(x)
                polyCorners = x

                'Pre-Calculate Polygon Selection Values for raycasting
                If CheckBox14.Checked = True Then
                    precalc_values()
                End If
            End If

            'Reset variables after each feature in shapefile is processed
            Dim i As Integer = 0
            Dim filename_data_sel As New List(Of SourceData)
            Dim filename_data_sel_CompletelyWithin As New List(Of SourceData)
            Dim count_filelist_sel As Integer = 0
            Dim SpatialSkip As Boolean = False 'is true when dataset completely within selection, no point-based query necessary

            'First check if datasets are concerned by user selected area
            '''''Dataset selection start''''''
            For Each DataSet As SourceData In filename_data
                Dim fs_data As New FeatureSet(FeatureType.Polygon)
                ' create a geometry from data_set extent
                Dim vertices As New List(Of Coordinate)()
                vertices.Add(New Coordinate(DataSet.leftlong, DataSet.bottomlat))
                vertices.Add(New Coordinate(DataSet.leftlong, DataSet.toplat))
                vertices.Add(New Coordinate(DataSet.rightlong, DataSet.toplat))
                vertices.Add(New Coordinate(DataSet.rightlong, DataSet.bottomlat))
                Dim geom As New Polygon(vertices)

                ' add the dataset-geometry to the featureset. 
                Dim data_feature As IFeature = fs_data.AddFeature(geom)

                If NoClip = True Then

                    filename_data_sel.Add(DataSet)
                    count_filelist_sel = count_filelist_sel + DataSet.datafiles.Count

                ElseIf ShapefileSearch = False Or mapPhotosFromLocals = True Then
                    Dim fs_SelBox As New FeatureSet(FeatureType.Polygon)
                    ' create a geometry from data_set extent
                    Dim vertices_Selbox As New List(Of Coordinate)()
                    vertices_Selbox.Add(New Coordinate(rectbottomleft.Lng, rectbottomleft.Lat))
                    vertices_Selbox.Add(New Coordinate(rectbottomleft.Lng, recttopright.Lat))
                    vertices_Selbox.Add(New Coordinate(recttopright.Lng, recttopright.Lat))
                    vertices_Selbox.Add(New Coordinate(recttopright.Lng, rectbottomleft.Lat))
                    Dim geom_Selbox As New Polygon(vertices_Selbox)

                    ' add the dataset-geometry to the featureset. 
                    Dim SelBox_feature As IFeature = fs_SelBox.AddFeature(geom_Selbox)

                    'Check intersection
                    If SelBox_feature.Intersects(data_feature) Then
                        filename_data_sel.Add(DataSet)
                        count_filelist_sel = count_filelist_sel + DataSet.datafiles.Count
                        If SelBox_feature.Contains(data_feature) Then
                            filename_data_sel_CompletelyWithin.Add(DataSet) 'Based on this list, datasets can be added to the output without detailed point-in-polygon test
                        End If
                    End If

                Else 'if Shapefilesearch (and no maplocaluser-search)
                    Dim f As Feature = ShapefilePoly.Features(shapenumber)
                    If f IsNot Nothing Then
                        If f.Intersects(data_feature) Then
                            filename_data_sel.Add(DataSet)
                            count_filelist_sel = count_filelist_sel + DataSet.datafiles.Count
                            If f.Contains(data_feature) Then
                                filename_data_sel_CompletelyWithin.Add(DataSet) 'Based on this list, datasets can be added to the output without detailed point-in-polygon test
                            End If
                        End If
                    End If
                End If
            Next
            '''''Dataset selection end''''''

            If filename_data_sel.Count = 0 Then
                MsgBox("No data exists for this boundary.")
                Exit Sub
            End If

            ProgressBar1.Value = 0
            ProgressBar1.Minimum = 0
            ProgressBar1.Maximum = count_filelist_sel

            countnewfiles = countnewfiles + 1
            If retainfolderstructure = False Then
                newfilenamepath = outputdir & outputname
                If export = True Or userOriginExport = True Then
                    If Not (Directory.Exists(outputdir)) Then
                        Directory.CreateDirectory(outputdir)
                    End If
                    If export = True Then outputfile = System.IO.File.CreateText(newfilenamepath & "_" & countnewfiles & ".txt")
                End If
            End If

            Label6.Visible = True

            'Now check each dataset for photos lying within the selection boundary + temporal constraints
            For Each dataSource As SourceData In filename_data_sel
                If retainfolderstructure = True AndAlso export = True Then
                    Dim split As String() = dataSource.datafiles(0).Split("\")
                    Dim parentFolder As String = split(split.Length - 2)
                    outputdir = AppPath & "Output\03_ClippedData\" & outputname & "\" & parentFolder & "\"
                    newfilenamepath = outputdir & parentFolder
                    If Not (Directory.Exists(outputdir)) Then
                        Directory.CreateDirectory(outputdir)
                    End If
                    If File.Exists(Path.GetDirectoryName(dataSource.datafiles(0)) & "\settings.txt") Then
                        File.Copy(Path.GetDirectoryName(dataSource.datafiles(0)) & "\settings.txt", outputdir & "\settings.txt")
                    End If
                    outputfile = System.IO.File.CreateText(newfilenamepath & "_" & countnewfiles & ".txt")
                End If

                SpatialSkip = False
                If NoClip OrElse filename_data_sel_CompletelyWithin.Contains(dataSource) OrElse mapPhotosFromLocals = True Then SpatialSkip = True
                For Each filename As String In dataSource.datafiles
                    Using fp As New FastPix(visMap)
                        Dim line As Long = 0
                        filenamepath = filename
                        filename = filename.Substring(filename.LastIndexOf("\"c) + 1)
                        If File.Exists(filenamepath) Then
                            i = i + 1
                            Label5.Text = "Processing file " & filename & " (" & i & " of " & count_filelist_sel.ToString & ")."
                            Me.Refresh()
                            'Start Reading File
                            Dim objReader As New System.IO.StreamReader(filenamepath)
                            Dim result As String() = Nothing
                            Dim resultNum As Integer
                            Dim resultLat, resultLng As Double
                            Dim linetext As String
                            Dim linetextArr As String()
                            Dim dateColumn As Integer = 0
                            Dim headerline_arr As String()
                            Dim headerline_arr_sel As New List(Of String)
                            Dim c As Integer = 0
                            linetext = objReader.ReadLine()
                            line = line + 1

                            'Define Headerline
                            header_line = linetext
                            headerline_arr = header_line.Split(",")

                            For Each d As String In headerline_arr
                                If dataSelList.Contains(d) Then headerline_arr_sel.Add(d) 'Add Items to Export-Selection if user has selected them
                                If RadioButton1.Checked = False Then
                                    If d = "DateTaken" Then
                                        If RadioButton3.Checked = True Then dateColumn = c
                                    End If
                                    If d = "UploadDate" Then
                                        If RadioButton3.Checked = False Then dateColumn = c
                                    End If
                                End If
                                c = c + 1
                            Next

                            If dateColumn = 0 AndAlso RadioButton1.Checked = False Then
                                MsgBox("Could not find Date Column in Input Data.")
                                Exit Sub
                            End If

                            'Re-Define Headerline if only specific data is to be exported
                            If dataselall = False Then
                                header_line = "ID"
                                For Each DataRow As String In dataSelList 'for each row-name in list of selected-for-export datarows
                                    header_line = header_line & "," & DataRow
                                Next
                            End If

                            Do While objReader.Peek() <> -1
                                line = line + 1
                                linetext = objReader.ReadLine()
                                linetextArr = linetext.Split(",")
                                If linetextArr.Length >= 11 Then
                                    resultNum = Val(linetextArr(0)) 'ID
                                    resultLat = Val(linetextArr(1)) 'Lat
                                    resultLng = Val(linetextArr(2)) 'Long

                                    PhotoIDc = Val(linetextArr(5)) 'PhotoID
                                    UserIDc = linetextArr(7) 'UserID (String!)
                                    If photocollection Then
                                        Views = Val(linetextArr(10)) 'Views
                                        PhotoURL = linetextArr(4) 'URL
                                    End If
                                Else : GoTo skip_line 'Skip erroneous line with less entries than expected
                                End If

                                'Check Local Photos (optional)
                                If mapPhotosFromLocals = True Then
                                    If hashUser.Contains(UserIDc) OrElse HelperFunctions.UserLocationGeocodeDict.ContainsKey(UserIDc) Then
                                        Dim ltlngPair As KeyValuePair(Of Double, Double) = HelperFunctions.UserLocationGeocodeDict(UserIDc)
                                        If Not LiesWithin(ltlngPair.Key, ltlngPair.Value, rectbottomleft, recttopright, ShapefileSearch, ShapefilePoly) = True Then
                                            GoTo skip_line 'Skip all photos not from locals
                                        End If
                                    Else
                                        GoTo skip_line 'Skip all photos from users with no info
                                    End If
                                End If

                                'Read DateValue from Line if DateLimit specified
                                If Not dateColumn = 0 Then PDate = DateTime.Parse(linetext.Split(",")(dateColumn))

                                If Not resultLat = 0 AndAlso Not resultLng = 0 AndAlso hash.Contains(PhotoIDc) = False AndAlso (SpatialSkip OrElse LiesWithin(resultLat, resultLng, rectbottomleft, recttopright, ShapefileSearch, ShapefilePoly)) Then
                                    If dateColumn = 0 OrElse LiesWithinDateRange(PDate, minDate, maxDate) Then
                                        If DataFiltering = False OrElse data_contains(filtertext1, filtertext2, filtertext3, linetextArr) = True Then 'linetextArr(11).Contains(filtertext1) Then
                                            countlines = countlines + 1

                                            'Statistics
                                            hash.Add(PhotoIDc) 'Hash-List for Duplicate Detection
                                            If hash.Count > 100000 Then 'Clear List if Hash gets too large (Duplicate Detection above 500,000 makes no sense anyway..)
                                                hash.Clear()
                                            End If
                                            If Not NoStatistics = True Then
                                                'Measure Photo Views
                                                'If Views > MostViews Then
                                                '    PhotosListTop10View.Add(New KeyValuePair(Of String, Integer)(PhotoIDc, Views))
                                                '    If PhotosListTop10View.Count > 10 Then
                                                '        MostViews = Views
                                                '        PhotosListTop10View.RemoveAt(PhotosListTop10View.Count - 1)
                                                '    End If
                                                'End If

                                                hashUser.Add(UserIDc) 'Count Unique Users
                                                Tagsc = Tagsc + CountCharacter(linetextArr(11), ";"c) - 2
                                                If estimateUnique = False AndAlso hashTags.Count > 10000000 Then 'Max size of Hashset = 2 Billion, but Strings consume more. Test Exception reached at 11,998,949
                                                    estimateUnique = True
                                                    estPhotosBase = countlines_sich + countlines
                                                    estHashtagBase = hashTags.Count
                                                    hashTags.Clear()
                                                Else
                                                    Try
                                                        hashTags.UnionWith(linetextArr(11).Split(";")) 'Count Unique Tags
                                                    Catch ex As OutOfMemoryException
                                                        If Not IsNothing(ex.Message) Then
                                                            MsgBox("System.OutOfMemoryException at HashTagsCount: " & hashTags.Count & ", freeing up memory..")
                                                            estimateUnique = True
                                                            estPhotosBase = countlines_sich + countlines
                                                            estHashtagBase = hashTags.Count
                                                            hashTags.Clear()
                                                        Else
                                                            Exit Sub
                                                        End If
                                                    End Try
                                                End If
                                            End If
                                            'Statistics End

                                            'Plot Point on map
                                            If CheckBox15.Checked Then 'Exclude erroneous data on draw (?)
                                                Dim localtourist As Integer = Nothing
                                                'Colormapping of locals
                                                If maptouristslocals = True Then
                                                    localtourist = 0
                                                    If HelperFunctions.UserLocationGeocodeDict.ContainsKey(UserIDc) Then
                                                        Dim ltlngPair As KeyValuePair(Of Double, Double) = HelperFunctions.UserLocationGeocodeDict(UserIDc)
                                                        If LiesWithin(ltlngPair.Key, ltlngPair.Value, rectbottomleft, recttopright, ShapefileSearch, ShapefilePoly) Then
                                                            localtourist = 1
                                                        Else
                                                            localtourist = 2
                                                        End If
                                                    End If
                                                End If
                                                If photocollection Then
                                                    visualForm.mapcoords(resultLat, resultLng, fp, localtourist, Views, PhotoIDc, PhotoURL)
                                                Else
                                                    visualForm.mapcoords(resultLat, resultLng, fp, localtourist)
                                                End If
                                            End If
                                            If export = True Then
                                                If header_line_written = False Then
                                                    outputfile.WriteLine(header_line)
                                                    header_line_written = True
                                                End If
                                                If dataselall = True Then 'If all data is to be exported
                                                    linetext = countlines & linetext.Substring(linetext.IndexOf(",")) 'Starts writing after first comma (ignores original ID's and appends new countlines)
                                                Else 'if only some data is to be exported
                                                    linetext = countlines
                                                    Dim ii As Integer = 0
                                                    For Each d As String In headerline_arr
                                                        If headerline_arr_sel.Contains(d) Then linetext = linetext & "," & linetextArr(ii)
                                                        ii = ii + 1
                                                    Next
                                                End If
                                                outputfile.WriteLine(linetext)
                                            End If
                                            If (countlines_sich + countlines) Mod (1 + ProgressBar1.Value) * 1000 = 0 Then 'Update Progress 
                                                Label6.Text = "Photos found: " & Math.Round(countlines_sich + countlines, 0).ToString("N0")
                                                Me.Refresh()
                                            End If
                                            If countlines >= 50000 Then
                                                countlines_sich = countlines_sich + countlines
                                                countlines = 0
                                                countnewfiles = countnewfiles + 1
                                                If export = True Then
                                                    outputfile.Flush()
                                                    outputfile.Close()
                                                    If retainfolderstructure = True Then
                                                        FileSystem.Rename(newfilenamepath & "_" & countnewfiles - 1 & ".txt", newfilenamepath & "_" & countnewfiles - 1 & "_" & 50000 & "_Part.txt")
                                                    End If
                                                    outputfile = System.IO.File.CreateText(newfilenamepath & "_" & countnewfiles & ".txt")
                                                    header_line_written = False
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
skip_line:                  Loop
                            objReader.Close()
                        End If
                        ProgressBar1.Value = ProgressBar1.Value + 1
                    End Using
                    If CheckBox15.Checked Then
                        'Update VisMap for each Dataset
                        visualForm.PictureBox1.Image = visMap
                        visualForm.Refresh()
                    End If
                Next
                'Delete Outputdirectory if Empty
                If retainfolderstructure = True And export = True Then
                    outputfile.Flush()
                    outputfile.Close()
                    'Delete Empty Output File
                    If countnewfiles = 1 AndAlso countlines = 0 Then
                        File.Delete(newfilenamepath & "_" & countnewfiles & ".txt")
                        Directory.Delete(outputdir, True)
                    Else
                        If retainfolderstructure = True Then
                            FileSystem.Rename(newfilenamepath & "_" & countnewfiles & ".txt", newfilenamepath & "_" & countnewfiles & "_" & countlines & "_Part.txt")
                        End If
                    End If
                    countlines_sich = countlines_sich + countlines
                    countlines = 0
                    countnewfiles = 1
                    header_line_written = False
                End If
            Next


            Dim uTagsCount As Long = 0
            If estimateUnique = True Then
                Dim photoTotalCount As Long = countlines_sich + countlines
                uTagsCount = Math.Round(((estHashtagBase / estPhotosBase) * photoTotalCount) / 5000, 0) * 5000 'Round to rough 5000s if estimation is true
            Else
                uTagsCount = hashTags.Count
            End If

            If CheckBox15.Checked Then
                Dim statText As String = Math.Round(countlines_sich + countlines, 0).ToString("N0") & vbCrLf & hashUser.Count.ToString("N0") & vbCrLf & Tagsc.ToString("N0") & vbCrLf & uTagsCount.ToString("N0")
                Dim statImage As New Bitmap(visualForm.PictureBox1.Width, visualForm.PictureBox1.Height)
                Dim grap2 As Drawing.Graphics = Drawing.Graphics.FromImage(statImage)
                grap2.Clear(Drawing.Color.Pink)
                statImage.MakeTransparent(Color.Pink)
                If visualForm.Button6.Text = "Stats On" Then
                    visualForm.PictureBox6.Visible = True
                End If
                writeTextOnBitmap(statImage, statText)
                visualForm.PictureBox6.Image = statImage
                visualForm.PictureBox1.Image = visMap
                visualForm.Refresh()
                visualForm.bmOrig = visMap
            End If
            If export = True AndAlso retainfolderstructure = False Then
                outputfile.Flush()
                outputfile.Close()
                'if no photos  written
                If countlines = 0 Then
                    System.IO.File.Delete(newfilenamepath & "_" & countnewfiles & ".txt")
                    If countnewfiles = 1 Then
                        Directory.Delete(outputdir)
                    End If
                End If
            End If
            Label6.Text = "Photos found: " & Math.Round(countlines_sich + countlines, 0).ToString("N0")
            Label13.Visible = True
            Label14.Visible = True
            Label14.Text = "Users: " & hashUser.Count.ToString("N0") & " | Total # of Tags: " & Tagsc.ToString("N0") & " | Distinct # of Tags: " & uTagsCount.ToString("N0")
            Me.Refresh()

            'Begin Analyze and Export UserOrigin
            Label5.Text = ""
            Label40.Text = ""

            If CheckBox31.Checked = True OrElse userOriginExport = True Then
                Label5.Text = "Analyzing User Locations.. "
                For Each ID As String In hashUser
                    If HelperFunctions.UserLocationGeocodeDict.ContainsKey(ID) Then
                        UserLocationGeocodeDict(ID) = HelperFunctions.UserLocationGeocodeDict(ID)
                        Dim ltlngPair As KeyValuePair(Of Double, Double) = HelperFunctions.UserLocationGeocodeDict(ID)
                        'key = lat
                        'value = lng
                        If LiesWithin(ltlngPair.Key, ltlngPair.Value, rectbottomleft, recttopright, ShapefileSearch, ShapefilePoly) Then
                            localusercount = localusercount + 1
                        End If
                    End If
                Next

                Label5.Text &= UserLocationGeocodeDict.Count & " matching IDs found."
                If userOriginExport = True Then
                    Dim userfilenamepath As String = outputdir & "UserOrigin_LatLng.txt"
                    Dim objWriter As New System.IO.StreamWriter(userfilenamepath) 'True for Append Entries
                    objWriter.WriteLine("Lat, Lng, UserID")
                    For Each entry As KeyValuePair(Of String, KeyValuePair(Of Double, Double)) In UserLocationGeocodeDict
                        Dim userid As String = Replace(Replace(entry.Key, ControlChars.Quote, ""), ",", ";").Trim
                        If Not userid = "" Then
                            objWriter.WriteLine(entry.Value.Key & "," & entry.Value.Value & "," & userid)
                        End If
                    Next
                    objWriter.Close()
                End If
            End If
            Label40.Visible = True
            Label40.Text = "Local Users: " & localusercount.ToString("N0") & " (" & Math.Round(localusercount / (UserLocationGeocodeDict.Count / 100), 0) & "%) " & "| Visiting: " & (UserLocationGeocodeDict.Count - localusercount).ToString("N0") & " (" & Math.Round((UserLocationGeocodeDict.Count - localusercount) / (UserLocationGeocodeDict.Count / 100), 0) & "%) "

        Next 'next Feature

        Label5.Text = "All done."
        Label44.Visible = False

    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        searchexport(True)
    End Sub

    Sub writeTextOnBitmap(ByRef visMap As Bitmap, ByVal myText As String)
        Dim g As Graphics = Graphics.FromImage(visMap)
        Dim myFont As System.Drawing.Font = New Drawing.Font("Arial", 9, FontStyle.Regular)
        Dim Rect As System.Drawing.Rectangle = New Rectangle(New System.Drawing.Point(visMap.Width - 220, visMap.Height - 80), New Size(200, 60))
        Dim RectBG As System.Drawing.Rectangle = New Rectangle(New System.Drawing.Point(visMap.Width - 220, visMap.Height - 81), New Size(200, 61))
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.FillRectangle(New SolidBrush(Color.FromArgb(180, 255, 255, 255)), RectBG)
        g.DrawRectangle(Pens.LightGray, RectBG)
        g.DrawString("Photos: " & vbCrLf & "Users: " & vbCrLf & "Total # of Tags: " & vbCrLf & "Distinct # of Tags: ", myFont, New SolidBrush(Color.FromArgb(255, 10, 10, 10)), New System.Drawing.Point(visMap.Width - 220, visMap.Height - 80))
        g.DrawString(myText, myFont, New SolidBrush(Color.FromArgb(255, 10, 10, 10)), New System.Drawing.Point(visMap.Width - 118, visMap.Height - 80))
    End Sub

    Function LiesWithin(latP As Double, lngP As Double, rectBottomleft As PointLatLng, rectTopright As PointLatLng, shapefilesearch As Boolean, ShapefilePoly As Shapefile) As Boolean
        LiesWithin = False
        'If Rectangle Selection Search
        If shapefilesearch = False Then
            If latP > rectBottomleft.Lat Then
                If latP < rectTopright.Lat Then
                    If lngP > rectBottomleft.Lng Then
                        If lngP < rectTopright.Lng Then
                            LiesWithin = True
                            Exit Function
                        End If
                    End If
                End If
            End If
        Else 'If Shapefile Selection Search
            'Basic PreCheck points
            If latP > b_yMax OrElse latP < b_yMin OrElse lngP > b_xMax OrElse lngP < b_xMin Then
                'If coordinate lies outside poly-max extent bound, exit function
                Exit Function
            End If

            'If raycasting disabled
            If CheckBox14.Checked = False Then
                'Dotspatial.contains (works, but slower):
                Dim PhotoPoint As New DotSpatial.Topology.Coordinate(lngP, latP)
                For Each f As DotSpatial.Data.Feature In ShapefilePoly.Features
                    Dim pg As DotSpatial.Topology.Polygon = TryCast(f.BasicGeometry, Polygon)
                    If pg IsNot Nothing Then
                        If pg.Intersects(New DotSpatial.Topology.Point(PhotoPoint)) Then
                            LiesWithin = True
                            Exit Function
                        End If
                    Else
                        ' If you have a multi-part polygon then this should also handle holes I think
                        Dim polygons As MultiPolygon = TryCast(f.BasicGeometry, MultiPolygon)
                        If polygons.Intersects(New DotSpatial.Topology.Point(PhotoPoint)) Then
                            LiesWithin = True
                            Exit Function
                        End If
                    End If
                Next
            Else
                'Raycasting Algorithm
                xP = lngP
                yP = latP

                If pointInPolygon() = True Then LiesWithin = True
            End If
        End If
    End Function

    'Point-In-Polygon Algorithm http://alienryderflex.com/polygon/
    '  Globals which should be set before calling these functions:
    '
    '  int    polyCorners  =  how many corners the polygon has (no repeats)
    '  float  polyX[]      =  horizontal coordinates of corners
    '  float  polyY[]      =  vertical coordinates of corners
    '  float  x, y         =  point to be tested
    '
    '  The following global arrays should be allocated before calling these functions:
    '
    '  float  constant[] = storage for precalculated constants (same size as polyX)
    '  float  multiple[] = storage for precalculated multipliers (same size as polyX)
    '
    '  (Globals are used in this example for purposes of speed.  Change as
    '  desired.)
    '
    '  USAGE:
    '  Call precalc_values() to initialize the constant[] and multiple[] arrays,
    '  then call pointInPolygon(x, y) to determine if the point is in the polygon.
    '
    '  The function will return YES if the point x,y is inside the polygon, or
    '  NO if it is not.  If the point is exactly on the edge of the polygon,
    '  then the function may return YES or NO.
    '
    '  Note that division by zero is avoided because the division is protected
    '  by the "if" clause which surrounds it.

    Private Sub precalc_values()
        Dim i As Integer, j As Integer = polyCorners - 1
        For i = 0 To polyCorners - 1
            If polyY(j) = polyY(i) Then
                constant(i) = polyX(i)
                multiple(i) = 0
            Else
                constant(i) = polyX(i) - (polyY(i) * polyX(j)) / (polyY(j) - polyY(i)) + (polyY(i) * polyX(i)) / (polyY(j) - polyY(i))
                multiple(i) = (polyX(j) - polyX(i)) / (polyY(j) - polyY(i))
            End If
            j = i
        Next
    End Sub

    'Point-In-Polygon Algorithm http://alienryderflex.com/polygon/
    Private Function pointInPolygon() As Boolean

        Dim i As Integer, j As Integer = polyCorners - 1
        Dim oddNodes As Boolean = False

        For i = 0 To polyCorners - 1
            If (polyY(i) < yP AndAlso polyY(j) >= yP OrElse polyY(j) < yP AndAlso polyY(i) >= yP) Then
                oddNodes = oddNodes Xor (yP * multiple(i) + constant(i) < xP)
            End If
            j = i
        Next

        Return oddNodes
    End Function

    Function LiesWithinDateRange(PDate As System.DateTime, MinDate As System.DateTime, MaxDate As System.DateTime) As Boolean
        LiesWithinDateRange = False
        If DateTime.Compare(PDate, MinDate) >= 0 Then
            If DateTime.Compare(PDate, MaxDate) <= 0 Then
                LiesWithinDateRange = True
                Exit Function
            End If
        End If
    End Function

    Public Function CountCharacter(ByVal value As String, ByVal ch As Char) As Integer
        Dim cnt As Integer = 0
        For Each c As Char In value
            If c = ch Then cnt += 1
        Next
        Return cnt
    End Function

    Function data_contains(ByVal string1 As String, ByVal string2 As String, ByVal string3 As String, ByVal linetextArr As String()) As Boolean
        data_contains = False
        If searchFullWords = False Then 'Add wildcards for non-full-word searches
            string1 = "*" & string1 & "*"
            string2 = "*" & string2 & "*"
            string3 = "*" & string3 & "*"
        End If
        linetextArr(11) = linetextArr(11).ToLower
        linetextArr(12) = linetextArr(12).ToLower
        linetextArr(3) = linetextArr(3).ToLower
        'Operator1 = 1 'And
        'Operator1 = 2 'or
        'Operator1 = 3 'not

        'String1
        If SearchTags1 AndAlso linetextArr(11) Like "*;" & string1 & ";*" Then
            data_contains = True
            GoTo Search2
        End If
        If SearchMtags1 AndAlso linetextArr(12) Like "*;" & string1 & ";*" Then
            data_contains = True
            GoTo Search2
        End If
        If SearchTitle1 AndAlso linetextArr(3) Like "* " & string1 & " *" Then
            data_contains = True
            GoTo Search2
        End If
Search2:  'String2
        If string2 = String.Empty OrElse (Operator1 = 2 AndAlso data_contains = True) OrElse (Operator1 = 1 AndAlso data_contains = False) Then GoTo Search3
        If Operator1 < 3 Then 'and
            If SearchTags2 AndAlso linetextArr(11) Like "*;" & string2 & ";*" Then
                data_contains = True
                GoTo Search3
            Else
                If Operator1 = 1 Then
                    data_contains = False
                    Exit Function
                End If
            End If
            If SearchMtags2 AndAlso linetextArr(12) Like "*;" & string2 & ";*" Then
                data_contains = True
                GoTo Search3
            Else
                If Operator1 = 1 Then
                    data_contains = False
                    Exit Function
                End If
            End If
            If SearchTitle2 AndAlso linetextArr(3) Like "* " & string2 & " *" Then
                data_contains = True
                GoTo Search3
            Else
                If Operator1 = 1 Then
                    data_contains = False
                    Exit Function
                End If
            End If
        Else 'Operator1 = 3
            If SearchTags2 AndAlso linetextArr(11) Like "*;" & string2 & ";*" Then
                data_contains = False
                Exit Function
            End If
            If SearchMtags2 AndAlso linetextArr(12) Like "*;" & string2 & ";*" Then
                data_contains = False
                Exit Function
            End If
            If SearchTitle2 AndAlso linetextArr(3) Like "* " & string2 & " *" Then
                data_contains = False
                Exit Function
            End If
        End If
Search3:  'String3
        If string3 = String.Empty OrElse (Operator2 = 2 AndAlso data_contains = True) OrElse (Operator2 = 1 AndAlso data_contains = False) Then Exit Function
        If Operator2 < 3 Then 'and
            If SearchTags3 AndAlso linetextArr(11) Like "*;" & string3 & ";*" Then
                data_contains = True
                Exit Function
            Else
                If Operator2 = 1 Then
                    data_contains = False
                    Exit Function
                End If
            End If
            If SearchMtags3 AndAlso linetextArr(12) Like "*;" & string3 & ";*" Then
                data_contains = True
                Exit Function
            Else
                If Operator2 = 1 Then
                    data_contains = False
                    Exit Function
                End If
            End If
            If SearchTitle3 AndAlso linetextArr(3) Like "* " & string3 & " *" Then
                data_contains = True
                Exit Function
            Else
                If Operator2 = 1 Then
                    data_contains = False
                    Exit Function
                End If
            End If
        Else 'Operator1 = 3
            If SearchTags3 AndAlso linetextArr(11) Like "*;" & string3 & ";*" Then
                data_contains = False
                Exit Function
            End If
            If SearchMtags3 AndAlso linetextArr(12) Like "*;" & string3 & ";*" Then
                data_contains = False
                Exit Function
            End If
            If SearchTitle3 AndAlso linetextArr(3) Like "* " & string3 & " *" Then
                data_contains = False
                Exit Function
            End If
        End If
    End Function

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        If Me.RadioButton3.Checked = True Then
            Me.min_date.Enabled = True
            Me.max_date.Enabled = True
            Label15.ForeColor = SystemColors.ControlText
            Label24.ForeColor = SystemColors.ControlText
            RadioButton3.ForeColor = SystemColors.ControlText
            RadioButton4.Checked = False
            RadioButton1.Checked = False
        Else
            Me.min_date.Enabled = False
            Me.max_date.Enabled = False
            Label15.ForeColor = SystemColors.InactiveCaptionText
            Label24.ForeColor = SystemColors.InactiveCaptionText
            RadioButton3.ForeColor = SystemColors.InactiveCaptionText
        End If
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs)
        If Me.RadioButton1.Checked = True Then
            Me.min_date.Enabled = False
            Me.max_date.Enabled = False
            Label15.ForeColor = SystemColors.InactiveCaptionText
            Label24.ForeColor = SystemColors.InactiveCaptionText
            RadioButton1.ForeColor = SystemColors.ControlText
            RadioButton3.ForeColor = SystemColors.InactiveCaptionText
            RadioButton4.ForeColor = SystemColors.InactiveCaptionText
            RadioButton4.Checked = False
            RadioButton3.Checked = False
        Else
            RadioButton1.ForeColor = SystemColors.InactiveCaptionText
        End If
    End Sub

    Private Sub RadioButton4_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton4.CheckedChanged, RadioButton1.CheckedChanged
        If Me.RadioButton4.Checked = True Then
            Me.min_date.Enabled = True
            Me.max_date.Enabled = True
            Label15.ForeColor = SystemColors.ControlText
            Label24.ForeColor = SystemColors.ControlText
            RadioButton4.ForeColor = SystemColors.ControlText
            RadioButton1.Checked = False
            RadioButton3.Checked = False
        Else
            Me.min_date.Enabled = False
            Me.max_date.Enabled = False
            Label15.ForeColor = SystemColors.InactiveCaptionText
            Label24.ForeColor = SystemColors.InactiveCaptionText
            RadioButton4.ForeColor = SystemColors.InactiveCaptionText
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        searchexport(False)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim strFileName As String = ""
        Dim folderDialogBox As New FolderBrowserDialog()

        If folderDialogBox.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            strPath = folderDialogBox.SelectedPath
            If strPath.EndsWith("\") Then
                strPath = strPath.Substring(strPath.Length - 1)
            End If
            Me.TextBox1.Text = strPath
        Else
            Exit Sub
        End If

    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            For Each x As GMapMarker In GMapControl1.Overlays(3).Markers
                x.ToolTipMode = MarkerTooltipMode.Always
            Next
        Else
            For Each x As GMapMarker In GMapControl1.Overlays(3).Markers
                x.ToolTipMode = MarkerTooltipMode.Never
            Next
        End If
        GMapControl1.Refresh()
    End Sub

    Private Sub GMapControl1_OnMapZoomChanged() Handles GMapControl1.OnMapZoomChanged
        If maploaded = True Then
            If MarkerVisible = True AndAlso GMapControl1.Zoom <= 6 Then
                For Each x As GMapMarker In GMapControl1.Overlays(3).Markers
                    x.IsVisible = False
                Next
                MarkerVisible = False
                GMapControl1.Refresh()

            ElseIf MarkerVisible = False AndAlso GMapControl1.Zoom > 6 Then
                For Each x As GMapMarker In GMapControl1.Overlays(3).Markers
                    x.IsVisible = True
                Next
                MarkerVisible = True
                GMapControl1.Refresh()
            End If
        End If
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged, CheckBox14.CheckedChanged
        If CheckBox2.Checked = True Then
            TextBox2.Enabled = False
            TextBox3.Enabled = False
            TextBox4.Enabled = False
            TextBox5.Enabled = False
            TextBox6.Enabled = False
            TextBox7.Enabled = False
            TextBox8.Enabled = False
            TextBox13.Enabled = False
            TextBox14.Enabled = False
            Button6.Enabled = False
            Button8.Enabled = False
            Label37.ForeColor = SystemColors.InactiveCaptionText
            Label27.ForeColor = SystemColors.InactiveCaptionText
            Label28.ForeColor = SystemColors.InactiveCaptionText
            Label30.ForeColor = SystemColors.InactiveCaptionText
            Label35.ForeColor = SystemColors.InactiveCaptionText
            Label38.ForeColor = SystemColors.InactiveCaptionText
            Label29.ForeColor = SystemColors.InactiveCaptionText
            Label34.ForeColor = SystemColors.InactiveCaptionText
            Label27.ForeColor = SystemColors.InactiveCaptionText
            Label36.ForeColor = SystemColors.InactiveCaptionText
            CheckBox14.ForeColor = SystemColors.InactiveCaptionText
            CheckBox14.Enabled = False
            CheckBox2.ForeColor = SystemColors.ControlText
        Else
            TextBox2.Enabled = True
            TextBox3.Enabled = True
            TextBox4.Enabled = True
            TextBox5.Enabled = True
            TextBox6.Enabled = True
            TextBox7.Enabled = True
            TextBox8.Enabled = True
            TextBox13.Enabled = True
            TextBox14.Enabled = True
            Button6.Enabled = True
            Button8.Enabled = True
            Label37.ForeColor = SystemColors.ControlText
            Label27.ForeColor = SystemColors.ControlText
            Label28.ForeColor = SystemColors.ControlText
            Label30.ForeColor = SystemColors.ControlText
            Label35.ForeColor = SystemColors.ControlText
            Label38.ForeColor = SystemColors.ControlText
            Label29.ForeColor = SystemColors.ControlText
            Label34.ForeColor = SystemColors.ControlText
            Label27.ForeColor = SystemColors.ControlText
            Label36.ForeColor = SystemColors.ControlText
            CheckBox14.Enabled = True
            CheckBox2.ForeColor = SystemColors.InactiveCaptionText
        End If
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged, CheckBox8.CheckedChanged, CheckBox7.CheckedChanged, CheckBox6.CheckedChanged, CheckBox5.CheckedChanged, CheckBox4.CheckedChanged, CheckBox3.CheckedChanged, CheckBox13.CheckedChanged, CheckBox12.CheckedChanged, CheckBox11.CheckedChanged, CheckBox10.CheckedChanged
        If formfullyloaded Then
            changeSelTextStatus()
        End If
    End Sub

    'Sub changes Color of Checkboxes in panel 5
    Sub changeSelTextStatus()
        Dim ctl As Control
        Dim chk As CheckBox
        Dim count As Integer = 0
        Dim CheckboxList As New List(Of CheckBox)

        For Each ctl In TabPage3.Controls 'loops through all controls in tab
            If TypeOf ctl Is CheckBox AndAlso ctl.Name.Length > 8 AndAlso Val(ctl.Name.Substring(8)) >= 3 AndAlso Val(ctl.Name.Substring(8)) <= 13 Then
                chk = ctl
                CheckboxList.Add(chk)
                If chk.Checked Then
                    count = count + 1
                End If
            End If
        Next ctl
        If count = 11 Then
            dataselall = True
            For Each chk In CheckboxList
                chk.ForeColor = SystemColors.InactiveCaptionText
            Next chk
        Else
            dataselall = False
            For Each chk In CheckboxList
                If chk.Checked = True Then
                    chk.ForeColor = SystemColors.ControlText
                Else
                    chk.ForeColor = SystemColors.InactiveCaptionText
                End If
            Next chk
        End If

    End Sub

    Private Sub ClipDataForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        formfullyloaded = True
        TextBox1.Text = My.Settings.Sourcepath
    End Sub

    'Shapefile load
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click, Button8.Click
        Dim filedialog As OpenFileDialog = New OpenFileDialog()
        filedialog.Title = "Select shapefile (WGS1984 Projection)"
        'filedialog.InitialDirectory = AppPath & "Output"
        If My.Settings.Shapepath = "" Then
            If Directory.Exists(AppPath & "ShapeSel\") Then
                filedialog.InitialDirectory = AppPath 'DE_WGS1984.shp
            Else
                filedialog.InitialDirectory = AppPath & "ShapeSel\" 'DE_WGS1984.shp
            End If
        Else
            filedialog.InitialDirectory = My.Settings.Shapepath
        End If
        filedialog.Filter = "shp files (*.shp)|*.shp|All files (*.*)|*.*"
        filedialog.RestoreDirectory = True
        If filedialog.ShowDialog() = DialogResult.OK Then
            TextBox4.Text = filedialog.FileName
        Else
            Exit Sub
        End If
        My.Settings.Shapepath = filedialog.InitialDirectory
        Dim ShapeOverlay As New GMapOverlay(GMapControl1, "ShapeOverlay")

        'Check projection
        If File.Exists(filedialog.FileName.Substring(0, filedialog.FileName.Length - 4) & ".prj") Then
            Dim sr As StreamReader = File.OpenText(filedialog.FileName.Substring(0, filedialog.FileName.Length - 4) & ".prj")
            Dim prj As String = sr.ReadLine()
            sr.Close()
            If Not prj.Contains("WGS_1984") Then
                MsgBox("Please project your shapefile to WGS_1984.")
                TextBox4.Text = ""
                Exit Sub
            End If
        End If

        'Check for Multiple Features in Shapefile
        ShapefilePoly = FeatureSet.Open(TextBox4.Text)
        If ShapefilePoly.Features.Count > 1 Then
            Dim result As Integer = MessageBox.Show("Multiple Features in Shapefile (" & ShapefilePoly.Features.Count & "): Do you wish to extract data for each feature separately?", "Multiple Features in Shapefile", MessageBoxButtons.YesNoCancel)
            If result = DialogResult.Cancel Then
                Exit Sub
            ElseIf result = DialogResult.No Then
                Shapecount = ShapefilePoly.Features.Count
                Singleextract = False
            ElseIf result = DialogResult.Yes Then
                Shapecount = ShapefilePoly.Features.Count
                Singleextract = True
            End If
        End If
        featuresShape.Clear()

        Dim x As Integer = 0
        For Each MyShapeRange As ShapeRange In ShapefilePoly.ShapeIndices
            Dim pointsShape As New List(Of PointLatLng) 'Points (Vertices) of each Feature
            For Each MyPartRange As PartRange In MyShapeRange.Parts
                For Each MyVertex As Vertex In MyPartRange
                    pointsShape.Add(New PointLatLng(MyVertex.Y, MyVertex.X))
                    ReDim Preserve polyY(x)
                    ReDim Preserve polyX(x)
                    polyY(x) = MyVertex.Y
                    polyX(x) = MyVertex.X
                    x = x + 1
                Next
            Next
            featuresShape.Add(pointsShape)
        Next

        'Warning for complex shapes
        If x > 1000 Then
            MsgBox("Polygon geometry is very complex (" & x.ToString & " vertices). Processing of points may be very slow.")
        End If

        'ReDim PolyCorners Constant for size of Polycorners
        ReDim constant(x)
        ReDim multiple(x)
        polyCorners = x

        Label12.Text = Math.Round(x, 0).ToString("N0") & " Vertices"

        'initialize start values
        b_yMin = polyY(0)
        b_yMax = polyY(0)
        b_xMin = polyX(0)
        b_xMax = polyX(0)

        'Precalculate Bounding Box for PointInPolygonTest and zoomTo (Dotspatial.contains, not used for raycasting)
        For i As Integer = 1 To polyCorners - 1
            If polyY(i) > b_yMax Then
                b_yMax = polyY(i)
            ElseIf polyY(i) < b_yMin Then
                b_yMin = polyY(i)
            End If
            If polyX(i) > b_xMax Then
                b_xMax = polyX(i)
            ElseIf polyX(i) < b_xMin Then
                b_xMin = polyX(i)
            End If
        Next

        'Clear Selection Layer and Marker
        'Clear previous shapefile display
        If GMapControl1.Overlays.Count > 4 Then
            'Overlays(4) contains shapefile polygons
            GMapControl1.Overlays(4).Polygons.Clear()
        End If

        GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(New GMap.NET.PointLatLng(b_yMax, b_xMin), New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(b_yMax - b_yMin, b_xMax - b_xMin))))

        If visualForm.fullyloaded = True Then
            syncMaps(True)
        End If

        Dim y As Integer = 0
        For Each PointsShape As List(Of PointLatLng) In featuresShape
            y = y + 1
            Dim polygon3 As New GMapPolygon(PointsShape, "ShapeSel" & y)
            polygon3.Fill = New SolidBrush(Color.FromArgb(40, Color.Aquamarine))
            polygon3.Stroke = New Pen(Color.Red, 0.25)
            ShapeOverlay.Polygons.Add(polygon3)
        Next
        GMapControl1.Overlays.Add(ShapeOverlay)
        GMapControl1.Refresh()
        CheckBox14.ForeColor = SystemColors.ControlText
    End Sub

    Private Sub CheckBox15_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox15.CheckedChanged
        If CheckBox15.Checked = True Then
            visualForm.Show()
            Button7.Enabled = True
            With visualForm.GMapControl1
                '.SetCurrentPositionByKeywords("USA")
                .MapType = GMap.NET.MapType.ArcGIS_World_Topo_Map
                '.MapProvider = GMapProviders.ArcGIS_World_Topo_Map
                .MinZoom = 1
                .MaxZoom = 25
                '.Zoom = 10
                .Manager.Mode = GMap.NET.AccessMode.ServerAndCache
                '.Position = New GMap.NET.PointLatLng(centerlat, centerlong)
                .DragButton = MouseButtons.Left
                .CanDragMap = True
            End With
            Dim NoClip As Boolean = CheckBox2.Checked
            Dim ShapefileSearch As Boolean = False
            Dim ShapefilePoly As Shapefile = Nothing
            If Not TextBox4.Text = "" Then ShapefileSearch = True
            If ShapefileSearch = True AndAlso NoClip = True OrElse TextBox6.Text = "" Then
                visualForm.GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(New GMap.NET.PointLatLng(90, -180), New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(180, 360))))
            Else
                visualForm.GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(New GMap.NET.PointLatLng(Val(TextBox5.Text), Val(TextBox3.Text)), New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(Val(TextBox5.Text) - Val(TextBox6.Text), Val(TextBox2.Text) - Val(TextBox3.Text)))))
            End If
            visualForm.PictureBox1.Parent = visualForm.GMapControl1
            visualForm.PictureBox6.Parent = visualForm.PictureBox1
            visualForm.GMapControl1.Refresh()
            visualForm.PictureBox1.BackColor = Color.Transparent
            visualForm.TransparencyKey = Color.Pink
            visualForm.PictureBox1.BackgroundImage = Nothing
            visualForm.PictureBox1.Image = Nothing
            visualForm.ComboBox2.Enabled = True
        Else
            visualForm.Close()
            Button7.Enabled = False
        End If
    End Sub


    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If CheckBox15.Checked = True Then
            syncArea()
            Dim rectbottomleft As PointLatLng
            Dim recttopright As PointLatLng
            Dim latDist, longDist As Double
            Dim i As Integer = 0

            GMapControl1.Overlays(1).Markers.Clear()
            GMapControl1.Overlays(1).Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(New PointLatLng(Val(Me.TextBox6.Text), Val(Me.TextBox3.Text))))
            GMapControl1.Overlays(1).Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(New PointLatLng(Val(Me.TextBox5.Text), Val(Me.TextBox2.Text))))
            firstmarkerset = True
            GMapControl1.Overlays(2).Polygons.Clear()
            rectbottomleft.Lat = GMapControl1.Overlays(1).Markers.Item(0).Position.Lat
            rectbottomleft.Lng = GMapControl1.Overlays(1).Markers.Item(0).Position.Lng
            recttopright.Lat = GMapControl1.Overlays(1).Markers.Item(1).Position.Lat
            recttopright.Lng = GMapControl1.Overlays(1).Markers.Item(1).Position.Lng

            Dim pointsRect As New List(Of PointLatLng)
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))

            Dim polygon2 As New GMapPolygon(pointsRect, "RectangleSel")
            polygon2.Fill = New SolidBrush(Color.FromArgb(50, Color.Red))
            polygon2.Stroke = New Pen(Color.Red, 0.25)
            GMapControl1.Overlays(2).Polygons.Add(polygon2)
            latDist = Math.Round(((Val(TextBox5.Text) - Val(TextBox6.Text)) * 60 * 1852) / 1000) 'Distance between two lng-values (simple calculation, not accurate. Should substitute with GetDistanceBetweenPoints (Haversine Formula)
            longDist = Math.Round(((Val(TextBox2.Text) - Val(TextBox3.Text)) * 48 * 1852) / 1000) 'Distance between two lat-values (simple calculation, not accurate. Should substitute with GetDistanceBetweenPoints (Haversine Formula)
            TextBox7.Text = Math.Round(longDist, 2).ToString & " km"
            TextBox8.Text = Math.Round(latDist, 2).ToString & " km"
        End If
    End Sub
    Public Sub syncArea()
        Me.TextBox6.Text = visualForm.GMapControl1.CurrentViewArea.Bottom
        Me.TextBox3.Text = visualForm.GMapControl1.CurrentViewArea.Left
        Me.TextBox5.Text = visualForm.GMapControl1.CurrentViewArea.Top
        Me.TextBox2.Text = visualForm.GMapControl1.CurrentViewArea.Right
    End Sub
    Public Sub syncMaps(Optional reverse As Boolean = False)
        Dim topleft As PointLatLng
        Dim heightLat, widthLong As Double
        If reverse = False Then
            heightLat = visualForm.GMapControl1.CurrentViewArea.HeightLat
            widthLong = visualForm.GMapControl1.CurrentViewArea.WidthLng
            topleft.Lng = visualForm.GMapControl1.CurrentViewArea.Left
            topleft.Lat = visualForm.GMapControl1.CurrentViewArea.Top
            GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(topleft, New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(heightLat, widthLong))))
        Else
            heightLat = GMapControl1.CurrentViewArea.HeightLat
            widthLong = GMapControl1.CurrentViewArea.WidthLng
            topleft.Lng = GMapControl1.CurrentViewArea.Left
            topleft.Lat = GMapControl1.CurrentViewArea.Top
            visualForm.GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(topleft, New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(heightLat, widthLong))))
        End If
    End Sub

    Private Sub TextBox10_Click(sender As Object, e As EventArgs) Handles TextBox10.Click
        RadioButton2.Enabled = True
        RadioButton5.Enabled = True
        RadioButton8.Enabled = True
        CheckBox17.Enabled = True
        CheckBox18.Enabled = True
        CheckBox19.Enabled = True
        CheckBox28.Enabled = True
        TextBox11.Enabled = True
        RadioButton2.Checked = True

        DataFiltering = True
        CheckBox26.Checked = False
    End Sub

    Private Sub TextBox10_Validated(sender As Object, e As EventArgs) Handles TextBox10.Validated
        If TextBox10.Text = String.Empty Then
            RadioButton2.Enabled = False
            RadioButton5.Enabled = False
            RadioButton8.Enabled = False
            CheckBox17.Enabled = False
            CheckBox18.Enabled = False
            CheckBox19.Enabled = False
            TextBox11.Enabled = False
            CheckBox28.Enabled = False

            DataFiltering = False
            CheckBox26.Checked = True
        End If
    End Sub

    Private Sub TextBox11_Click(sender As Object, e As EventArgs) Handles TextBox11.Click
        RadioButton9.Enabled = True
        RadioButton7.Enabled = True
        RadioButton6.Enabled = True
        CheckBox20.Enabled = True
        CheckBox21.Enabled = True
        CheckBox22.Enabled = True
        RadioButton9.Checked = True
        TextBox12.Enabled = True
    End Sub

    Private Sub TextBox11_Validated(sender As Object, e As EventArgs) Handles TextBox11.Validated
        If TextBox11.Text = String.Empty Then
            RadioButton9.Enabled = False
            RadioButton7.Enabled = False
            RadioButton6.Enabled = False
            CheckBox20.Enabled = False
            CheckBox21.Enabled = False
            CheckBox22.Enabled = False
            TextBox12.Enabled = False
        End If
    End Sub

    Private Sub TextBox12_Click(sender As Object, e As EventArgs) Handles TextBox12.Click
        CheckBox23.Enabled = True
        CheckBox24.Enabled = True
        CheckBox25.Enabled = True
    End Sub

    Private Sub TextBox12_Validated(sender As Object, e As EventArgs) Handles TextBox12.Validated
        If TextBox12.Text = String.Empty Then
            CheckBox23.Enabled = False
            CheckBox24.Enabled = False
            CheckBox25.Enabled = False
        End If
    End Sub

    Private Sub Button5_Click_1(sender As Object, e As EventArgs) Handles Button5.Click
        If maploaded = False Then
            MsgBox("Please click on Load Folder (Data) first.")
        Else
            CheckBox15.Checked = True
        End If

    End Sub

    Sub savesettings()
        'Save Settings
        My.Settings.Sourcepath = Me.TextBox1.Text
        My.Settings.Save()
    End Sub



End Class

'Filenamedata Object definition (Grid consisting of 4 Lat/Long values)
Public Class SourceData
    Private _filename As String
    Public Property filename() As String
        Get
            Return _filename
        End Get
        Set(value As String)
            _filename = value
        End Set
    End Property

    Private _toplat As Double
    Public Property toplat() As Double
        Get
            Return _toplat
        End Get
        Set(value As Double)
            _toplat = value
        End Set
    End Property

    Private _bottomlat As Double
    Public Property bottomlat() As Double
        Get
            Return _bottomlat
        End Get
        Set(value As Double)
            _bottomlat = value
        End Set
    End Property

    Private _rightlong As Double
    Public Property rightlong() As Double
        Get
            Return _rightlong
        End Get
        Set(value As Double)
            _rightlong = value
        End Set
    End Property

    Private _leftlong As Double
    Public Property leftlong() As Double
        Get
            Return _leftlong
        End Get
        Set(value As Double)
            _leftlong = value
        End Set
    End Property

    Private _datafiles As New List(Of String)
    Public Property datafiles() As List(Of String)
        Get
            Return _datafiles
        End Get
        Set(value As List(Of String))
            _datafiles = value
        End Set
    End Property
    Private _fromDate As String
    Public Property fromDate() As String
        Get
            Return _fromDate
        End Get
        Set(value As String)
            _fromDate = value
        End Set
    End Property
    Private _toDate As String
    Public Property toDate() As String
        Get
            Return _toDate
        End Get
        Set(value As String)
            _toDate = value
        End Set
    End Property
    Private _photosPerFile As Long
    Public Property photosPerFile() As Long
        Get
            Return _photosPerFile
        End Get
        Set(value As Long)
            _photosPerFile = value
        End Set
    End Property
    Private _TotalPhotos As Long
    Public Property TotalPhotos() As Long
        Get
            Return _TotalPhotos
        End Get
        Set(value As Long)
            _TotalPhotos = value
        End Set
    End Property
End Class

