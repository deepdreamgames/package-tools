# com.deepdreamgames.package-tools

Wrapper for Unity's `UnityEditor.PackageManager.Client.Pack()` which allows to list and select embedded packages to pack. 


# Pack using GUI

Select `Tools` > `Deep Dream Games` > `Pack Embedded Package(s)...`


# Pack from code

Call `DeepDreamGames.PackageTools.Pack("com.deepdreamgames.packagetools");` where `com.deepdreamgames.packagetools` is the `name` of your package as seen in `package.json`.

