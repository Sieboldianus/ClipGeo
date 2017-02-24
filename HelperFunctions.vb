Imports System.IO
Imports GMap.NET
Imports GMap.NET.WindowsForms

Public Class HelperFunctions
    Public Shared AppPath As String = Application.StartupPath() & "\"
    Public Shared versionnumber As String = Application.ProductVersion

    Public Shared Function GetSettingItem(ByVal File As String, ByVal Identifier As String) As String
        Using S As New IO.StreamReader(File) : Dim Result As String = ""
            Do While (S.Peek <> -1)
                Dim line As String = S.ReadLine
                If line.ToLower.StartsWith(Identifier.ToLower & ":") Then
                    Result = line.Substring(Identifier.Length + 2)
                End If
            Loop
            Return Result
        End Using
        'S.Close()
        'S = Nothing
    End Function

    Public Shared Sub InitialDrawMarkers(centerlat As Double, centerlong As Double, toplat As Double, bottomlat As Double, leftlong As Double, rightlong As Double, ByRef points As List(Of PointLatLng), ByRef polygon_red As GMapPolygon, startDataset As SourceData, ByRef overlayOne As GMapOverlay, ByRef MarkerOverlay As GMapOverlay)

        Dim MarkerRectOverlay As New GMapOverlay(ClipDataForm.GMapControl1, "RectMarker")
        ClipDataForm.GMapControl1.Overlays.Add(overlayOne)


        points.Add(New PointLatLng(toplat, leftlong))
        points.Add(New PointLatLng(toplat, rightlong))
        points.Add(New PointLatLng(bottomlat, rightlong))
        points.Add(New PointLatLng(bottomlat, leftlong))
        points.Add(New PointLatLng(toplat, leftlong))

        polygon_red.Fill = New SolidBrush(Color.FromArgb(0, Color.White))
        polygon_red.Stroke = New Pen(Color.Red, 0.25)
        overlayOne.Polygons.Add(polygon_red)
        ClipDataForm.GMapControl1.ZoomAndCenterMarkers(overlayOne.Id)

        If ClipDataForm.CheckBox2.Checked = False Then
            ClipDataForm.GMapControl1.Overlays.Add(MarkerOverlay)
            Dim rectbottomleft As PointLatLng
            Dim recttopright As PointLatLng

            rectbottomleft.Lat = bottomlat + (Math.Abs(Math.Abs(toplat) - Math.Abs(bottomlat)) / 4)
            rectbottomleft.Lng = leftlong + (Math.Abs(Math.Abs(rightlong) - Math.Abs(leftlong)) / 4)
            recttopright.Lat = toplat - (Math.Abs(Math.Abs(toplat) - Math.Abs(bottomlat)) / 4)
            recttopright.Lng = rightlong - (Math.Abs(Math.Abs(rightlong) - Math.Abs(leftlong)) / 4)



            Dim pointsRect As New List(Of PointLatLng)
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, recttopright.Lng))
            pointsRect.Add(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng))
            pointsRect.Add(New PointLatLng(recttopright.Lat, rectbottomleft.Lng))


            Dim polygon2 As New GMapPolygon(pointsRect, "RectangleSel")
            polygon2.Fill = New SolidBrush(Color.FromArgb(50, Color.Red))
            polygon2.Stroke = New Pen(Color.Red, 0.25)
            MarkerRectOverlay.Polygons.Add(polygon2)
            ClipDataForm.GMapControl1.Overlays.Add(MarkerRectOverlay)

            'Set Zoom/Pan to First Dataset Rect
            'GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(New GMap.NET.PointLatLng(filename_data.Item(0).toplat, filename_data.Item(0).leftlong), New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(filename_data.Item(0).toplat - filename_data.Item(0).bottomlat, filename_data.Item(0).rightlong - filename_data.Item(0).leftlong))))
            If startDataset IsNot Nothing Then
                ClipDataForm.GMapControl1.SetZoomToFitRect(New GMap.NET.RectLatLng(New GMap.NET.PointLatLng(startDataset.toplat, startDataset.leftlong), New GMap.NET.SizeLatLng(New GMap.NET.PointLatLng(startDataset.toplat - startDataset.bottomlat, startDataset.rightlong - startDataset.leftlong))))
            End If
            MarkerOverlay.Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(New PointLatLng(rectbottomleft.Lat, rectbottomleft.Lng)))
            MarkerOverlay.Markers.Add(New GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(New PointLatLng(recttopright.Lat, recttopright.Lng)))

        End If
    End Sub


    '' User Location Functions
    Public Shared UserGeocodeIndex_Path As String = AppPath & "\Output\02_UserData\00_Index\UserGeocodeIndex.txt"
    Public Shared UserLocationGeocodeDict As Dictionary(Of String, KeyValuePair(Of Double, Double)) = New Dictionary(Of String, KeyValuePair(Of Double, Double))(System.StringComparer.OrdinalIgnoreCase) 'Dictionary of String-Location to lat/lng values

    'Read UserGeocodeLocationDatabase into Dictionary
    Public Shared Sub LoadUserLocationGeocodeIndex()
        Dim filenamepath As String = UserGeocodeIndex_Path
        Dim linetextArr As String()
        UserLocationGeocodeDict.Clear()
        If File.Exists(filenamepath) Then
            Dim objReader As New System.IO.StreamReader(filenamepath)
            Do While objReader.Peek() <> -1
                linetextArr = objReader.ReadLine().Split(",")
                Dim resultLatitude As Double = linetextArr(0)
                Dim resultLongitude As Double = linetextArr(1)
                Dim resultUserID As String = Replace(linetextArr(2), ";", ",") 'Location
                UserLocationGeocodeDict(resultUserID) = New KeyValuePair(Of Double, Double)(resultLatitude, resultLongitude)
            Loop
            objReader.Close()
        End If
    End Sub

    'See http://stackoverflow.com/questions/10175724/calculate-distance-between-two-points-in-bing-maps
    'Haversine Formula
    Public Shared Function GetDistanceBetweenPoints(lat1 As Double, long1 As Double, lat2 As Double, long2 As Double) As Double
        Dim distance As Double = 0

        Dim dLat As Double = (lat2 - lat1) / 180 * Math.PI
        Dim dLong As Double = (long2 - long1) / 180 * Math.PI

        Dim a As Double = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 / 180 * Math.PI) * Math.Cos(lat2 / 180 * Math.PI) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2)
        Dim c As Double = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))

        'Calculate radius of earth
        ' For this you can assume any of the two points.
        Dim radiusE As Double = 6378135
        ' Equatorial radius, in metres
        Dim radiusP As Double = 6356750
        ' Polar Radius
        'Numerator part of function
        Dim nr As Double = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2)
        'Denominator part of the function
        Dim dr As Double = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2) + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2)
        Dim radius As Double = Math.Sqrt(nr / dr)

        'Calculate distance in meters.
        distance = radius * c
        Return distance
        ' distance in meters
    End Function

    Public Shared Function TransferSettings(OrigSettingsfile As String, NewSettingsfile As System.IO.TextWriter) As Boolean
        Dim bottomlat As String = ""
        Dim leftlong As String = ""
        Dim toplat As String = ""
        Dim rightlong As String = ""
        Dim querytype As String = "upload_time"
        Dim minuploaddate As String = ""
        Dim maxuploaddate As String = ""
        Dim maxtakendate As String = ""
        Dim mintakendate As String = ""
        Dim geoquery As String = "bbox"
        Dim bboxwidth As String = ""
        Dim bboxlength As String = ""
        Dim tags As String = ""
        Dim accuracy As String = "16"
        Dim safesearch As String = "3"
        Dim contenttype As String = ""
        Dim profilename As String = ""
        Dim lat As String = ""
        Dim lng As String = ""
        Dim perpage As String = ""
        Dim sort As String = ""
        Dim maxquery As String = ""
        Dim queryname As String = ""
        Dim subgrid As String = ""
        Dim tilesize As String = ""
        Dim subgridStart As String = ""
        Dim subgridEnd As String = ""
        Dim querysnooze As String = ""
        Dim queryWaitTime As String = ""
        Dim queryAPICallWait As String = ""
        Dim radius As String = ""

        'Create new file / transfer items
        NewSettingsfile = System.IO.File.CreateText(AppPath & "\settings.txt")

        'MsgBox(OrigSettingsfile)
        profilename = GetSettingItem(OrigSettingsfile, "profilename")
        queryname = GetSettingItem(OrigSettingsfile, "queryname")
        lat = GetSettingItem(OrigSettingsfile, "lat")
        lng = GetSettingItem(OrigSettingsfile, "lng")
        bottomlat = GetSettingItem(OrigSettingsfile, "bottomlat")
        leftlong = GetSettingItem(OrigSettingsfile, "leftlong")
        toplat = GetSettingItem(OrigSettingsfile, "toplat")
        rightlong = GetSettingItem(OrigSettingsfile, "rightlong")
        geoquery = GetSettingItem(OrigSettingsfile, "geoquery")
        querytype = GetSettingItem(OrigSettingsfile, "querytype")

        If geoquery = "radial" Then
            radius = GetSettingItem(OrigSettingsfile, "radius")
        Else
            bboxwidth = GetSettingItem(OrigSettingsfile, "bboxwidth")
            bboxlength = GetSettingItem(OrigSettingsfile, "bboxlength")
        End If
        If querytype = "upload_time" Then
            minuploaddate = GetSettingItem(OrigSettingsfile, "minuploaddate")
            maxuploaddate = GetSettingItem(OrigSettingsfile, "maxuploaddate")
        Else '"date_taken"
            mintakendate = GetSettingItem(OrigSettingsfile, "mintakendate")
            mintakendate = GetSettingItem(OrigSettingsfile, "maxtakendate")
        End If

        tags = GetSettingItem(OrigSettingsfile, "tags")
        accuracy = GetSettingItem(OrigSettingsfile, "accuracy")
        bboxlength = GetSettingItem(OrigSettingsfile, "bboxlength")
        safesearch = GetSettingItem(OrigSettingsfile, "safesearch")
        contenttype = GetSettingItem(OrigSettingsfile, "contenttype")

        'Export Settings to File
        NewSettingsfile.WriteLine("profilename: " & profilename)

        NewSettingsfile.WriteLine("lat: " & lat)
        NewSettingsfile.WriteLine("lng: " & lng)
        NewSettingsfile.WriteLine("bottomlat: " & bottomlat)
        NewSettingsfile.WriteLine("leftlong: " & leftlong)
        NewSettingsfile.WriteLine("toplat: " & toplat)
        NewSettingsfile.WriteLine("rightlong: " & rightlong)

        If querytype = "upload_time" = True Then
            NewSettingsfile.WriteLine("querytype: upload_time")
            NewSettingsfile.WriteLine("minuploaddate: " & minuploaddate)
            NewSettingsfile.WriteLine("maxuploaddate: " & maxuploaddate)
        Else
            NewSettingsfile.WriteLine("querytype: date_taken")
            NewSettingsfile.WriteLine("mintakendate: " & mintakendate)
            NewSettingsfile.WriteLine("maxtakendate: " & maxtakendate)
        End If

        If geoquery = "radial" Then
            NewSettingsfile.WriteLine("geoquery: radial")
            NewSettingsfile.WriteLine("radius: " & Me.radius.Text)
        Else
            NewSettingsfile.WriteLine("geoquery: bbox")
            NewSettingsfile.WriteLine("bboxwidth: " & bboxwidth)
            NewSettingsfile.WriteLine("bboxlength: " & bboxlength)
        End If
        NewSettingsfile.WriteLine("tags: " & tags)
        NewSettingsfile.WriteLine("accuracy: " & accuracy)
        NewSettingsfile.WriteLine("safesearch: " & safesearch)
        NewSettingsfile.WriteLine("contenttype: " & contenttype)

        NewSettingsfile.WriteLine("sort: " & sort)
        NewSettingsfile.WriteLine("maxquery: " & maxquery)
        NewSettingsfile.WriteLine("queryname: " & queryname)

        NewSettingsfile.WriteLine("GetGeoVersion: " & versionnumber)


        NewSettingsfile.Flush()
        NewSettingsfile.Close()
        TransferSettings = True
    End Function
End Class
