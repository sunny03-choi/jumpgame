using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class WebGLBuilder
{
    [MenuItem("Build/Build WebGL")]
    public static void Build()
    {
        string buildPath = "game/jumpgameweb";
        
        // Ensure the directory exists
        if (!System.IO.Directory.Exists(buildPath))
        {
            System.IO.Directory.CreateDirectory(buildPath);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenes();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    private static string[] GetScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        string[] scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }
        return scenePaths;
    }
}
