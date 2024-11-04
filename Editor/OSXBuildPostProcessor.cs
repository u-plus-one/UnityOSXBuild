using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;

namespace OSXBuild.Editor
{
	public class MacBuildPostProcessor : IPostprocessBuildWithReport
	{
		public int callbackOrder => int.MaxValue;

		public void OnPostprocessBuild(BuildReport report)
		{
			if(report.summary.platform == BuildTarget.StandaloneOSX)
			{
				
			}
		}
	}
}
