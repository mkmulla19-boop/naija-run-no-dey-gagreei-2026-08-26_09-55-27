// BuildAPK.cs – placed in Assets/Editor
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public static class BuildAPK {
    private static readonly string ApkPath = Path.GetFullPath("Builds/OlomuSurvival-latest.apk");
    private const string ScenePath = "Assets/Scenes/OlomuVillage.unity";

    [MenuItem("Build/BuildAndInstall")] // optional menu entry
    public static void BuildAndInstall() {
        // Build the APK
        var buildOptions = new BuildPlayerOptions {
            scenes = new[] { ScenePath },
            locationPathName = ApkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(buildOptions);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) {
            UnityEngine.Debug.LogError($"APK build failed: {report.summary.result}");
            return;
        }
        UnityEngine.Debug.Log($"APK built at {ApkPath}");

        // Locate bundled adb (relative to Unity.exe)
        string unityRoot = Path.GetDirectoryName(EditorApplication.applicationPath);
        string adbPath = Path.GetFullPath(Path.Combine(unityRoot, "../PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe"));
        if (!File.Exists(adbPath)) {
            UnityEngine.Debug.LogError($"adb not found at {adbPath}");
            return;
        }

        // Determine connected device ID
        var getDevices = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = adbPath,
                Arguments = "devices",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        getDevices.Start();
        string devicesOut = getDevices.StandardOutput.ReadToEnd();
        getDevices.WaitForExit();
        string deviceId = null;
        foreach (var line in devicesOut.Split('\n')) {
            if (line.EndsWith("\tdevice")) {
                deviceId = line.Split('\t')[0];
                break;
            }
        }
        if (string.IsNullOrEmpty(deviceId)) {
            UnityEngine.Debug.LogError("No Android device detected.");
            return;
        }

        // Install the APK
        var installProc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = adbPath,
                Arguments = $"-s {deviceId} install -r \"{Path.GetFullPath(ApkPath)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        installProc.Start();
        string outStr = installProc.StandardOutput.ReadToEnd();
        string errStr = installProc.StandardError.ReadToEnd();
        installProc.WaitForExit();
        UnityEngine.Debug.Log(outStr);
        if (!string.IsNullOrEmpty(errStr)) UnityEngine.Debug.LogError(errStr);
    }
}
