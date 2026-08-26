using UnityEngine;
using NaijaRun.Environment;

namespace NaijaRun.Core
{
    public sealed class Stage1Verification : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== RUNNING STAGE 1 ACCEPTANCE VERIFICATION ===");

            Camera[] cameras = Camera.allCameras;
            if (cameras.Length == 1 && cameras[0].CompareTag("MainCamera"))
                Debug.Log("[PASS] Only one Main Camera exists.");
            else
                Debug.LogError($"[FAIL] Found {cameras.Length} cameras. Only 1 allowed.");

            if (Application.targetFrameRate == 60)
                Debug.Log("[PASS] Frame rate target set to 60 FPS.");
            else
                Debug.LogWarning($"[WARN] Target frame rate is set to {Application.targetFrameRate}, expected 60.");

            AudioClip voicePreview = Resources.Load<AudioClip>("Audio/NaijaRun_voice_preview");
            if (voicePreview != null)
                Debug.Log("[PASS] Approved preview audio located.");
            else
                Debug.LogError("[FAIL] Approved preview audio missing from Resources/Audio.");

            GameObject player = GameObject.Find("Player_Efe");
            if (player == null)
            {
                Debug.LogError("[FAIL] Player_Efe object missing from scene.");
                return;
            }

            if (player.CompareTag("Player"))
                Debug.Log("[PASS] Player_Efe tag verified.");
            else
                Debug.LogError("[FAIL] Player_Efe must use the Player tag.");

            if (Mathf.Approximately(player.transform.localScale.y, 1f))
                Debug.Log("[PASS] Player_Efe root scale verified.");
            else
                Debug.LogError("[FAIL] Player_Efe root scale is not 1.");

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null && Mathf.Approximately(controller.height, 1.8f) && Mathf.Approximately(controller.radius, 0.35f))
                Debug.Log("[PASS] Player collider dimensions verified.");
            else
                Debug.LogError("[FAIL] Player collider dimensions are incorrect.");

            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<CameraFollow>() != null)
                Debug.Log("[PASS] CameraFollow is attached to the existing Main Camera.");
            else
                Debug.LogError("[FAIL] CameraFollow is missing from Main Camera.");
        }
    }
}