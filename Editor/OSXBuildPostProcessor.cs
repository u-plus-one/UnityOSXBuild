#if UNITY_EDITOR_WIN
using System;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;
using System.IO.Compression;
using System.IO;
using UnityEngine;

namespace OSXBuild.Editor
{
	public class OSXBuildPostProcessor : IPostprocessBuildWithReport
	{
		//							         _______rwxrwxrwx________________
		const uint UNIX_FLAGS_EXECUTABLE = 0b10000001111111110000000000000000;
		const uint UNIX_FLAGS_DEFAULT = 0b10000001101101100000000000000000;
		//						             \------/\------/\------/\------/

		public int callbackOrder => int.MaxValue;

		public void OnPostprocessBuild(BuildReport report)
		{
			if(report.summary.platform == BuildTarget.StandaloneOSX)
			{
				CreateUnixZIP(report);
			}
		}

		private void CreateUnixZIP(BuildReport report)
		{
			var rootPath = report.summary.outputPath;
			var buildName = Path.GetFileName(rootPath);
			var executableFilePath = $"{buildName}/Contents/MacOS/{PlayerSettings.productName}";

			//Delete existing zip if present
			if(File.Exists(rootPath + ".zip"))
			{
				File.Delete(rootPath + ".zip");
			}

			//Compress executable into a zip file
			ZipFile.CreateFromDirectory(rootPath, rootPath + ".zip", System.IO.Compression.CompressionLevel.NoCompression, true);

			int entryCount;
			//Modify zip to set the executable attributes
			using(var zip = ZipFile.Open(rootPath + ".zip", ZipArchiveMode.Update))
			{
				entryCount = zip.Entries.Count;
				//Set standard unix flags for all files and executable flags for the executable
				foreach(var entry in zip.Entries)
				{
					bool executable = entry.FullName == executableFilePath;
					SetUnixFlags(entry, executable ? UNIX_FLAGS_EXECUTABLE : UNIX_FLAGS_DEFAULT);
				}
			}

			//Manual trickery to pretend that the zip was created on unix
			SetHostOS(rootPath, entryCount);

			//Self test
			CheckExecutableFlags(rootPath, executableFilePath);

			//Delete original build directory
			DeleteBuildDirectory(rootPath);
		}

		private static void SetHostOS(string rootPath, int entryCount)
		{
			const byte P = (byte)'P';
			const byte K = (byte)'K';
			//Minimum version needed to extract: 0x14 (20) = 2.0
			const byte V = 0x14;
			var bytes = File.ReadAllBytes(rootPath + ".zip");
			int modifiedEntries = 0;
			for(int i = 0; i < bytes.Length; i++)
			{
				if(i + 6 >= bytes.Length) break;
				if(bytes[i] == P && bytes[i + 1] == K && bytes[i + 2] == 0x01 && bytes[i + 3] == 0x02 && bytes[i + 4] == V)
				{
					bytes[i + 5] = 0x03;
					modifiedEntries++;
					//Approximate minimum central directory entry length
					i += 48;
				}
			}
			if(entryCount != modifiedEntries)
			{
				Debug.LogError($"Number of modified entries does not match actual entry count (expected: {entryCount}, modified: {modifiedEntries})");
			}
			else
			{
				Debug.Log($"Zip Host OS changed successfully ({modifiedEntries} entries modified)");
			}
			File.WriteAllBytes(rootPath + ".zip", bytes);
		}

		private static void SetUnixFlags(ZipArchiveEntry entry, uint flags)
		{
			unchecked
			{
				int i = (int)flags;
				entry.ExternalAttributes |= i;
			}
		}

		private static void CheckExecutableFlags(string rootPath, string executableFilePath)
		{
			using var zip = ZipFile.OpenRead(rootPath + ".zip");
			var attributes = zip.GetEntry(executableFilePath).ExternalAttributes;
			bool test;
			unchecked
			{
				test = ((uint)attributes & UNIX_FLAGS_EXECUTABLE) == UNIX_FLAGS_EXECUTABLE;
			}
			string base2 = Convert.ToString(zip.GetEntry(executableFilePath).ExternalAttributes, 2).PadLeft(32, '0');
			if(!test) Debug.LogError("Unix perms test failed: " + base2);
		}

		private static void DeleteBuildDirectory(string rootPath)
		{
			//Delete contents of the build directory but keep the directory itself
			foreach(var dir in Directory.GetDirectories(rootPath))
			{
				Directory.Delete(dir, true);
			}
			foreach(var file in Directory.GetFiles(rootPath))
			{
				File.Delete(file);
			}
		}
	}
}
#endif