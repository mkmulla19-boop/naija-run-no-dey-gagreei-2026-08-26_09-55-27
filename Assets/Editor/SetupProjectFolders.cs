using System.IO;
using UnityEditor;
using UnityEngine;

public static class SetupProjectFolders
{
    [MenuItem("Tools/NaijaRun/Generate Folder Structure")]
    public static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Prefabs",
            "Assets/Materials",
            "Assets/Textures",
            "Assets/Models",
            "Assets/Audio",
            "Assets/Resources/Audio",
            "Assets/Environment/Road",
            "Assets/Environment/Market",
            "Assets/Environment/Buildings",
            "Assets/Environment/Vegetation",
            "Assets/Environment/Obstacles",
            "Assets/Environment/Collectibles"
        };

        foreach (string folder in folders)
            Directory.CreateDirectory(folder);

        AssetDatabase.Refresh();
        Debug.Log("NaijaRun folder structure successfully created.");
    }
}