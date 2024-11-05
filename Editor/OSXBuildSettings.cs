using System.IO;
using UnityEditor;
using UnityEngine;

namespace OSXBuild.Editor
{
	public enum CompressionMethod
	{
		WSL = 0,
		ZipManipulation = 1
	}

	public enum CompressionLevel
	{
		None = 0,
		Fastest = 1,
		Optimal = 2
	}

	public enum OriginalBuildOption
	{
		KeepOriginal = 0,
		Delete = 1,
		KeepEmptyDirectory = 2
	}

    public class OSXBuildSettings : ScriptableObject
    {
		public CompressionMethod zipCreationMethod = CompressionMethod.WSL;
		public CompressionLevel zipCompressionLevel = CompressionLevel.Optimal;
		public OriginalBuildOption originalBuildOption = OriginalBuildOption.KeepOriginal;
		public bool verboseLogging = false;

		public static OSXBuildSettings Instance
		{
			get
			{
				if(instance == null) Initialize();
				return instance;
			}
		}

		private static OSXBuildSettings instance;

	    private static string ProjectAssetPath => Path.Combine("ProjectSettings", "OSXBuildPreferences.asset");

	    private static void Initialize()
	    {
#if UNITY_EDITOR
		    if(!File.Exists(ProjectAssetPath))
		    {
			    CreateNewSettings();
			    Save();
		    }
		    else
		    {
			    Load();
		    }
#endif
	    }

		private static void CreateNewSettings()
		{
			instance = CreateInstance<OSXBuildSettings>();
		}

		public static void Save()
		{
			var json = EditorJsonUtility.ToJson(Instance, true);
			File.WriteAllText(ProjectAssetPath, json);
		}

		private static void Load()
		{
			var json = File.ReadAllText(ProjectAssetPath);
			instance = CreateInstance<OSXBuildSettings>();
			EditorJsonUtility.FromJsonOverwrite(json, instance);
		}
    }
}