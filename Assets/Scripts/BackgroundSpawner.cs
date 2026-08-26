using UnityEngine;
using System.Collections.Generic;

namespace NaijaRun.Environment
{
    public sealed class BackgroundSpawner : MonoBehaviour
    {
        [Header("Player & Track References")]
        public Transform playerTransform;
        public float segmentLength = 30.0f;
        public int segmentsOnScreen = 5;

        [Header("Tier 3 Void Filler Prefabs")]
        public GameObject[] leftBackgroundPrefabs;
        public GameObject[] rightBackgroundPrefabs;
        public GameObject groundExtensionPrefab;

        [Header("Outer Depth Coordinates (Visible Range)")]
        public float leftXPosition = -12.0f;
        public float rightXPosition = 12.0f;

        private float spawnZ = 0.0f;
        private List<GameObject> activeBackgrounds = new List<GameObject>();
        private bool initialized = false;
        private Transform environmentContainer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoAttach()
        {
            if (FindFirstObjectByType<BackgroundSpawner>() == null)
            {
                GameObject trackManager = new GameObject("TrackManager");
                trackManager.AddComponent<BackgroundSpawner>();
                DontDestroyOnLoad(trackManager);
            }
        }

        private void Start()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!initialized)
            {
                TryInitialize();
                return;
            }

            if (playerTransform == null) return;

            if (playerTransform.position.z - segmentLength > spawnZ - (segmentsOnScreen * segmentLength))
            {
                SpawnBackgroundSegment();
                RemoveOldBackground();
            }
        }

        private void TryInitialize()
        {
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.Find("Player_Efe");
                if (playerObj == null) playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }

            if (playerTransform != null && !initialized)
            {
                GameObject env = GameObject.Find("--- ENVIRONMENT ---");
                if (env == null) env = new GameObject("--- ENVIRONMENT ---");
                environmentContainer = env.transform;

                CreateFallbackPrefabs();

                for (int i = 0; i < segmentsOnScreen; i++)
                {
                    SpawnBackgroundSegment();
                }

                initialized = true;
                Debug.Log("[BackgroundSpawner] Visible walls locked at X = +/-12.");
            }
        }

        private void SpawnBackgroundSegment()
        {
            GameObject parentSegment = new GameObject("BackgroundSegment_" + spawnZ);
            if (environmentContainer != null)
                parentSegment.transform.SetParent(environmentContainer);

            if (groundExtensionPrefab != null)
            {
                GameObject ground = Instantiate(groundExtensionPrefab, new Vector3(0f, -0.1f, spawnZ), Quaternion.identity);
                ground.transform.SetParent(parentSegment.transform);
            }

            if (leftBackgroundPrefabs != null && leftBackgroundPrefabs.Length > 0)
            {
                int idx = Random.Range(0, leftBackgroundPrefabs.Length);
                if (leftBackgroundPrefabs[idx] != null)
                {
                    Vector3 leftPos = new Vector3(leftXPosition, 5.0f, spawnZ);
                    GameObject leftObj = Instantiate(leftBackgroundPrefabs[idx], leftPos, Quaternion.identity);
                    leftObj.transform.SetParent(parentSegment.transform);
                }
            }

            if (rightBackgroundPrefabs != null && rightBackgroundPrefabs.Length > 0)
            {
                int idx = Random.Range(0, rightBackgroundPrefabs.Length);
                if (rightBackgroundPrefabs[idx] != null)
                {
                    Vector3 rightPos = new Vector3(rightXPosition, 5.0f, spawnZ);
                    GameObject rightObj = Instantiate(rightBackgroundPrefabs[idx], rightPos, Quaternion.Euler(0, 180, 0));
                    rightObj.transform.SetParent(parentSegment.transform);
                }
            }

            activeBackgrounds.Add(parentSegment);
            spawnZ += segmentLength;
        }

        private void RemoveOldBackground()
        {
            if (activeBackgrounds.Count > 0)
            {
                Destroy(activeBackgrounds[0]);
                activeBackgrounds.RemoveAt(0);
            }
        }

        private void CreateFallbackPrefabs()
        {
            if (leftBackgroundPrefabs == null || leftBackgroundPrefabs.Length == 0)
            {
                leftBackgroundPrefabs = new GameObject[] { CreatePrimitiveTemplate("LeftBlock", new Vector3(8f, 22f, 30f), new Color(0.65f, 0.35f, 0.25f)) };
            }

            if (rightBackgroundPrefabs == null || rightBackgroundPrefabs.Length == 0)
            {
                rightBackgroundPrefabs = new GameObject[] { CreatePrimitiveTemplate("RightBlock", new Vector3(8f, 22f, 30f), new Color(0.65f, 0.35f, 0.25f)) };
            }

            if (groundExtensionPrefab == null)
            {
                groundExtensionPrefab = CreatePrimitiveTemplate("GroundBlock", new Vector3(50f, 0.2f, 30f), new Color(0.20f, 0.18f, 0.15f));
            }
        }

        private GameObject CreatePrimitiveTemplate(string name, Vector3 scale, Color col)
        {
            GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
            template.name = name + "_Template";
            template.transform.localScale = scale;
            if (environmentContainer != null)
                template.transform.SetParent(environmentContainer);

            Collider c = template.GetComponent<Collider>();
            if (c != null) DestroyImmediate(c);

            Renderer r = template.GetComponent<Renderer>();
            if (r != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit");
                if (sh == null) sh = Shader.Find("Standard");
                r.material = new Material(sh);

                if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", col);
                else r.material.color = col;
            }

            template.SetActive(false);
            return template;
        }
    }
}
