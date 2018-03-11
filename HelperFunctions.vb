Imports System.IO
Imports GMap.NET
Imports GMap.NET.WindowsForms
Imports System.Reflection

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
    'Load Source Mapping for different types of data
    Public Shared Function LoadSourceTypeMapping(ByVal File As String) As sourcetype
        Dim currentDataSourceType As New sourcetype
        Dim config_properties As New List(Of String)
        Dim _type As Type = currentDataSourceType.GetType()
        Dim properties() As PropertyInfo = _type.GetProperties()
        Using S As New IO.StreamReader(File)
            Do While (S.Peek <> -1)
                Dim line As String = S.ReadLine
                For Each _property As PropertyInfo In properties
                    If line.StartsWith(_property.Name) Then
                        'MsgBox(_property.Name & " set to : " & line.Substring(_property.Name.Length + 2))
                        Dim pvalue As String = line.Substring(_property.Name.Length + 2)
                        If _property.PropertyType Is GetType(Integer) Then
                            _property.SetValue(currentDataSourceType, Convert.ToInt32(pvalue))
                        ElseIf _property.PropertyType Is GetType(Char) Then
                            _property.SetValue(currentDataSourceType, Convert.ToChar(pvalue))
                        Else
                            _property.SetValue(currentDataSourceType, pvalue)
                        End If
                        Exit For
                    End If
                Next
            Loop
            Return currentDataSourceType
        End Using
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
    Public Shared UserGeocodeIndex_Path As String '= AppPath & "\Output\02_UserData\00_Index\UserGeocodeIndex.txt"
    Public Shared UserGeocodeIndex_PathCC As String '= AppPath & "\Output\02_UserData\00_Index\UserGeocodeIndex_inclCountry.txt"
    Public Shared UserLocationGeocodeDict As Dictionary(Of String, KeyValuePair(Of Double, Double)) = New Dictionary(Of String, KeyValuePair(Of Double, Double))(System.StringComparer.OrdinalIgnoreCase) 'Dictionary of String-Location to lat/lng values
    Public Shared UserOriginCountryDict As Dictionary(Of String, KeyValuePair(Of String, String)) = New Dictionary(Of String, KeyValuePair(Of String, String))(System.StringComparer.OrdinalIgnoreCase) 'Dictionary of String-origin based on Country, e.g. "Userxyz, Germany"

    'Read UserGeocodeLocationDatabase into Dictionary
    Public Shared Sub LoadUserLocationGeocodeIndex(Optional CC As Boolean = False)
        'if CC true, then a different file is loaded (UserGeocodeIndex_inclCountry.txt), which includes Country & Continent for each user
        'these are aggregated to analyze & visualize user origin bias
        Dim filenamepath As String
        If CC = False Then
            filenamepath = UserGeocodeIndex_Path
        Else
            filenamepath = UserGeocodeIndex_PathCC
        End If
        Dim linetextArr As String()
        UserLocationGeocodeDict.Clear()
        If File.Exists(filenamepath) Then
            Dim objReader As New System.IO.StreamReader(filenamepath)
            If CC Then linetextArr = objReader.ReadLine().Split(",") 'Skip first line (header) for CC
            Do While objReader.Peek() <> -1
                linetextArr = objReader.ReadLine().Split(",")
                Dim resultLatitude As Double = linetextArr(0)
                Dim resultLongitude As Double = linetextArr(1)
                Dim resultUserID As String = Replace(linetextArr(2), ";", ",") 'Location
                If CC = False Then
                    UserLocationGeocodeDict(resultUserID) = New KeyValuePair(Of Double, Double)(resultLatitude, resultLongitude)
                Else
                    Dim resultUserCountry As String = linetextArr(3)
                    Dim resultUserContinent As String = linetextArr(4)
                    UserOriginCountryDict(resultUserID) = New KeyValuePair(Of String, String)(resultUserCountry, resultUserContinent)
                End If
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

    Public Shared Function TransferSettings(OrigSettingsfile As String, NewSettingsfilePath As String) As Boolean
        Dim NewSettingsDict As Dictionary(Of String, String) = New Dictionary(Of String, String)(System.StringComparer.OrdinalIgnoreCase)
        'Add here the list of settings that need to get transferred, all others will be ignored
        'Settings that have values will overwrite values upon transfer
        Dim splitPath As String() = OrigSettingsfile.Split("\")
        Dim parentFolder As String = splitPath(splitPath.Length - 2)
        NewSettingsDict.Add("profilename", parentFolder)
        NewSettingsDict.Add("bottomlat", "")
        NewSettingsDict.Add("leftlong", "")
        NewSettingsDict.Add("toplat", "")
        NewSettingsDict.Add("rightlong", "")
        NewSettingsDict.Add("querytype", "") 'upload_time
        NewSettingsDict.Add("minuploaddate", "") '1/1/2007
        NewSettingsDict.Add("maxuploaddate", "") '1/1/2017
        NewSettingsDict.Add("maxperfile", "") '50000
        'NewSettingsDict.Add("maxtakendate","")
        'NewSettingsDict.Add("mintakendate", "")
        'NewSettingsDict.Add("geoquery","bbox")
        'NewSettingsDict.Add("bboxwidth", "")
        'NewSettingsDict.Add("bboxlength","")
        'NewSettingsDict.Add("tags", "")
        'NewSettingsDict.Add("accuracy","16")
        'NewSettingsDict.Add("safesearch", "3")
        'NewSettingsDict.Add("contenttype","")
        'NewSettingsDict.Add("lat", "")
        'NewSettingsDict.Add("lng", "")
        'NewSettingsDict.Add("perpage","")
        'NewSettingsDict.Add("sort", "")
        'NewSettingsDict.Add("maxquery","")
        'NewSettingsDict.Add("queryname", "")
        'NewSettingsDict.Add("subgrid","")
        'NewSettingsDict.Add("tilesize", "")
        'NewSettingsDict.Add("subgridStart","")
        'NewSettingsDict.Add("subgridEnd", "")
        'NewSettingsDict.Add("querysnooze","")
        'NewSettingsDict.Add("queryWaitTime", "")
        'NewSettingsDict.Add("queryAPICallWait","")
        'NewSettingsDict.Add("radius", "")
        'NewSettingsDict.Add("GetGeoVersion", "")


        Dim OldSettingsDict As Dictionary(Of String, String) = New Dictionary(Of String, String)

        Using S As New IO.StreamReader(OrigSettingsfile) : Dim Result As String = ""
            Do While (S.Peek <> -1)
                Dim line As String = S.ReadLine
                If line.Contains(":") Then
                    Dim split As String() = line.Split(":")
                    OldSettingsDict.Add(split(0).Trim, split(1).Trim)
                End If
            Loop
        End Using

        'Create new file / transfer items
        Dim NewSettingsfile As System.IO.TextWriter = System.IO.File.CreateText(NewSettingsfilePath)
        For Each setting As String In NewSettingsDict.Keys
            If NewSettingsDict(setting) = "" Then 'Only copy settings if not otherwise pre-configured
                For Each oldsetting As String In OldSettingsDict.Keys
                    If oldsetting = setting Then
                        NewSettingsfile.WriteLine(setting & ": " & OldSettingsDict(oldsetting))
                    End If
                Next
            Else
                NewSettingsfile.WriteLine(setting & ": " & NewSettingsDict(setting))
            End If
        Next
        'NewSettingsfile.WriteLine("ClipGeoVersion: " & versionnumber)
        NewSettingsfile.Flush()
        NewSettingsfile.Close()
        TransferSettings = True
    End Function
End Class
