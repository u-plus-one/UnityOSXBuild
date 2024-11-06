#if UNITY_EDITOR_WIN
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;
using UnityEngine;

namespace OSXBuild.Editor
{
	public class OSXBuildPostProcessor : IPostprocessBuildWithReport
	{
		public int callbackOrder => int.MaxValue;

		public void OnPostprocessBuild(BuildReport report)
		{
			if(report.summary.platform == BuildTarget.StandaloneOSX)
			{
				ZipBuilder builder;
				if(OSXBuildSettings.Instance.zipCreationMethod == CompressionMethod.WSL)
				{
					builder = new WSLZipBuilder(report);
				}
				else if(OSXBuildSettings.Instance.zipCreationMethod == CompressionMethod.ZipManipulation)
				{
					builder = new ManualZipBuilder(report);
				}
				else
				{
					throw new System.NotImplementedException();
				}
				VerboseLog($"Using {builder.GetType().Name} to create Zip ...");

				string sourceDir = report.summary.outputPath;
				string zipDir = sourceDir + ".zip";
				builder.CreateZip(sourceDir, zipDir);
				if(File.Exists(zipDir))
				{
					VerboseLog($"OSX Build zip created successfully at {zipDir}");
				}
				else
				{
					Debug.LogError("OSX build zip creation failed.");
				}
			}
		}

		public static void VerboseLog(string message)
		{
			if(OSXBuildSettings.Instance.verboseLogging)
			{
				Debug.Log(message);
			}
		}
	}
}
#endif