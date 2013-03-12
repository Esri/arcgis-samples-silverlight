# arcgis-samples-silverlight

This project contains the C# and VB source code for the ArcGIS API for Silverlight interactive sample app.  

[View it live](http://resources.arcgis.com/en/help/silverlight-api/samples/start.htm)
[![Image of sample app](https://raw.github.com/Esri/arcgis-samples-silverlight/master/arcgis-samples-silverlight.png "Interactive sample app")](http://resources.arcgis.com/en/help/silverlight-api/samples/start.htm)


## Instructions (for the Silverlight 5 platform)

1. Fork and then clone the repo or download the .zip file. 
2. Download the [ArcGIS API for Silverlight (requires Esri Global account)](http://www.esri.com/apps/products/download/index.cfm?fuseaction=download.main&downloadid=876).   
3. Download and install the [December 2011 version of the Silverlight 5 Toolkit](http://silverlight.codeplex.com/) on CodePlex. 
4. Download and install the [Expression Blend Preview for Silverlight 5](http://www.microsoft.com/en-us/download/details.aspx?id=9503). 
5. Download and install the [Silverlight 5 Tools for Visual Studio 2010 SP1](http://www.microsoft.com/en-us/download/details.aspx?id=28358).
6. Two solutions are included.  One with CSharp code, and the other with VB.NET code. Each solution contains two projects, a Silverlight project and a web host application.  In Visual Studio, open the CSharp or VB.NET solution. If you want to see the VB.NET code-behind pages when running the CSharp solution, the VB.NET solution must be built before the CSharp solution.
7. If necessary, repair the references to the ArcGIS API for Silverlight assemblies, System.Windows.Controls.Toolkit assembly (Silverlight 5 Toolkit), and System.Windows.Interactivity assembly (Expression Blend Preview for Silverlight 5) in the Silverlight application.
8. Colorized text in the XAML and code-behind views is generated using logic in the SyntaxHighlighting.dll included with the Silverlight project under the Support folder. Since the assembly is included with the interactive SDK that was downloaded from the ArcGIS Resource Center, unblock it before building and running the application. For instructions on how to unblock an assembly for Visual Studio, see How to:Use an assembly from the web in Visual Studio.
9. In the Solution Explorer, right-click the c:\...\ArcGISSilverlightSDKWeb\ project and elect "Set as StartUp Project".
10. Clean and build the solution, then run the application. 

## Requirements

* [Supported system configurations](http://resources.arcgis.com/en/help/silverlight-api/concepts/#/System_requirements/01660000000t000000/)

## Resources

* [ArcGIS API for Silverlight Resource Center](http://resources.arcgis.com/en/communities/silverlight-api/index.html)
* [ArcGIS API for Silverlight download (requires Esri Global account)](http://www.esri.com/apps/products/download/index.cfm?fuseaction=download.main&downloadid=876)

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

