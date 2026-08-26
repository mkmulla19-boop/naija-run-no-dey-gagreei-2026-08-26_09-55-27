using UnityEngine;
using NaijaRun.Player;
using NaijaRun.Environment;

namespace NaijaRun.Core
{
    public sealed class Stage1Bootstrap : MonoBehaviour
    {
        private static readonly Color[] LaneColors =
        {
            new Color(0.35f, 0.12f, 0.65f),
            new Color(0.95f, 0.60f, 0.08f),
            new Color(0.08f, 0.55f, 0.70f)
        };

        private static readonly Color SidewalkColor = new Color(0.42f, 0.42f, 0.40f);
        private static readonly Color MarketColor = new Color(0.82f, 0.32f, 0.08f);
        private static readonly Color CanopyColor = new Color(0.95f, 0.65f, 0.08f);
        private static readonly Color VegetationColor = new Color(0.08f, 0.42f, 0.12f);
        private static readonly Color BuildingColor = new Color(0.45f, 0.55f, 0.62f);
        private static readonly Color SignColor = new Color(0.05f, 0.20f, 0.28f);
        private static readonly Color LampColor = new Color(0.08f, 0.08f, 0.08f);
        private static readonly Color DanfoColor = new Color(0.95f, 0.75f, 0.05f);
        private static readonly Color SunColor = new Color(1f, 0.945f, 0.773f);
        private static readonly Color LampLightColor = new Color(1f, 0.69f, 0.259f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateFoundation()
        {
            if (FindFirstObjectByType<Stage1Bootstrap>() != null)
                return;

            var foundation = new GameObject("Stage1_Foundation");
            foundation.AddComponent<Stage1Bootstrap>();
            foundation.AddComponent<AudioManager>();
            BuildScene(foundation.transform);
        }

        private static void BuildScene(Transform root)
        {
            Material[] laneMaterials = new Material[3];
            for (int lane = 0; lane < 3; lane++)
            {
                laneMaterials[lane] = CreateMaterial(LaneColors[lane]);
                CreatePrimitive("Lane_" + lane, PrimitiveType.Cube, new Vector3((lane - 1) * 3f, -0.1f, 100f), new Vector3(3f, 0.2f, 200f), laneMaterials[lane], root);
            }

            BuildSurroundings(root);
            BuildTestObstacles(root);

            Material playerMaterial = CreateMaterial(new Color(0.06f, 0.06f, 0.08f));
            GameObject player = new GameObject("Player_Efe");
            player.tag = "Player";
            player.transform.SetParent(root);
            player.transform.position = Vector3.zero;
            GameObject playerVisual = CreatePrimitive("Player_Visual", PrimitiveType.Capsule, new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 0.9f, 0.7f), playerMaterial, player.transform);
            CapsuleCollider visualCollider = playerVisual.GetComponent<CapsuleCollider>();
            if (visualCollider != null)
                Destroy(visualCollider);
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            player.AddComponent<PlayerController>();

            ItemSpawner itemSpawner = root.gameObject.AddComponent<ItemSpawner>();
            itemSpawner.CreateNairaCoin(new Vector3(0f, 1f, 12f));
            itemSpawner.CreateNaijaFuel(new Vector3(3f, 1f, 24f));
            root.gameObject.AddComponent<Stage1Verification>();

            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("Stage 1 requires an existing Main Camera in the scene.");
                return;
            }

            GameObject cameraObject = camera.gameObject;
            camera.fieldOfView = 60f;
            camera.transform.position = new Vector3(0f, 3.5f, -6f);
            camera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            CameraFollow follower = cameraObject.GetComponent<CameraFollow>() ?? cameraObject.AddComponent<CameraFollow>();
            follower.offset = new Vector3(0f, 3.5f, -6f);
            follower.smoothSpeed = 10f;
            follower.playerTransform = player.transform;

