using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildScript
{
    static readonly StringBuilder errorLog = new StringBuilder();

    static void Capture(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            errorLog.AppendLine("[" + type + "] " + condition);
            if (!string.IsNullOrEmpty(stackTrace)) errorLog.AppendLine(stackTrace);
        }
    }

    public static void BuildAndroidTest()
    {
        Application.logMessageReceived += Capture;

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "OlomuTestCube";
        cube.transform.position = new Vector3(0f, 1f, 0f);
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");

        PlayerSettings.companyName = "Olomu";
        PlayerSettings.productName = "Olomu Survival";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.olomu.survival");
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Boot.unity" },
            locationPathName = "Builds/OlomuSurvival-test.apk",
            target = BuildTarget.Android,
            options = BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Application.logMessageReceived -= Capture;

        File.WriteAllText(@"C:\ProgramData\olomu-build-errors.txt", errorLog.ToString());

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("BUILD SUCCEEDED: " + report.summary.outputPath + " (" + report.summary.totalSize + " bytes)");
        }
        else
        {
            Debug.LogError("BUILD FAILED: " + report.summary.result + " errors: " + report.summary.totalErrors);
            EditorApplication.Exit(1);
        }
    }
}
