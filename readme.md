# arcgis-samples-silverlight

This project contains the C# and VB source code for the ArcGIS API for Silverlight interactive sample app.  

[View it live](http://resources.arcgis.com/en/help/arcgis-toolkit-sl-wpf/samples/start.htm)

![App](https://raw.github.com/Esri/arcgis-toolkit-sl-wpf/master/arcgis-toolkit-sl-wpf.png)

## Instructions

1. Fork and then clone the repo or download the .zip file. 
2. Download the ArcGIS API for Silverlight   
3. Download and install the December 2011 version of the Silverlight 5 Toolkit on CodePlex. If necessary, repair the reference to the System.Windows.Controls.Toolkit assembly.
Download and install the Expression Blend 4 SDK for Silverlight. If necessary, repair the reference to the System.Windows.Interactivity assembly.
Unzip the sample to a location of your choice. Two solutions are included—one with CSharp code, and the other with VB.NET code. Each solution contains two projects, a Silverlight project and a web host application.
In Visual Studio 2010, open the CSharp or VB.NET solution. The Silverlight 5 Tools for Visual Studio 2010 SP1 must be installed.
NoteNote:

If you want to see the VB.NET code-behind pages when running the CSharp solution, the VB.NET solution must be built before the CSharp solution.
If necessary, repair the references to the ArcGIS API for Silverlight assemblies in the Silverlight application.
Colorized text in the XAML and code-behind views is generated using logic in the SyntaxHighlighting.dll included with the Silverlight project under the Support folder. Since the assembly is included with the interactive SDK that was downloaded from the ArcGIS Resource Center, unblock it before building and running the application. For instructions on how to unblock an assembly for Visual Studio, see How to: Use an assembly from the web in Visual Studio.
In the Solution Explorer, right-click the c:\...\ArcGISSilverlightSDKWeb\ project and select Set as StartUp Project.
Clean and build the solution, then run the application. 

## Requirements

* ArcGIS API for Silverlight
* Visual Studio 2010 or 2012

See instructions above.

## Resources

* [ArcGIS API for Silverlight Resource Center](http://resources.arcgis.com/en/communities/silverlight-api/index.html)
* [ArcGIS API for Silverlight download (requires Esri Global account](http://www.esri.com/apps/products/download/index.cfm?fuseaction=download.main&downloadid=876)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to contribute. 

## Licensing
Copyright 2012 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt]( https://raw.github.com/Esri/arcgis-samples-silverlight/master/license.txt) file.

[](Esri Tags: ArcGIS API Silverlight)
[](Esri Language: Silverlight)