            Light light = FindFirstObjectByType<Light>();
            if (light == null)
            {
                GameObject lightObject = new GameObject("Stage1_Sun");
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.color = SunColor;
            light.intensity = 1.2f;
            light.transform.position = new Vector3(0f, 15f, 0f);
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void BuildSurroundings(Transform root)
        {
            Material sidewalkMaterial = CreateMaterial(SidewalkColor);
            Material marketMaterial = CreateMaterial(MarketColor);
            Material canopyMaterial = CreateMaterial(CanopyColor);
            Material vegetationMaterial = CreateMaterial(VegetationColor);
            Material buildingMaterial = CreateMaterial(BuildingColor);
            Material signMaterial = CreateMaterial(SignColor);
            Material lampMaterial = CreateMaterial(LampColor);
            Material danfoMaterial = CreateMaterial(DanfoColor);

            CreatePrimitive("Sidewalk_Left", PrimitiveType.Cube, new Vector3(-5.5f, 0f, 100f), new Vector3(2f, 0.2f, 200f), sidewalkMaterial, root);
            CreatePrimitive("Sidewalk_Right", PrimitiveType.Cube, new Vector3(5.5f, 0f, 100f), new Vector3(2f, 0.2f, 200f), sidewalkMaterial, root);

            for (int section = 0; section < 7; section++)
            {
                float z = 15f + section * 25f;
                CreateMarketStall("MarketStall_Left_" + section, new Vector3(-6.5f, 1f, z), marketMaterial, canopyMaterial, root);
                CreateMarketStall("MarketStall_Right_" + section, new Vector3(6.5f, 1f, z + 10f), marketMaterial, canopyMaterial, root);
                CreatePalm("Palm_Left_" + section, new Vector3(-10f, 1.5f, z + 7f), vegetationMaterial, root);
                CreatePalm("Palm_Right_" + section, new Vector3(10f, 1.5f, z + 17f), vegetationMaterial, root);
                CreateSign("LagosSign_Left_" + section, new Vector3(-8f, 4.5f, z + 4f), signMaterial, root);
                CreateSign("LagosSign_Right_" + section, new Vector3(8f, 2.5f, z + 14f), signMaterial, root);
                CreateStreetLamp("StreetLamp_Left_" + section, new Vector3(-5f, 0f, z + 5f), lampMaterial, root);
                CreateStreetLamp("StreetLamp_Right_" + section, new Vector3(5f, 0f, z + 15f), lampMaterial, root);
                CreatePrimitive("Building_Left_" + section, PrimitiveType.Cube, new Vector3(-12f, 3f, z + 12f), new Vector3(3f, 6f, 8f), buildingMaterial, root);
                CreatePrimitive("Building_Right_" + section, PrimitiveType.Cube, new Vector3(12f, 3f, z + 20f), new Vector3(3f, 6f, 8f), buildingMaterial, root);
                CreateDanfo("Danfo_Left_" + section, new Vector3(-7f, 0.75f, z + 23f), danfoMaterial, root);
                CreateDanfo("Danfo_Right_" + section, new Vector3(7f, 0.75f, z + 33f), danfoMaterial, root);
            }
        }

        private static void CreateMarketStall(string objectName, Vector3 position, Material stallMaterial, Material canopyMaterial, Transform parent)
        {
            CreatePrimitive(objectName + "_Counter", PrimitiveType.Cube, position, new Vector3(2.5f, 1f, 1.5f), stallMaterial, parent);
            CreatePrimitive(objectName + "_Canopy", PrimitiveType.Cube, position + new Vector3(0f, 1.7f, 0f), new Vector3(3f, 0.15f, 2f), canopyMaterial, parent);
        }

        private static void CreatePalm(string objectName, Vector3 position, Material vegetationMaterial, Transform parent)
        {
            CreatePrimitive(objectName + "_Trunk", PrimitiveType.Cylinder, position, new Vector3(0.25f, 1.5f, 0.25f), vegetationMaterial, parent);
            CreatePrimitive(objectName + "_Crown", PrimitiveType.Sphere, position + new Vector3(0f, 1.8f, 0f), new Vector3(1.4f, 0.6f, 1.4f), vegetationMaterial, parent);
        }

        private static void CreateSign(string objectName, Vector3 position, Material signMaterial, Transform parent)
        {
            CreatePrimitive(objectName + "_Post", PrimitiveType.Cylinder, position, new Vector3(0.08f, 1.8f, 0.08f), signMaterial, parent);
            CreatePrimitive(objectName + "_Board", PrimitiveType.Cube, position + new Vector3(0f, 1.2f, 0f), new Vector3(2f, 0.8f, 0.1f), signMaterial, parent);
        }

        private static void CreateStreetLamp(string objectName, Vector3 position, Material lampMaterial, Transform parent)
        {
            CreatePrimitive(objectName + "_Pole", PrimitiveType.Cylinder, position, new Vector3(0.1f, 2.5f, 0.1f), lampMaterial, parent);
            GameObject head = CreatePrimitive(objectName + "_Light", PrimitiveType.Sphere, position + new Vector3(0f, 2.2f, 0f), Vector3.one * 0.25f, lampMaterial, parent);
            Light pointLight = head.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = LampLightColor;
            pointLight.intensity = 2f;
            pointLight.range = 8f;
        }

        private static void CreateDanfo(string objectName, Vector3 position, Material vehicleMaterial, Transform parent)
        {
            CreatePrimitive(objectName + "_Body", PrimitiveType.Cube, position, new Vector3(2.2f, 1.5f, 4.5f), vehicleMaterial, parent);
            CreatePrimitive(objectName + "_Roof", PrimitiveType.Cube, position + new Vector3(0f, 0.9f, 0f), new Vector3(2.3f, 0.15f, 4.6f), vehicleMaterial, parent);
        }

        private static void BuildTestObstacles(Transform root)
        {
            Material crateMaterial = CreateMaterial(new Color(0.55f, 0.28f, 0.08f));
            Material barrierMaterial = CreateMaterial(new Color(0.9f, 0.12f, 0.04f));

            CreatePrimitive("TestCrate_CenterLane", PrimitiveType.Cube, new Vector3(0f, 0.75f, 42f), Vector3.one * 1.5f, crateMaterial, root);
            CreatePrimitive("TestBarricade_RightLane", PrimitiveType.Cube, new Vector3(3f, 0.6f, 66f), new Vector3(2.2f, 1.2f, 0.35f), barrierMaterial, root);
        }

        private static GameObject CreatePrimitive(string objectName, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject created = GameObject.CreatePrimitive(type);
            created.name = objectName;
            created.transform.SetParent(parent);
            created.transform.position = position;
            created.transform.localScale = scale;
            created.GetComponent<Renderer>().sharedMaterial = material;
            return created;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }
    }
}