using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Olomu.Systems;

public static class OlomuSceneBuilder
{
    const string ScenePath = "Assets/Scenes/OlomuVillage.unity";
    const string FbxPath = "Assets/Art/Character/olomu_player_male.fbx";
    const string CirclePng = "Assets/Art/UI/circle.png";
    const string RoundRectPng = "Assets/Art/UI/roundrect.png";
    const string ControllerPath = "Assets/Art/Character/OlomuController.controller";

    static Font font;

    [MenuItem("Olomu/Build Village Scene")]
    public static void BuildVillageScene()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Art/UI");
        EnsureTexture(CirclePng, MakeCircleTexture(128));
        EnsureTexture(RoundRectPng, MakeRoundedRectTexture(128));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var sunGo = new GameObject("Sun");
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sunGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        sun.color = new Color(1f, 0.886f, 0.69f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.52f, 0.55f, 0.5f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.0055f;
        RenderSettings.fogColor = new Color(0.78f, 0.82f, 0.72f);

        BuildGround();
        BuildRiver();
        BuildPaths();
        BuildVillage();
        BuildWilderness();
        var player = BuildPlayer();
        var storyActors = BuildStoryActors();
        var father = storyActors.Item1;
        var raiders = storyActors.Item2;
        BuildNPCs();
        BuildEnemies();
        var audio = BuildAudio();
        var hud = BuildHUD(player);

        var cineCamGo = new GameObject("CineCamera");
        var cineCam = cineCamGo.AddComponent<Camera>();
        cineCam.fieldOfView = 55f;
        cineCam.farClipPlane = 500f;
        cineCamGo.tag = "Untagged";
        cineCam.depth = 10f;

        var focusGo = new GameObject("VillageFocus");
        focusGo.transform.position = new Vector3(0f, 2.5f, 0f);

        var fireT = GameObject.Find("Campfire").transform;

        var playerCtl = player.GetComponent<ThirdPersonController>();
        var gameCam = playerCtl.GetComponentInChildren<Camera>(true);
        if (gameCam != null) gameCam.gameObject.SetActive(false);
        playerCtl.joystick = hud.GetComponent<VirtualJoystick>();

        var director = BuildCinematicUI(cineCam, player, playerCtl,
            hud.GetComponent<MobileHUD>(), audio,
            father, raiders, focusGo.transform, fireT);

        var saveLoadGo = new GameObject("SaveLoad");
        saveLoadGo.AddComponent<SaveLoad>();

        var gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();

        var promoGo = new GameObject("PromoCapture");
        promoGo.AddComponent<PromoCapture>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("OLOMU SCENE BUILT: " + ScenePath);
    }

    static void EnsureTexture(string path, Texture2D tex)
    {
        if (File.Exists(path)) return;
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
    }

