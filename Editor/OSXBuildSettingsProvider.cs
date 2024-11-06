using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OSXBuild.Editor
{
	public static class OSXBuildSettingsProvider
	{
		private static SettingsProvider provider;

		[SettingsProvider]
		internal static SettingsProvider Register()
		{
			provider = new SettingsProvider("Project/OSX Zip Options", SettingsScope.Project)
			{
				guiHandler = OnGUI,
				deactivateHandler = OnClose
			};
			return provider;
		}

		private static void OnGUI(string search)
		{
			var width = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 200;
			var so = new SerializedObject(OSXBuildSettings.Instance);
			so.Update();
			EditorGUILayout.PropertyField(so.FindProperty(nameof(OSXBuildSettings.zipCreationMethod)));
			EditorGUILayout.PropertyField(so.FindProperty(nameof(OSXBuildSettings.zipCompressionLevel)));
			EditorGUILayout.PropertyField(so.FindProperty(nameof(OSXBuildSettings.originalBuildOption)));
			GUILayout.Space(20);
			EditorGUILayout.PropertyField(so.FindProperty(nameof(OSXBuildSettings.verboseLogging)));
			if(so.ApplyModifiedProperties())
			{
				OSXBuildSettings.Save();
			}
			EditorGUIUtility.labelWidth = width;
		}

		private static void OnClose()
		{
			OSXBuildSettings.Save();
		}
	} 
}
