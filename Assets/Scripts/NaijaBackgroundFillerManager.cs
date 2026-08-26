using UnityEngine;

namespace NaijaRun.Environment
{
    public class NaijaBackgroundFillerManager : MonoBehaviour
    {
        [Header("Track Alignment References")]
        public Transform currentBackgroundChunk;
        public GameObject backgroundFillPrefab;
        public Transform environmentParent;

        [Header("Stage 1 Dimensions & Limits")]
        public float chunkLength = 50f;
        [SerializeField] private float maxStageLength = 200f; // Stage 1 Boundary Lock (Z = 200m)

        public void SpawnNextBackgroundFill()
        {
            if (currentBackgroundChunk == null || backgroundFillPrefab == null)
            {
                Debug.LogWarning("[NaijaBackgroundFillerManager] Missing reference chunk or prefab.");
                return;
            }

            Vector3 nextSpawnPosition = currentBackgroundChunk.position;
            nextSpawnPosition.z += chunkLength;

            // Stage 1 Gate: Strictly stop at the end of the handcrafted Stage 1 boundary
            if (nextSpawnPosition.z > maxStageLength)
            {
                return;
            }

            GameObject newFill = Instantiate(backgroundFillPrefab, nextSpawnPosition, Quaternion.identity);

            if (environmentParent != null)
            {
                newFill.transform.SetParent(environmentParent);
            }

            currentBackgroundChunk = newFill.transform;
        }
    }
}
