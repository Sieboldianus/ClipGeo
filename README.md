**ClipGeo** is a side project I developed over the years to clip, extract and visualize simple but very 
large georeferenced point datasets (such as 200 Million Flickr photo locations). The tool is optimized for speed. 
For example, by using a geo-modified binary search, it is possible to map and extract 6 million photo locations in Germany in under 2 Minutes on a regular laptop.

![ClipGeo Vis Example](https://github.com/Sieboldianus/ClipGeo/blob/master/ressources/Europe_b.png?raw=true)

## Code Example

- todo: add formal conventions for file handling, code structure etc.

## Motivation

Filtering, extracting, clipping and visualizing large georeferenced point datasets is not possible with common GIS Software, such as ESRI ArcGIS. 
My experience was that beyond 5 million points, ArcGIS quits. This tool was build to initially extract parts of a larger point dataset to be imported into other Software, 
such as ArcGIS, for more advanced analysis.

## Installation

- currently, no installation is needed
- DotNet framework 4.5 recommended
- download debug folder and start executable or install using setup files

## Tests

- todo: provide tutorial

## Contributors

- todo: future goals, extending scope of program beyond Flickr photo data (include Twitter & Instagram, for example)

## Use of other Software and Code
This project includes and makes use of several other projects/libraries/frameworks:

>[*FastPix*](http://www.vbforums.com/showthread.php?586709-FastPix-Rapid-Pixel-Processing-for-Dummies-and-Dudes) - (c) Vic Joseph (Boops Boops) 2009-2013
>> A fast substitute for Bitmap.GetPixel and Bitmap.SetPixel
>> Used for fast bitmap manipulations (map render, alpha values) in visual.vb

>[*DotSpatial*](https://github.com/DotSpatial/DotSpatial) (DotSpatial.Data & DotSpatial.Topology)
>>Opening Shapefiles & extracting point coordinates; Optional used in Shapefile intersect method in clipdata.vb

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
>Initial commit: cleaned up project, removed unnecessary references to functions not used anymore.

>This project is a branch of a larger project. I removed all links to the larger project to continue developing  this branch separately.

>Todo:
>>- translate comments to english
>>- clean up the mess
>>- provide instructions for using program

