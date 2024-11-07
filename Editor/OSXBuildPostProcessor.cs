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
				string sourceDir = report.summary.outputPath;
				string zipFileName = sourceDir + ".zip";

				//Delete existing zip if present
				if(File.Exists(zipFileName))
				{
					File.Delete(zipFileName);
				}

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

				//Create zip from the built app directory
				builder.CreateZip(sourceDir, zipFileName);
				if(File.Exists(zipFileName))
				{
					VerboseLog($"OSX Build zip created successfully at {zipFileName}");
				}
				else
				{
					Debug.LogError("OSX build zip creation failed.");
				}

				//Perform actions on the build directory based on project setting
				CleanSourceDirectory(sourceDir);
			}
		}

		private void CleanSourceDirectory(string sourceDir)
		{
			if(OSXBuildSettings.Instance.originalBuildOption == OriginalBuildOption.KeepEmptyDirectory)
			{
				VerboseLog("Clearing original build directory ...");
				//Delete contents of the build directory but keep the directory itself
				foreach(var dir in Directory.GetDirectories(sourceDir))
				{
					Directory.Delete(dir, true);
				}
				foreach(var file in Directory.GetFiles(sourceDir))
				{
					File.Delete(file);
				}
			}
			else if(OSXBuildSettings.Instance.originalBuildOption == OriginalBuildOption.Delete)
			{
				VerboseLog("Deleting original build directory ...");
				Directory.Delete(sourceDir, true);
			}
		}

		private static void VerboseLog(string message)
		{
			if(OSXBuildSettings.Instance.verboseLogging)
			{
				Debug.Log(message);
			}
		}
	}
}
#endif