    static Texture2D MakeCircleTexture(int size)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d);
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        t.Apply();
        return t;
    }

    static Texture2D MakeRoundedRectTexture(int size)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float rad = size * 0.22f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = Mathf.Clamp(x + 0.5f, rad, size - rad);
                float py = Mathf.Clamp(y + 0.5f, rad, size - rad);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(px, py));
                float a = Mathf.Clamp01(rad - d);
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        t.Apply();
        return t;
    }

    static Material Mat(Color c, float smoothness = 0.05f)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        m.SetFloat("_Glossiness", smoothness);
        return m;
    }

    static GameObject Prim(PrimitiveType type, string name, Material mat, Vector3 pos, Vector3 scale, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static void BuildGround()
    {
        Prim(PrimitiveType.Plane, "Ground", Mat(new Color(0x6B / 255f, 0x5A / 255f, 0x3E / 255f)), new Vector3(0, 0, 0), new Vector3(50, 1, 50));
        Prim(PrimitiveType.Plane, "ClearingDirt", Mat(new Color(0x8A / 255f, 0x73 / 255f, 0x4F / 255f)), new Vector3(0, 0.02f, 0), new Vector3(4.4f, 1, 4.4f));
    }

    static void BuildRiver()
    {
        Prim(PrimitiveType.Plane, "River", Mat(new Color(0.16f, 0.42f, 0.55f), 0.85f), new Vector3(-30f, 0.06f, 0), new Vector3(2.2f, 1, 60f));
        Prim(PrimitiveType.Plane, "RiverBank", Mat(new Color(0.58f, 0.47f, 0.31f)), new Vector3(-24.2f, 0.03f, 0), new Vector3(1.2f, 1, 60f));

        for (int i = 0; i < 5; i++)
        {
            var spot = new GameObject("DrinkSpot" + i);
            spot.transform.position = new Vector3(-24.8f, 0.1f, -40f + i * 20f);
            spot.AddComponent<DrinkSpot>();
            Prim(PrimitiveType.Cylinder, "WaterMarker", Mat(new Color(0.3f, 0.75f, 0.9f), 0.9f), spot.transform.position + Vector3.up * 0.02f, new Vector3(0.9f, 0.01f, 0.9f), spot.transform);
        }
    }

    static void BuildPaths()
    {
        Prim(PrimitiveType.Plane, "PathToRiver", Mat(new Color(0.66f, 0.53f, 0.34f)), new Vector3(-13f, 0.04f, 2f), new Vector3(3.4f, 1, 0.28f));
        Prim(PrimitiveType.Plane, "PathSouth", Mat(new Color(0.66f, 0.53f, 0.34f)), new Vector3(2f, 0.04f, -14f), new Vector3(0.24f, 1, 3.2f));
        Prim(PrimitiveType.Plane, "PathNorth", Mat(new Color(0.66f, 0.53f, 0.34f)), new Vector3(-4f, 0.04f, 14f), new Vector3(0.22f, 1, 3.0f));
    }

    static GameObject LoadWorldPrefab(string rel)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/World/" + rel);
    }

    static void BuildHut(Vector3 pos, float rotDeg)
    {
        var prefab = LoadWorldPrefab("hut.fbx");
        if (prefab != null)
        {
            var hut = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            hut.name = "Hut";
            hut.transform.position = pos;
            hut.transform.rotation = Quaternion.Euler(0, rotDeg, 0);
            float s = Random.Range(0.92f, 1.15f);
            hut.transform.localScale = new Vector3(s, s, s);
            return;
        }
        var fallback = new GameObject("Hut");
        fallback.transform.position = pos;
        Prim(PrimitiveType.Cylinder, "Wall", Mat(new Color(0.55f, 0.41f, 0.26f)), pos + Vector3.up * 1.1f, new Vector3(4.4f, 1.1f, 4.4f), fallback.transform);
    }

    static void BuildVillage()
    {
        Random.InitState(1234);
        var hutSpots = new[]
        {
            new Vector3(11f, 0, 3f), new Vector3(9f, 0, -9f), new Vector3(-4f, 0, 12f),
            new Vector3(-9f, 0, -7f), new Vector3(14f, 0, -3f), new Vector3(-2f, 0, -13f),
            new Vector3(-14f, 0, 8f)
        };
        for (int i = 0; i < hutSpots.Length; i++)
            BuildHut(hutSpots[i], Random.Range(0f, 360f));

        var fire = new GameObject("Campfire");
        fire.transform.position = new Vector3(1.5f, 0, 1.5f);
        Prim(PrimitiveType.Cylinder, "Stones", Mat(new Color(0.45f, 0.43f, 0.4f)), fire.transform.position + Vector3.up * 0.08f, new Vector3(1.4f, 0.08f, 1.4f), fire.transform);
        Prim(PrimitiveType.Cylinder, "Logs", Mat(new Color(0.4f, 0.27f, 0.14f)), fire.transform.position + Vector3.up * 0.16f, new Vector3(0.7f, 0.08f, 0.7f), fire.transform);
        var flame = Prim(PrimitiveType.Sphere, "Flame", Mat(new Color(1f, 0.55f, 0.1f)), fire.transform.position + Vector3.up * 0.45f, new Vector3(0.5f, 0.8f, 0.5f), fire.transform);
        flame.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        AddWoodPile(new Vector3(4.5f, 0, 5.5f));
        AddWoodPile(new Vector3(-6.5f, 0, 4f));
        AddStonePile(new Vector3(6f, 0, -5f));
        AddStonePile(new Vector3(-5f, 0, -10.5f));
    }

    static void AddWoodPile(Vector3 pos)
    {
        var pile = new GameObject("WoodPile");
        pile.transform.position = pos;
        var mat = Mat(new Color(0.44f, 0.3f, 0.16f));
        Prim(PrimitiveType.Cylinder, "Log1", mat, pos + new Vector3(0, 0.18f, 0), new Vector3(0.32f, 1.1f, 0.32f), pile.transform).transform.rotation = Quaternion.Euler(90, 0, 0);
        Prim(PrimitiveType.Cylinder, "Log2", mat, pos + new Vector3(0.12f, 0.5f, 0), new Vector3(0.32f, 1.1f, 0.32f), pile.transform).transform.rotation = Quaternion.Euler(90, 25, 0);
        Prim(PrimitiveType.Cylinder, "Log3", mat, pos + new Vector3(-0.1f, 0.5f, 0.05f), new Vector3(0.32f, 1.1f, 0.32f), pile.transform).transform.rotation = Quaternion.Euler(90, -20, 0);

        var col = pile.AddComponent<BoxCollider>();
        col.size = new Vector3(1.4f, 1.2f, 1.4f);
        col.center = new Vector3(0, 0.5f, 0);
        var g = pile.AddComponent<Gatherable>();
        g.itemName = "wood";
        g.amount = 3;
    }

    static void AddStonePile(Vector3 pos)
    {
        var pile = new GameObject("StonePile");
        pile.transform.position = pos;
        var mat = Mat(new Color(0.49f, 0.48f, 0.45f));
        Prim(PrimitiveType.Sphere, "S1", mat, pos + new Vector3(0, 0.25f, 0), new Vector3(0.7f, 0.5f, 0.7f), pile.transform);
        Prim(PrimitiveType.Sphere, "S2", mat, pos + new Vector3(0.45f, 0.2f, 0.2f), new Vector3(0.5f, 0.38f, 0.5f), pile.transform);
        Prim(PrimitiveType.Sphere, "S3", mat, pos + new Vector3(-0.4f, 0.18f, -0.15f), new Vector3(0.45f, 0.34f, 0.45f), pile.transform);

        var col = pile.AddComponent<SphereCollider>();
        col.radius = 0.75f;
        col.center = new Vector3(0, 0.3f, 0);
        var g = pile.AddComponent<Gatherable>();
        g.itemName = "stone";
        g.amount = 2;
    }

    static void BuildPalm(Vector3 pos)
    {
        var prefab = LoadWorldPrefab("palm.fbx");
        GameObject tree;
        float h;
        if (prefab != null)
        {
            tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            tree.name = "Palm";
            tree.transform.position = pos;
            h = 6.5f;
        }
        else
        {
            tree = new GameObject("Palm");
            tree.transform.position = pos;
            h = Random.Range(3.2f, 4.6f);
            var trunk = Prim(PrimitiveType.Cylinder, "Trunk", Mat(new Color(0.42f, 0.3f, 0.17f)), pos + Vector3.up * h / 2f, new Vector3(0.32f, h / 2f, 0.32f), tree.transform);
            trunk.transform.rotation = Quaternion.Euler(Random.Range(-6f, 6f), 0, Random.Range(-6f, 6f));
        }
        var col = tree.AddComponent<CapsuleCollider>();
        col.radius = 0.3f;
        col.height = h;
        col.center = new Vector3(0, h / 2f, 0);
        var g = tree.AddComponent<Gatherable>();
        g.itemName = "wood";
        g.amount = 2;
        g.gatherTime = 1.1f;
    }

    static void BuildBush(Vector3 pos)
    {
        var bush = new GameObject("BerryBush");
        bush.transform.position = pos;
        Prim(PrimitiveType.Sphere, "Leaves", Mat(new Color(0.2f, 0.42f, 0.19f)), pos + Vector3.up * 0.45f, new Vector3(1.3f, 0.95f, 1.3f), bush.transform);
        var berryMat = Mat(new Color(0.85f, 0.15f, 0.15f));
        for (int i = 0; i < 4; i++)
        {
            Vector2 r = Random.insideUnitCircle * 0.45f;
            Prim(PrimitiveType.Sphere, "Berry", berryMat, pos + new Vector3(r.x, 0.55f + Random.Range(-0.15f, 0.2f), r.y), new Vector3(0.14f, 0.14f, 0.14f), bush.transform);
        }
        var col = bush.AddComponent<SphereCollider>();
        col.radius = 0.8f;
        col.center = new Vector3(0, 0.45f, 0);
        var g = bush.AddComponent<Gatherable>();
        g.itemName = "food";
        g.amount = 2;
        g.gatherTime = 0.7f;
    }

    static void BuildWilderness()
    {
        int placed = 0, guard = 0;
        while (placed < 55 && guard++ < 800)
        {
            Vector2 r = Random.insideUnitCircle * 70f;
            Vector3 p = new Vector3(r.x, 0, r.y);
            if (p.magnitude < 16f) continue;
            if (p.x < -21f && p.x > -39f) continue;
            BuildPalm(p);
            placed++;
        }
        placed = 0; guard = 0;
        while (placed < 18 && guard++ < 400)
        {
            Vector2 r = Random.insideUnitCircle * 55f;
            Vector3 p = new Vector3(r.x, 0, r.y);
            if (p.magnitude < 14f) continue;
            if (p.x < -21f && p.x > -39f) continue;
            BuildBush(p);
            placed++;
        }
        placed = 0; guard = 0;
        var rockPrefab = LoadWorldPrefab("rock.fbx");
        while (placed < 22 && guard++ < 400)
        {
            Vector2 r = Random.insideUnitCircle * 65f;
            Vector3 p = new Vector3(r.x, 0, r.y);
            if (p.magnitude < 15f) continue;
            if (p.x < -21f && p.x > -39f) continue;
            if (rockPrefab != null)
            {
                var rock = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                rock.name = "Rock";
                rock.transform.position = p;
                float s = Random.Range(0.5f, 1.6f);
                rock.transform.localScale = new Vector3(s, s * Random.Range(0.6f, 1.1f), s);
                rock.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
            else
            {
                Prim(PrimitiveType.Sphere, "Rock", Mat(new Color(0.47f, 0.46f, 0.43f)), p + Vector3.up * 0.2f,
                    new Vector3(Random.Range(0.5f, 1.6f), Random.Range(0.35f, 0.9f), Random.Range(0.5f, 1.6f)));
            }
            placed++;
        }
    }

    static AnimatorController BuildAnimator()
    {
        var clips = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__")).ToList();

        AnimationClip idle = clips.FirstOrDefault(c => c.name.ToLower().Contains("idle"));
        AnimationClip walk = clips.FirstOrDefault(c => c.name.ToLower().Contains("walk"));
        AnimationClip run = clips.FirstOrDefault(c => c.name.ToLower().Contains("run"));
        AnimationClip jump = clips.FirstOrDefault(c => c.name.ToLower().Contains("jump"));
        AnimationClip attack = clips.FirstOrDefault(c => c.name.ToLower().Contains("attack"));

        Debug.Log("OLOMU CLIPS: idle=" + (idle != null) + " walk=" + (walk != null) +
                  " run=" + (run != null) + " jump=" + (jump != null) + " attack=" + (attack != null));

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var root = controller.layers[0].stateMachine;
        controller.AddParameter("AnimState", AnimatorControllerParameterType.Int);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        AnimatorState MakeState(AnimationClip clip, string name, Vector3 pos)
        {
            var s = root.AddState(name, pos);
            if (clip != null) s.motion = clip;
            return s;
        }

        var idleS = MakeState(idle, "Idle", new Vector3(0, 0, 0));
        var walkS = MakeState(walk, "Walk", new Vector3(220, 0, 0));
        var runS = MakeState(run, "Run", new Vector3(440, 0, 0));
        var jumpS = MakeState(jump, "Jump", new Vector3(220, 160, 0));
        var atkS = MakeState(attack, "Attack", new Vector3(220, -160, 0));
        root.defaultState = idleS;

        void Link(AnimatorState from, AnimatorState to, bool exitTime, float exitT,
            AnimatorConditionMode mode = AnimatorConditionMode.If, float threshold = 0f, string param = null)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = exitTime;
            t.exitTime = exitT;
            t.duration = 0.12f;
            t.hasFixedDuration = true;
            t.offset = 0f;
            t.interruptionSource = TransitionInterruptionSource.None;
            if (param != null) t.AddCondition(mode, threshold, param);
        }

        Link(idleS, walkS, false, 0f, AnimatorConditionMode.Greater, 0f, "AnimState");
        Link(walkS, idleS, false, 0f, AnimatorConditionMode.Less, 1f, "AnimState");
        Link(walkS, runS, false, 0f, AnimatorConditionMode.Greater, 1f, "AnimState");
        Link(runS, walkS, false, 0f, AnimatorConditionMode.Less, 2f, "AnimState");

        Link(idleS, jumpS, false, 0f, AnimatorConditionMode.Equals, 3f, "AnimState");
        Link(walkS, jumpS, false, 0f, AnimatorConditionMode.Equals, 3f, "AnimState");
        Link(runS, jumpS, false, 0f, AnimatorConditionMode.Equals, 3f, "AnimState");
        Link(jumpS, idleS, false, 0f, AnimatorConditionMode.Less, 3f, "AnimState");

        Link(idleS, atkS, false, 0f, AnimatorConditionMode.If, 0f, "Attack");
        Link(walkS, atkS, false, 0f, AnimatorConditionMode.If, 0f, "Attack");
        Link(runS, atkS, false, 0f, AnimatorConditionMode.If, 0f, "Attack");
        Link(atkS, idleS, true, 0.85f);

        return controller;
    }

    static GameObject BuildPlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.05f, 8f);

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.7f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0, 0.85f, 0);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.4f;

        player.AddComponent<ThirdPersonController>();
        player.AddComponent<SurvivalNeeds>();
        player.AddComponent<Inventory>();
        player.AddComponent<Interactor>();
        player.AddComponent<Health>();

        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbx != null)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            model.name = "Model";
            model.transform.SetParent(player.transform, false);
            model.transform.localRotation = Quaternion.Euler(0, 180f, 0);

            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                float height = Mathf.Max(b.size.y, 0.01f);
                float scale = 1.65f / height;
                model.transform.localScale = Vector3.one * scale;

                b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                model.transform.localPosition += new Vector3(0f, player.transform.position.y - b.min.y, 0f);
            }

            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            var avatar = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Avatar>().FirstOrDefault();
            if (avatar != null) animator.avatar = avatar;
            animator.runtimeAnimatorController = BuildAnimator();
            animator.applyRootMotion = false;
        }
        else
        {
            Debug.LogError("FBX missing at " + FbxPath);
        }

        return player;
    }

    static void BuildNPCs()
    {
        var spots = new[]
        {
            new Vector3(5f, 1.0f, 2f), new Vector3(-6f, 1.0f, -3f), new Vector3(2f, 1.0f, -8f),
            new Vector3(-10f, 1.0f, 6f)
        };
        var colors = new[]
        {
            new Color(0.78f, 0.6f, 0.42f), new Color(0.5f, 0.55f, 0.7f),
            new Color(0.72f, 0.5f, 0.35f), new Color(0.6f, 0.65f, 0.45f)
        };
        for (int i = 0; i < spots.Length; i++)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = "Villager" + i;
            npc.transform.position = spots[i];
            npc.transform.localScale = new Vector3(0.8f, 1.05f, 0.8f);
            npc.GetComponent<Renderer>().sharedMaterial = Mat(colors[i]);
            npc.AddComponent<CharacterController>();
            npc.GetComponent<CharacterController>().height = 2f;
            npc.AddComponent<SimpleNPC>();
        }
    }

    static System.Tuple<CineActor, CineActor[]> BuildStoryActors()
    {
        var fatherGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fatherGo.name = "Father";
        fatherGo.transform.position = new Vector3(-4f, 1.05f, 11.5f);
        fatherGo.transform.localScale = new Vector3(0.85f, 1.1f, 0.85f);
        fatherGo.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.35f, 0.24f, 0.15f));
        var fcc = fatherGo.AddComponent<CharacterController>();
        fcc.height = 2.1f;
        var father = fatherGo.AddComponent<CineActor>();
        father.speed = 2.4f;
        father.waypoints = new[]
        {
            new Vector3(-2.5f, 1.05f, 10f),
            new Vector3(0.8f, 1.05f, 9.2f)
        };

        var raiders = new CineActor[5];
        Vector3[][] paths =
        {
            new[] { new Vector3(-14f, 1.05f, -12f), new Vector3(2f, 1.05f, 2f), new Vector3(16f, 1.05f, 10f) },
            new[] { new Vector3(16f, 1.05f, -10f), new Vector3(0f, 1.05f, -1f), new Vector3(-14f, 1.05f, 9f) },
            new[] { new Vector3(-13f, 1.05f, 6f), new Vector3(3f, 1.05f, 4f), new Vector3(15f, 1.05f, -6f) },
            new[] { new Vector3(13f, 1.05f, 7f), new Vector3(-2f, 1.05f, 3f), new Vector3(-16f, 1.05f, -8f) },
            new[] { new Vector3(4f, 1.05f, -14f), new Vector3(1f, 1.05f, 1f), new Vector3(-12f, 1.05f, 12f) }
        };
        var raidColors = new[]
        {
            new Color(0.25f, 0.18f, 0.18f), new Color(0.3f, 0.22f, 0.15f),
            new Color(0.2f, 0.2f, 0.22f), new Color(0.28f, 0.18f, 0.2f),
            new Color(0.22f, 0.25f, 0.18f)
        };
        for (int i = 0; i < raiders.Length; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Raider" + i;
            go.transform.position = paths[i][0];
            go.transform.localScale = new Vector3(0.85f, 1.08f, 0.85f);
            go.GetComponent<Renderer>().sharedMaterial = Mat(raidColors[i]);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.1f;
            var actor = go.AddComponent<CineActor>();
            actor.speed = 4.6f;
            actor.waypoints = new[] { paths[i][1], paths[i][2] };
            var ai = go.AddComponent<EnemyAI>();
            ai.enabled = false;
            ai.health = 100f;
            ai.attackDamage = 12f;
            ai.attackCooldown = 1.15f;
            ai.chaseSpeed = 5.9f;
            ai.detectRadius = 16f;
            ai.patrolRadius = 6f;
            ai.lootItem = "hide";
            raiders[i] = actor;
        }

        return new System.Tuple<CineActor, CineActor[]>(father, raiders);
    }

    static void BuildEnemies()
    {
        var raidMats = new[]
        {
            new Color(0.42f, 0.2f, 0.18f), new Color(0.36f, 0.24f, 0.14f), new Color(0.3f, 0.26f, 0.22f)
        };
        Vector3[] raidSpots =
        {
            new Vector3(-6f, 1.05f, -38f),
            new Vector3(8.5f, 1.05f, -44f),
            new Vector3(15f, 1.05f, -30f)
        };
        for (int i = 0; i < raidSpots.Length; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "RaiderPatrol" + i;
            go.transform.position = raidSpots[i];
            go.transform.localScale = new Vector3(0.85f, 1.1f, 0.85f);
            go.GetComponent<Renderer>().sharedMaterial = Mat(raidMats[i]);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.1f;
            var ai = go.AddComponent<EnemyAI>();
            ai.health = 100f;
            ai.attackDamage = 12f;
            ai.chaseSpeed = 5.9f;
            ai.detectRadius = 10f;
            ai.patrolRadius = 11f;
            ai.lootItem = "hide";
        }

        var dogMat = Mat(new Color(0.23f, 0.19f, 0.16f));
        Vector3[] dogSpots =
        {
            new Vector3(34f, 1.02f, 26f),
            new Vector3(-40f, 1.02f, -20f),
            new Vector3(28f, 1.02f, -46f),
            new Vector3(-32f, 1.02f, 38f)
        };
        foreach (var spot in dogSpots)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "WildDog";
            go.transform.position = spot;
            go.transform.localScale = new Vector3(0.55f, 0.5f, 0.95f);
            go.GetComponent<Renderer>().sharedMaterial = dogMat;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.1f;
            cc.radius = 0.35f;
            var ai = go.AddComponent<EnemyAI>();
            ai.health = 55f;
            ai.attackDamage = 9f;
            ai.attackCooldown = 0.85f;
            ai.chaseSpeed = 7.1f;
            ai.walkSpeed = 2.4f;
            ai.detectRadius = 12f;
            ai.patrolRadius = 14f;
            ai.lootItem = "meat";
            ai.lootAmount = 2;
        }
    }

    static AudioDirector BuildAudio()
    {
        var go = new GameObject("AudioDirector");
        var ad = go.AddComponent<AudioDirector>();
        ad.music = go.AddComponent<AudioSource>();
        ad.ambience = go.AddComponent<AudioSource>();
        ad.sfx = go.AddComponent<AudioSource>();
        ad.bed = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/olomu_bed.wav");
        ad.tensionHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/tension_hit.wav");
        ad.morningAmbience = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/morning_ambience.wav");
        return ad;
    }

    static CinematicDirector BuildCinematicUI(Camera cineCam, GameObject player,
        ThirdPersonController playerCtl, MobileHUD hud, AudioDirector audio,
        CineActor father, CineActor[] raiders, Transform focus, Transform campfire)
    {
        Sprite rrect = LoadSprite(RoundRectPng);

        var blackGo = new GameObject("BlackOverlay");
        blackGo.transform.SetParent(hud.transform, false);
        var blackRect = blackGo.AddComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;
        var blackImg = blackGo.AddComponent<Image>();
        blackImg.color = Color.black;
        blackImg.raycastTarget = false;

        var letterTopGo = new GameObject("LetterTop");
        letterTopGo.transform.SetParent(hud.transform, false);
        var ltRect = letterTopGo.AddComponent<RectTransform>();
        ltRect.anchorMin = new Vector2(0, 1);
        ltRect.anchorMax = new Vector2(1, 1);
        ltRect.pivot = new Vector2(0.5f, 1f);
        var ltImg = letterTopGo.AddComponent<Image>();
        ltImg.color = Color.black;
        ltImg.raycastTarget = false;

        var letterBotGo = new GameObject("LetterBot");
        letterBotGo.transform.SetParent(hud.transform, false);
        var lbRect = letterBotGo.AddComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(0, 0);
        lbRect.anchorMax = new Vector2(1, 0);
        lbRect.pivot = new Vector2(0.5f, 0f);
        var lbImg = letterBotGo.AddComponent<Image>();
        lbImg.color = Color.black;
        lbImg.raycastTarget = false;

        var titleStudio = MakeText(blackGo.transform, "TitleStudio", "", 30,
            new Color(0.85f, 0.78f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-600, -40), new Vector2(600, 20), TextAnchor.LowerCenter);
        var titleMain = MakeText(blackGo.transform, "TitleMain", "", 120,
            new Color(0.95f, 0.78f, 0.32f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-500, -220), new Vector2(500, 60), TextAnchor.UpperCenter);
        titleMain.fontStyle = FontStyle.Bold;

        var subtitle = MakeText(hud.transform, "Subtitle", "", 36, Color.white,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-700, 210), new Vector2(700, 280), TextAnchor.LowerCenter);
        subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
        subtitle.fontStyle = FontStyle.Italic;

        var skipBtn = MakeButton(hud.transform, "SkipButton", rrect, new Color(0, 0, 0, 0.55f),
            new Vector2(0.5f, 0), new Vector2(320, 96), "SKIP  >", 34, out _);
        skipBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

        var director = hud.gameObject.AddComponent<CinematicDirector>();
        director.cineCam = cineCam;
        director.villageFocus = focus;
        director.campfire = campfire;
        director.playerT = player.transform;
        director.playerController = playerCtl;
        director.father = father;
        director.raiders = raiders;
        director.titleStudio = titleStudio;
        director.titleMain = titleMain;
        director.subtitle = subtitle;
        director.blackOverlay = blackImg;
        director.letterTop = ltRect;
        director.letterBot = lbRect;
        director.skipButton = skipBtn;
        director.audioDirector = audio;
        director.hud = hud;

        hud.cinematicHidden = new GameObject[]
        {
            skipBtn.gameObject
        };

        return director;
    }

    static Text MakeText(Transform parent, string name, string content, int size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offMin, Vector2 offMax, TextAnchor align = TextAnchor.MiddleLeft)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offMin;
        rect.offsetMax = offMax;
        var t = go.AddComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Image MakeImage(Transform parent, string name, Sprite sprite, Color color,
        Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    static Button MakeButton(Transform parent, string name, Sprite sprite, Color color,
        Vector2 anchor, Vector2 size, string label, int fontSize, out Text labelText)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        labelText = null;
        if (!string.IsNullOrEmpty(label))
        {
            var tgo = new GameObject("Label");
            tgo.transform.SetParent(go.transform, false);
            var trect = tgo.AddComponent<RectTransform>();
            trect.anchorMin = Vector2.zero;
            trect.anchorMax = Vector2.one;
            trect.offsetMin = Vector2.zero;
            trect.offsetMax = Vector2.zero;
            var t = tgo.AddComponent<Text>();
            t.font = font;
            t.text = label;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            labelText = t;
        }
        return btn;
    }

    static GameObject BuildHUD(GameObject player)
    {
        var canvasGo = new GameObject("HUD");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Sprite circle = LoadSprite(CirclePng);
        Sprite rrect = LoadSprite(RoundRectPng);

        var baseImg = MakeImage(canvasGo.transform, "JoystickBase", circle, new Color(1, 1, 1, 0.14f), new Vector2(0.5f, 0.5f), new Vector2(260, 260));
        var knobImg = MakeImage(canvasGo.transform, "JoystickKnob", circle, new Color(1, 1, 1, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(104, 104));
        var joystick = canvasGo.AddComponent<VirtualJoystick>();
        joystick.baseCircle = baseImg.rectTransform;
        joystick.knob = knobImg.rectTransform;
        joystick.radius = 130f;

        var healthBg = MakeImage(canvasGo.transform, "HealthBg", rrect, new Color(0, 0, 0, 0.55f), new Vector2(0, 1), new Vector2(380, 40));
        healthBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(210, -134);
        var healthFill = MakeImage(canvasGo.transform, "HealthFill", rrect, new Color(0.85f, 0.18f, 0.15f), new Vector2(0, 1), new Vector2(360, 28));
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.rectTransform.SetParent(healthBg.transform, false);
        healthFill.rectTransform.anchorMin = new Vector2(0, 0.5f);
        healthFill.rectTransform.anchorMax = new Vector2(0, 0.5f);
        healthFill.rectTransform.pivot = new Vector2(0, 0.5f);
        healthFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
        healthFill.rectTransform.sizeDelta = new Vector2(360, 28);
        healthFill.fillAmount = 1f;
        MakeText(healthBg.transform, "HealthLabel", "HEALTH", 20, Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-12, 0), TextAnchor.MiddleLeft);

        var hungerBg = MakeImage(canvasGo.transform, "HungerBg", rrect, new Color(0, 0, 0, 0.55f), new Vector2(0, 1), new Vector2(380, 40));
        hungerBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(210, -34);
        var hungerFill = MakeImage(canvasGo.transform, "HungerFill", rrect, new Color(0.95f, 0.55f, 0.15f), new Vector2(0, 1), new Vector2(360, 28));
        hungerFill.type = Image.Type.Filled;
        hungerFill.fillMethod = Image.FillMethod.Horizontal;
        hungerFill.rectTransform.SetParent(hungerBg.transform, false);
        hungerFill.rectTransform.anchorMin = new Vector2(0, 0.5f);
        hungerFill.rectTransform.anchorMax = new Vector2(0, 0.5f);
        hungerFill.rectTransform.pivot = new Vector2(0, 0.5f);
        hungerFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
        hungerFill.rectTransform.sizeDelta = new Vector2(360, 28);
        hungerFill.fillAmount = 1f;
        MakeText(hungerBg.transform, "HungerLabel", "FOOD", 20, Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-12, 0), TextAnchor.MiddleLeft);

        var thirstBg = MakeImage(canvasGo.transform, "ThirstBg", rrect, new Color(0, 0, 0, 0.55f), new Vector2(0, 1), new Vector2(380, 40));
        thirstBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(210, -84);
        var thirstFill = MakeImage(canvasGo.transform, "ThirstFill", rrect, new Color(0.2f, 0.7f, 0.95f), new Vector2(0, 1), new Vector2(360, 28));
        thirstFill.type = Image.Type.Filled;
        thirstFill.fillMethod = Image.FillMethod.Horizontal;
        thirstFill.rectTransform.SetParent(thirstBg.transform, false);
        thirstFill.rectTransform.anchorMin = new Vector2(0, 0.5f);
        thirstFill.rectTransform.anchorMax = new Vector2(0, 0.5f);
        thirstFill.rectTransform.pivot = new Vector2(0, 0.5f);
        thirstFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
        thirstFill.rectTransform.sizeDelta = new Vector2(360, 28);
        thirstFill.fillAmount = 1f;
        MakeText(thirstBg.transform, "ThirstLabel", "WATER", 20, Color.white, new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-12, 0), TextAnchor.MiddleLeft);

        var invText = MakeText(canvasGo.transform, "InventoryText", "", 26, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -140), new Vector2(430, -240), TextAnchor.UpperLeft);
        invText.horizontalOverflow = HorizontalWrapMode.Wrap;

        var toastText = MakeText(canvasGo.transform, "Toast", "", 30, new Color(1f, 0.95f, 0.7f), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-500, -150), new Vector2(500, -110), TextAnchor.UpperCenter);

        var promptText = MakeText(canvasGo.transform, "Prompt", "", 32, Color.white, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-600, 320), new Vector2(600, 370), TextAnchor.LowerCenter);

        var interactBtn = MakeButton(canvasGo.transform, "InteractButton", rrect, new Color(0.85f, 0.45f, 0.1f, 0.92f), new Vector2(1, 0), new Vector2(170, 170), "GATHER", 30, out var interactLabel);
        interactBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, 190);
        var jumpBtn = MakeButton(canvasGo.transform, "JumpButton", rrect, new Color(0.25f, 0.5f, 0.85f, 0.92f), new Vector2(1, 0), new Vector2(130, 130), "JUMP", 26, out _);
        jumpBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300, 130);
        var eatBtn = MakeButton(canvasGo.transform, "EatButton", rrect, new Color(0.75f, 0.25f, 0.2f, 0.92f), new Vector2(1, 0), new Vector2(96, 96), "EAT", 24, out _);
        eatBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, 390);
        var drinkBtn = MakeButton(canvasGo.transform, "DrinkButton", rrect, new Color(0.2f, 0.55f, 0.8f, 0.92f), new Vector2(1, 0), new Vector2(96, 96), "DRINK", 22, out _);
        drinkBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, 420);
        var pauseBtn = MakeButton(canvasGo.transform, "PauseButton", rrect, new Color(0.2f, 0.2f, 0.2f, 0.85f), new Vector2(1, 1), new Vector2(80, 80), "II", 30, out _);
        pauseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-60, -60);

        var pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasGo.transform, false);
        var ppRect = pausePanel.AddComponent<RectTransform>();
        ppRect.anchorMin = Vector2.zero;
        ppRect.anchorMax = Vector2.one;
        ppRect.offsetMin = Vector2.zero;
        ppRect.offsetMax = Vector2.zero;
        var ppImg = pausePanel.AddComponent<Image>();
        ppImg.color = new Color(0, 0, 0, 0.82f);
        MakeText(pausePanel.transform, "Title", "PAUSED", 64, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300, 180), new Vector2(300, 260), TextAnchor.MiddleCenter);
        var statsText = MakeText(pausePanel.transform, "Stats", "", 30, new Color(0.9f, 0.9f, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350, 20), new Vector2(350, 160), TextAnchor.UpperCenter);
        var resumeBtn = MakeButton(pausePanel.transform, "ResumeButton", rrect, new Color(0.2f, 0.6f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(340, 84), "RESUME", 32, out _);
        resumeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
        var saveBtn = MakeButton(pausePanel.transform, "SaveButton", rrect, new Color(0.25f, 0.45f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(340, 84), "SAVE GAME", 32, out _);
        saveBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -230);

        var hud = canvasGo.AddComponent<MobileHUD>();
        hud.hungerFill = hungerFill;
        hud.thirstFill = thirstFill;
        hud.healthFill = healthFill;
        hud.inventoryText = invText;
        hud.promptText = promptText;
        hud.toastText = toastText;
        hud.jumpButton = jumpBtn;
        hud.interactButton = interactBtn;
        hud.interactLabel = interactLabel;
        hud.eatButton = eatBtn;
        hud.drinkButton = drinkBtn;
        hud.pauseButton = pauseBtn;
        hud.pausePanel = pausePanel;
        hud.resumeButton = resumeBtn;
        hud.saveButton = saveBtn;
        hud.statsText = statsText;

        return canvasGo;
    }
}
