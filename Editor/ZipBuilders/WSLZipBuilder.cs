using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static OSXBuild.Editor.OSXBuildPostProcessor;

namespace OSXBuild.Editor
{
	public class WSLZipBuilder : ZipBuilder
	{
		public WSLZipBuilder(BuildReport report) : base(report) 
		{

		}

		public override void CreateZip(string buildDirectory, string targetZip)
		{
			#region Variables
			bool previousBuildRenamed = false;
			bool oldBuildRecycled = false;

			string buildFolder = Directory.GetParent(buildDirectory).FullName;
			char driveLetter = buildFolder[0];
			
			string buildFolderWsl = buildFolder;
			if (buildFolderWsl[1].Equals(':'))
			{
				buildFolderWsl = buildFolderWsl.Substring(2);
				buildFolderWsl = "/mnt/" + char.ToLower(driveLetter).ToString() + buildFolderWsl;
			}
			buildFolderWsl = buildFolderWsl.Replace("\\", "/");
			buildFolderWsl = buildFolderWsl.Replace(" ", @"\ ");

			string buildName = Path.GetFileName(buildDirectory);
			string productName = Application.productName;
			#endregion

			#region WSL installed check
			VerboseLog("Checking if WSL is installed");

			if (!CheckCommandAvailableErrorContains("wsl", "--list", "--install"))
			{
				throw new Exception("WSL has no installed distributions. For more information, go to https://learn.microsoft.com/windows/wsl/install");
			}
			VerboseLog("WSL installed");
			#endregion

			#region zip installed check
			VerboseLog("Checking if the 'zip' package is installed in WSL");

			if (!CheckCommandAvailableErrorEquals("wsl", "-e which zip", string.Empty))
			{
				throw new Exception("Zip package not installed on WSL. Please make sure you install zip before using this code. To do so, run 'sudo apt install zip' in WSL");
			}
			VerboseLog("Zip package installed");
			#endregion

			#region Zipping process

			if (File.Exists($"{buildDirectory}.zip"))
			{
				previousBuildRenamed = true;

				UnityEngine.Debug.LogWarning($"Previous build found, renaming it to '{buildName}.old.zip");

				if (File.Exists($"{buildDirectory}.old.zip"))
				{
					oldBuildRecycled = true;

					UnityEngine.Debug.LogWarning($"Old zipped build found, moving to recycle bin");

					string randomStr = RandomString(20);
					Directory.CreateDirectory($"Assets/{randomStr}");
					File.Move($"{buildDirectory}.old.zip", $"Assets/{randomStr}/{buildName}.old.zip");
					AssetDatabase.MoveAssetToTrash($"Assets/{randomStr}/{buildName}.old.zip");
					AssetDatabase.DeleteAsset($"Assets/{randomStr}");
				}

				File.Move($"{buildDirectory}.zip", $"{buildDirectory}.old.zip");
			}

            UnityEngine.Debug.Log("Creating zip...");

            var process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "wsl",
					Arguments = $"-e bash -c \"cd {buildFolderWsl} && zip -r {buildName}.zip {buildName}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				},
				EnableRaisingEvents = true,
			};
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.Exited += (sender, e) => Process_Exited(sender, e, buildName, previousBuildRenamed, oldBuildRecycled);
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit(60000);
			#endregion
		}

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                VerboseLog(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                UnityEngine.Debug.LogError(e.Data);
        }
        private void Process_Exited(object sender, EventArgs e, string buildName, bool previousBuildRenamed, bool oldBuildRecycled)
        {
			UnityEngine.Debug.Log("Zip created!");
			if (previousBuildRenamed) UnityEngine.Debug.LogWarning($"Previous build renamed to '{buildName}.old.zip'");
			if (oldBuildRecycled) UnityEngine.Debug.LogWarning($"Old zipped build moved to recycle bin");
        }

        private bool CheckCommandAvailableErrorContains(string commandFileName, string commandArguments, string outputContainsError)
		{
			string output = GetCommandOutput(commandFileName, commandArguments);

			VerboseLog($"Checking output\n{output}");

			if (output.Contains(outputContainsError))
			{
				return false;
			}
			return true;
		}

		private bool CheckCommandAvailableErrorEquals(string commandFileName, string commandArguments, string outputEqualsError)
		{
            string output = GetCommandOutput(commandFileName, commandArguments);

            VerboseLog($"Checking output\n{output}");

            if (output.Equals(outputEqualsError))
            {
                return false;
            }
            return true;
        }

        private string GetCommandOutput(string commandFileName, string commandArguments)
		{
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = commandFileName,
                    Arguments = commandArguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            return output.Replace("\0", string.Empty);
        }

        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        }
    }
}
