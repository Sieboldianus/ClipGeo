Imports System.IO

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
