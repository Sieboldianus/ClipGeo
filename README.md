ClipGeo 
=============
Spatial Point Data Extraction and Visualization

**ClipGeo** is a side project I developed over the years to clip, extract and visualize simple but very 
large georeferenced point datasets (such as 200 Million Flickr photo locations). The tool is optimized for speed. 
For example, by using a geo-modified binary search, it is possible to map and extract 6 million photo locations in Germany in under 2 Minutes on a regular laptop.

![ClipGeo Vis Example](https://github.com/Sieboldianus/ClipGeo/blob/master/ressources/Europe_b.png?raw=true)

## Motivation

Filtering, extracting, clipping and visualizing large georeferenced point datasets is not possible with common GIS Software, such as ESRI ArcGIS. 
My experience was that beyond 5 million points, ArcGIS quits. This tool was build to initially extract parts of a larger point dataset to be imported into other Software, 
such as ArcGIS, for more advanced analysis.

## Code Example

The following code is at the heart of the Lat/Lng-point mapping. It is a fast, geo-modified binary search that assigns the best pixel-location for each pair of coordinates. After doing some research, I believe this is one of the fastest ways to map millions of points in just a few seconds.

```vbnet
    'Point Mapping function: Input (LatLng), Output (Best pixel-location on map)
    'Needs precalculation of Pixel-LatLng-grid (Sub: precalcValues)
    Function bestPixel(ByVal searchValueLat As Double, ByVal searchValuelng As Double) As GMap.NET.GPoint
        'Binary Search for best corresponding pixel ID on map
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
    
    'Precalculate LatLng for each map pixel
    Public Sub precalcValues(ByVal Height As Integer, ByVal Width As Integer)
        YList.Clear() 'Sorted List for binary search
        YDict.Clear() 'Dictionary for fast assigning of coordinates
        XList.Clear() 'Sorted List for binary search
        XDict.Clear() 'Dictionary for fast assigning of coordinates

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

## Installation/ Getting Started

* currently, no installation is needed
* DotNet framework 4.5 recommended
* download debug folder and start executable or install using setup files

## Formal conventions

* todo: add formal conventions for file handling, code structure etc.

## Tests

* todo: provide tutorial

## Contributors

* todo: future goals, extending scope of program beyond Flickr photo data (include Twitter & Instagram, for example)

## Built With
This project includes and makes use of several other projects/libraries/frameworks:

>[*FastPix*](http://www.vbforums.com/showthread.php?586709-FastPix-Rapid-Pixel-Processing-for-Dummies-and-Dudes) - (c) Vic Joseph (Boops Boops) 2009-2013
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

## Changelog

2017-02-08:

* Initial commit: cleaned up project, removed unnecessary references to functions not used anymore.

* This project is a branch of a larger project. I removed all links to the larger project to continue developing  this branch separately.

* Todo:
    * translate comments to english
    * clean up the mess
    * provide instructions for using program

[//]: # (Readme formatting based on https://gist.github.com/PurpleBooth/109311bb0361f32d87a2) 
