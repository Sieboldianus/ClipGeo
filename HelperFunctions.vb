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
End Class
