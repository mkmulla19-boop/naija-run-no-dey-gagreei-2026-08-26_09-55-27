using System.Linq;
using UnityEditor;
using UnityEngine;

public static class OlomuFbxProbe
{
    public static void Probe()
    {
        const string path = "Assets/Art/Character/olomu_player.fbx";
        var importer = AssetImporter.GetAtPath(path);
        Debug.Log("PROBE importer: " + (importer == null ? "NULL" : importer.GetType().Name));

        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Debug.Log("PROBE load as GO: " + (go == null ? "NULL" : go.name));

        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        Debug.Log("PROBE sub-assets: " + all.Length);
        foreach (var a in all.Take(20))
            Debug.Log("PROBE asset: " + a.GetType().Name + " -> " + a.name);

        if (go != null)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(go);
            Debug.Log("PROBE instantiate: " + (inst == null ? "NULL" : "OK"));
            if (inst != null)
            {
                var renderers = inst.GetComponentsInChildren<Renderer>();
                var anim = inst.GetComponent<Animator>();
                var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToList();
                Debug.Log("PROBE renderers=" + renderers.Length + " animator=" + (anim != null) +
                          " clips=" + clips.Count + " clipNames=" + string.Join(",", clips.Select(c => c.name)));
                Object.DestroyImmediate(inst);
            }
        }
    }
}
