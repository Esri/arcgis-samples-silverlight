# arcgis-samples-silverlight

This project contains the C# and VB source code for the ArcGIS API for Silverlight interactive sample app.  

[View it live](https://developers.arcgis.com/en/silverlight/sample-code/start.htm)

[![Image of sample app](https://raw.github.com/Esri/arcgis-samples-silverlight/master/arcgis-samples-silverlight.png "Interactive sample app")](https://developers.arcgis.com/en/silverlight/sample-code/start.htm)


## Instructions 
####Visual Studio 2010 (includes support for Web Developer Express edition)

1. Fork and then clone the repo or download the .zip file.
2. Assure that Visual Studio 2010 is capable of building Silverlight 5 projects.  See the ArcGIS API for Silverlight [system requirements](https://developers.arcgis.com/en/silverlight/guide/system-requirements.htm) for details.  
3. Two Visual Studio 2010 solutions are included. One with CSharp code (ArcGISSilverlightSDK_VS2010.sln), and the other with VB.NET code
(ArcGISSilverlightSDK_VBNet_VS2010.sln).  Each solution contains two projects, a Silverlight project and a web host application. In Visual Studio, open 
the CSharp or VB.NET solution.
4. Colorized text in the XAML and code-behind views is generated using logic in the SyntaxHighlighting.dll included with the Silverlight project under the Support folder. Since the assembly is included with the source code on GitHub, [unblock it](http://go.microsoft.com/fwlink/?LinkId=179545) before building and running the application. 
5. In the Solution Explorer, right-click the c:...\ArcGISSilverlightSDKWeb\ project and select "Set as StartUp Project".  Select the Default_VS2010.htm 
page, right-click and select the "Set as Start Page" option.    
6. Clean and build the solution.  The solution references Nuget packages to retrieve the following dependencies:
 - [ArcGIS API for Silverlight](https://www.nuget.org/packages/ArcGISSilverlight-All/) (portion of Blend SDK for Silverlight 5 included with [Behaviors package](https://www.nuget.org/packages/ArcGISSilverlight-Behaviors/)) 
 - [Silverlight 5 Toolkit](https://www.nuget.org/packages/SilverlightToolkit-Input/) (includes Core and Input packages)
 - [Microsoft Async](https://www.nuget.org/packages/Microsoft.Bcl.Async)  
   References to the Microsoft Async assemblies have been removed from the samples project for Visual Studio 2010.  Use of Microsoft Async functionality with Silverlight 5 requires Visual Studio 2012 or greater.  Samples that reference the task async pattern are not available in Visual Studio 2010.  
7. Run the application. 

####Visual Studio 2012/2013 (includes support for Express for Web editions)

1. Fork and then clone the repo or download the .zip file.
2. Two Visual Studio solutions are included. One with CSharp code (ArcGISSilverlightSDK.sln), and the other with VB.NET code
(ArcGISSilverlightSDK_VBNet.sln).  Each solution contains two projects, a Silverlight project and a web host application. In Visual Studio, open 
the CSharp or VB.NET solution.
3. Colorized text in the XAML and code-behind views is generated using logic in the SyntaxHighlighting.dll included with the Silverlight project under the Support folder. Since the assembly is included with the source code on GitHub, [unblock it](http://go.microsoft.com/fwlink/?LinkId=179545) before building and running the application. 
4. In the Solution Explorer, right-click the c:...\ArcGISSilverlightSDKWeb\ project and select "Set as StartUp Project".  Select the Default.htm page,
right-click and select the "Set as Start Page" option.    
5. Clean and build the solution.  The solution references Nuget packages to retrieve the following dependencies:
 - [ArcGIS API for Silverlight](https://www.nuget.org/packages/ArcGISSilverlight-All/) (portion of Blend SDK for Silverlight 5 included with [Behaviors package](https://www.nuget.org/packages/ArcGISSilverlight-Behaviors/)) 
 - [Silverlight 5 Toolkit](https://www.nuget.org/packages/SilverlightToolkit-Input/) (includes Core and Input packages)
 - [Microsoft Async](https://www.nuget.org/packages/Microsoft.Bcl.Async)  
   The following error will be returned when the samples project is built the first time.    
_"The build restored NuGet packages. Build the project again to include these packages in the build. For more information, see
http://go.microsoft.com/fwlink/?LinkID=317568."_ 
This is expected. The Microsoft Async Nuget package requires a second build to complete successfully. The VB must be closed and reopened for the second build to complete successfully.      
6. Run the application.  

## Requirements

* ArcGIS API for Silverlight (see [system requirements](https://developers.arcgis.com/en/silverlight/guide/system-requirements.htm))
 - [Download (requires ArcGIS Developer account)](https://developers.arcgis.com/en/silverlight/?download=ArcGISAPI32forSilverlight.exe)
 - [Nuget](http://www.nuget.org/packages/ArcGISSilverlight-All/)
* Blend SDK for Silverlight 5
 - Included with Blend for Visual Studio 2012 and Blend for Visual Studio 2013.  Blend is included with [Visual Studio 2012/2013](http://www.visualstudio.com/)
 - Portion of Blend SDK for Silverlight 5 included with [ArcGIS API for Silverlight - Behaviors package](http://www.nuget.org/packages/ArcGISSilverlight-Behaviors/)
* Silverlight 5 Toolkit
 - [Download on CodePlex](http://silverlight.codeplex.com/)
 - [Nuget (Core and Input)](http://www.nuget.org/packages/SilverlightToolkit-Input/)  
* Microsoft Async (only available via Nuget)
 - [Nuget](http://www.nuget.org/packages/Microsoft.Bcl.Async/)
* Nuget 2.7+ 
 - [Visual Studio extension](http://docs.nuget.org/docs/start-here/installing-nuget)

## Resources

* [ArcGIS API for Silverlight Resource Center](https://developers.arcgis.com/en/silverlight)
* [ArcGIS API for Silverlight download (requires ArcGIS Developer account)](https://developers.arcgis.com/en/silverlight/?download=ArcGISAPI32forSilverlight.exe)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to contribute. 

## Licensing
Copyright 2013 Esri

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

[](Esri Tags: ArcGIS API Silverlight C-Sharp VB C# XAML)
[](Esri Language: DotNet)

