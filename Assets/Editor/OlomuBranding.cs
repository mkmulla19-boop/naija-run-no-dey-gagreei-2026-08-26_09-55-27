using UnityEditor;
using UnityEngine;

public static class OlomuBranding
{
    const string Dir = "Assets/Art/Branding";

    [MenuItem("Olomu/Apply Branding")]
    public static void Apply()
    {
        PlayerSettings.companyName = "Mkmulla Game Studio";
        PlayerSettings.productName = "Olomu Survival";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.olomu.survival");

        var t432 = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "/icon_432.png");
        var t192 = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "/icon_192.png");
        var t512 = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "/icon_512.png");

        if (t192 != null)
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { t192 });

        try
        {
            var android = typeof(PlayerSettings).GetProperty("Android") ??
                          typeof(PlayerSettings).GetProperty("androidSettings",
                              System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (android != null)
            {
                var pi = android.GetValue(null);
                SetIfPossible(pi, "adaptiveIconForeground", t432);
                SetIfPossible(pi, "adaptiveIconBackground", null);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Adaptive icon API not set: " + e.Message);
        }

        Debug.Log("BRANDING APPLIED: company=Mkmulla Game Studio icons=" +
                  (t192 != null && t432 != null));
    }

    static void SetIfPossible(object obj, string prop, Texture2D val)
    {
        var p = obj.GetType().GetProperty(prop);
        if (p != null && p.CanWrite) { p.SetValue(obj, val); Debug.Log("Set " + prop); }
        else Debug.Log("Prop missing: " + prop);
    }
}
