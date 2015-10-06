## Google Maps Server-side Clustering with C# ##

THIS PROJECT HAS MOVED TO
https://github.com/kunukn/Google-Maps-Clustering-CSharp


This is a server-side clustering of markers on Google Maps which can handle 100.000+ markers with reasonable performance.
This is implemented in VS 2010 and webforms.

  * Click the image for a demonstration of how it works.
  * Read the [FAQ](http://code.google.com/p/google-maps-server-side-clustering-dotnet/wiki/FAQ) to get started using this application.
  * Read the [Version history](http://code.google.com/p/google-maps-server-side-clustering-dotnet/wiki/Version) for the feature list.

[![](http://google-maps-server-side-clustering-dotnet.googlecode.com/svn/wiki/images/clustering.png)](http://jory.dk/AreaGMC/)



### Features ###
  * Server-side clustering
  * K-nearest neighbors - (Haversine formula)
  * No database requirement, data are loaded from a csv file
  * Plain html


### External Minimum Dependencies ###
  * jQuery
  * Google Maps `JavaScript` API v3


### Ajax examples ###
  * `Markers` http://jory.dk/AreaGMC/gmc.svc/GetMarkers/nelat=-14;nelon=85_1;swlat=-28;swlon=50;zoom=6
  * `MarkerInfo` http://jory.dk/AreaGMC/gmc.svc/GetMarkerInfo/id=1545752
  * `Nearest neighbors` http://jory.dk/AreaGMC/gmc.svc/Knn/lat=-20_3;lon=57_6;k=3


Latest version is in SVN