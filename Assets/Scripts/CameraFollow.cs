using UnityEngine;

namespace NaijaRun.Environment
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Target to Follow")]
        public Transform playerTransform;

        [Header("Camera Distance Offset")]
        public Vector3 offset = new Vector3(0f, 3.5f, -6f);

        [Header("Follow Smoothness")]
        public float smoothSpeed = 10f;

        private void LateUpdate()
        {
            if (playerTransform == null)
                return;

            float smoothX = Mathf.Lerp(transform.position.x, playerTransform.position.x + offset.x, smoothSpeed * Time.deltaTime);
            transform.position = new Vector3(smoothX, offset.y, playerTransform.position.z + offset.z);
        }
    }
}