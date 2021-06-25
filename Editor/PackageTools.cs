using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

// https://docs.unity3d.com/Manual/upm-api.html
namespace DeepDreamGames
{
	static public class PackageTools
	{
		#region Public Methods
		[MenuItem("Tools/Deep Dream Games/Pack Embedded Package(s)...")]
		static public void SelectAndPack()
		{
			EditorCoroutines.StartCoroutine(SelectAndPackAsync(true));
		}

		/// <summary>
		/// Usage: [MenuItem("Tools/Pack Package")] static public void DoPack() { DeepDreamGames.PackageTools.Pack("com.deepdreamgames.package-tools"); }
		/// </summary>
		static public void Pack(string name, bool openFolder = true)
		{
			EditorCoroutines.StartCoroutine(PackAsync(name, openFolder));
		}
        #endregion

        #region Private Methods
        // 
        static private IEnumerator SelectAndPackAsync(bool openFolder)
        {
            ListRequest listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
                yield return null;
            }
            if (IsFailed(listRequest))
            {
                yield break;
            }

            List<string> names = new List<string>();
            List<PackageInfo> embedded = new List<PackageInfo>();
            PackageCollection list = listRequest.Result;
            using (var e = (list as IEnumerable<PackageInfo>).GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var p = e.Current;
                    if (p.source == PackageSource.Embedded)
                    {
                        embedded.Add(p);
                        names.Add(string.Format("{0} {1} ({2})", p.displayName, p.version, p.name));
                    }
                }
            }

            List<PackageInfo> toPack = new List<PackageInfo>();
            if (embedded.Count > 1)
            {
                List<int> result = new List<int>();
                SelectionWindow window = SelectionWindow.Open("Select package(s) to pack", names, result);
                for (int i = 0; i < result.Count; i++)
                {
                    int index = result[i];
                    toPack.Add(embedded[index]);
                }
            }
            // Don't ask if there's only one embedded package
            else if (embedded.Count == 1)
            {
                toPack.Add(embedded[0]);
            }
            else
            {
                Report(LogType.Error, "No embedded packages found to pack! You should put your package to '{0}'", Path.Combine(Directory.GetCurrentDirectory(), "Packages").Replace('\\', '/'));
            }

            for (int i = 0; i < toPack.Count; i++)
            {
                PackageInfo package = toPack[i];
                yield return PackAsync(package, openFolder);
            }
        }

		// 
		static private IEnumerator PackAsync(string name, bool openFolder)
		{
			ListRequest listRequest = Client.List(true);
			while (!listRequest.IsCompleted)
			{
				yield return null;
			}
			if (IsFailed(listRequest))
			{
				yield break;
			}

            // Find package
            PackageInfo packageInfo = null;
			PackageCollection list = listRequest.Result;
			using (var e = (list as IEnumerable<PackageInfo>).GetEnumerator())
			{
				while (e.MoveNext())
				{
					var p = e.Current;
					if (p.name == name)
					{
						packageInfo = p;
						break;
					}
				}
			}

			if (packageInfo == null)
			{
				Report(LogType.Error, "Package '{0}' is not found!", name);
				yield break;
			}

			yield return PackAsync(packageInfo, openFolder);
		}

		// 
		static private IEnumerator PackAsync(PackageInfo packageInfo, bool openFolder)
		{
			if (packageInfo == null)
			{
				Report(LogType.Error, "PackageInfo cannot be null!");
				yield break;
			}

            string fileName = string.Format("{0}-{1}.tgz", packageInfo.name, packageInfo.version);

            // Select directory
            string targeteFolder = EditorUtility.SaveFolderPanel(string.Format("Save {0} To", fileName), Path.GetFullPath("Releases"), string.Empty);
			if (string.IsNullOrEmpty(targeteFolder))
			{
				yield break;
			}
			if (!Directory.Exists(targeteFolder))
			{
				Report(LogType.Warning, "Directory at path '{0}' does not exist!", targeteFolder);
				yield break;
			}

			string fullPath = Path.Combine(targeteFolder, fileName).Replace('\\', '/');
			if (File.Exists(fullPath))
			{
				bool overwrite = EditorUtility.DisplayDialog("Overwrite", string.Format("File '{0}' exists. Overwrite?", fileName), "Yes", "No");
				if (!overwrite)
				{
					yield break;
				}
			}

			PackRequest packRequest = Client.Pack(packageInfo.assetPath, targeteFolder);   // packageInfo.resolvedPath
			while (!packRequest.IsCompleted)
			{
				yield return null;
			}
			if (IsFailed(packRequest))
			{
				yield break;
			}

			PackOperationResult pack = packRequest.Result;
			if (pack != null && openFolder)
			{
				EditorUtility.RevealInFinder(pack.tarballPath);
			}
		}

        // 
        static private bool IsFailed(Request request)
		{
			if (request.Status != StatusCode.Failure) { return false; }

			Error error = request.Error;
			Report(LogType.Error, string.Format("PackageManager request failed. Error code: {0}\n{1}", error.errorCode, error.message));
			return true;
		}

		//
		static private void Report(LogType logType, string format, params object[] args)
		{
			string message = args == null || args.Length == 0 ? format : string.Format(format, args);
			Debug.unityLogger.Log(logType, message);
			EditorUtility.DisplayDialog(logType.ToString(), message, "OK");
		}
		#endregion
	}
}