using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

public static class BuildProject {
    [MenuItem("ProjecteKahoot/Build Windows x64")]
    public static void Windows() {
        PlayerSettings.productName="ProjecteKahoot";
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        Directory.CreateDirectory("Builds/Windows");
        var report=BuildPipeline.BuildPlayer(scenes,"Builds/Windows/ProjecteKahoot.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Build: "+report.summary.result);
        UnityEngine.Debug.Log("ProjecteKahoot: Builds/Windows");
    }
}
