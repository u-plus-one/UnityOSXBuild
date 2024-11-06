using UnityEditor.Build.Reporting;

namespace OSXBuild.Editor
{
	public class ManualZipBuilder : ZipBuilder
	{
		public ManualZipBuilder(BuildReport report) : base(report) 
		{

		}

		public override void CreateZip(string buildDirectory, string targetZip)
		{
			throw new System.NotImplementedException();
		}
	} 
}
