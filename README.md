ClipGeo 
=============
Large Spatial Point Dataset Extraction and Visualization

**ClipGeo** is a side project I developed over the years to clip, extract and visualize simple but very 
large georeferenced point datasets (such as 200 Million Flickr photo locations). The tool is optimized for speed. 
For example, by using a geo-modified binary search, it is possible to map and extract 6 million photo locations in Germany in under 2 Minutes on a regular laptop.

![ClipGeo Vis Example](https://raw.githubusercontent.com/Sieboldianus/ClipGeo/master/Resources/Europe_b.png)
![ClipGeo Interface Ani](https://raw.githubusercontent.com/Sieboldianus/ClipGeo/master/Resources/interface.gif)

## Motivation

Filtering, extracting, clipping and visualizing large georeferenced point datasets is not possible with common GIS Software, such as ESRI ArcGIS. 
My experience was that beyond 5 million points, ArcGIS quits. This tool was build to initially extract parts of a larger point dataset to be imported into other Software, 
such as ArcGIS, for more advanced analysis. How the speed of reading data and exporting CSV data is improved in ClipGeo:

* CSV files are not read fully, column by column, but line by line, using Streamreader, and only up to the point in each line where data is required
* When exporting/clipping data, CSVs are not re-formatted. Instead, each line is copied as a whole from original to output CSV
* Data is pre-structured to improve clipping and filtering. So far, data can be structured spatially using QuadTrees (each subfolder represents a single quad) or temporally based on Days, Months or Years (etc.)

## Code Example

The following code is at the heart of the Lat/Lng-point mapping. It is a fast, geo-modified binary search that assigns the best pixel-location for each pair of coordinates. After doing some research, I believe this is one of the fastest ways to map millions of points in just a few seconds.

```vb
    'Sorted list for binary search lat/long
    Dim YList As New List(Of Double)
    Dim XList As New List(Of Double)
    'Dictionary for fast assigning of coordinates to pixels
    Dim YDict As Dictionary(Of Double, Integer) = New Dictionary(Of Double, Integer)
    Dim XDict As Dictionary(Of Double, Integer) = New Dictionary(Of Double, Integer)
    
    Function bestPixel(ByVal searchValueLat As Double, ByVal searchValuelng As Double) As GMap.NET.GPoint
        'Point Mapping function: Input (LatLng), Output (Best pixel-location on map)
        'Needs precalculation of Pixel-LatLng-grid (Sub: precalcValues)         
        Dim indexY As Long = YList.BinarySearch(searchValueLat)
        Dim indexX As Long = XList.BinarySearch(searchValuelng)
        'Binary Search for best corresponding pixel ID on map
        If indexY < 0 Then
            indexY = indexY Xor -1
        End If
        If indexX < 0 Then
            indexX = indexX Xor -1
        End If
        bestPixel.Y = YDict.Item(YList.Item(indexY))
        bestPixel.X = XDict.Item(XList.Item(indexX))
    End Function
        
    Public Sub precalcValues(ByVal Height As Integer, ByVal Width As Integer)
    'Precalculate LatLng for each map pixel
        YList.Clear() 
        YDict.Clear() 
        XList.Clear()
        XDict.Clear()

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
    End Sub
```

## Resources

* [Installation/ Getting Started](../../wiki/Installation-&-Getting-started)
    * [Loading, Clipping and Exporting Data](../../wiki/instructions-01-clipping)
    * [Preview and Mapping of Data](../../wiki/instructions-02-preview)
* [Basic File Structure](../../wiki/Basic-File-Structure)
* [Format Conventions](../../wiki/Basic-Format-Conventions)


## Contributors

* todo: future goals, extending scope of program beyond Flickr photo data (include Twitter & Instagram, for example)

## Built With
This project includes and makes use of several other projects/libraries/frameworks:

>[*FastPix*](http://www.vbforums.com/showthread.php?586709-FastPix-Rapid-Pixel-Processing-for-Dummies-and-Dudes) - (c) Vic Joseph 2009-2013
>> A fast substitute for Bitmap.GetPixel and Bitmap.SetPixel
>> Used for fast bitmap manipulations (map render, alpha values) in visual.vb

>[*DotSpatial*](https://github.com/DotSpatial/DotSpatial) (DotSpatial.Data & DotSpatial.Topology)
>> Opening Shapefiles & extracting point coordinates; Optional used in Shapefile intersect method in clipdata.vb

>[*GMap.Net*](https://github.com/radioman/greatmaps)
>>Online Tile-based map display in windows forms, visualization of shapes & overlays, interface functions (such as selecting analysis extent)

>[*Super Context Menu*](https://www.codeproject.com/Articles/22780/Super-Context-Menu-Strip)
>>Right-Click Context Menu for Photo display on maps (Photo collection based on descending popularity, including links to original photos online)

.. at other times, code was slightly modified before incoorporating it into the project:
>[*Point-In-Polygon Raycasting Algorithm*](http://alienryderflex.com/polygon/) 1998-2007 Darel Rex Finley & Patrick Mullen
>>Slight modification to apply test in a spatial context


## License

GNU GPLv3

## Changelog & Download

2018-03-21 [**ClipGeo v0.9.500**](https://github.com/Sieboldianus/ClipGeo/wiki/publish/ClipGeo_0_9_500.zip)

* Added support for additional CSV structures
    * Tested with Twitter and Instagram data
* Fixed Locals/Tourists filtering
* Removed explicit references to Flickr-CSV structure
* PhotoIDs are now handled as strings, not Long Integers (increased scope)

2017-03-08: [**ClipGeo v0.9.300 Rev19**](https://github.com/Sieboldianus/ClipGeo/wiki/publish/ClipGeo_0_9_300_Rev19.zip)

* First published version
* Added Wiki
* Solved some DPI Display issues
* Minor Bugfixes and improvements, cleaned up code

2017-02-08:

* Initial commit: cleaned up project, removed unnecessary references to functions not used anymore.
* This project is a branch of a larger project. I removed all links to the larger project to continue developing  this branch separately.
* Todo:
    * translate comments to english
    * clean up the mess
    * provide instructions for using program

[//]: # (Readme formatting based on https://gist.github.com/PurpleBooth/109311bb0361f32d87a2) 
