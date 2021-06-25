# com.deepdreamgames.package-tools

Wrapper for Unity's `UnityEditor.PackageManager.Client.Pack()` which allows to pack embedded packages into `.tgz` archives.


# Installation

`Window` > `Package Manager` > `+` > `Add package from git URL...` paste `https://github.com/deepdreamgames/package-tools.git` and press `Add` button. 

Alternatively - you can download latest [release](https://github.com/deepdreamgames/package-tools/releases/latest) and add it to Unity by using [Pachka](https://github.com/deepdreamgames/pachka) package registry server. 


# Usage

* Pack using GUI: Select `Tools` > `Deep Dream Games` > `Pack Embedded Package(s)...`
* Pack from code: Call `DeepDreamGames.PackageTools.Pack("com.deepdreamgames.packagetools");` where `com.deepdreamgames.packagetools` is the `name` of your package as seen in `package.json`.
