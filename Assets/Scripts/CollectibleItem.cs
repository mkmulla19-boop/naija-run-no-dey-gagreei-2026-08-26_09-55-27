using UnityEngine;

namespace NaijaRun.Environment
{
    public enum CollectibleType
    {
        NairaCoin,
        BoostItem,
        NaijaFuel
    }

    public sealed class CollectibleItem : MonoBehaviour
    {
        public CollectibleType type;
        public float rotationSpeed = 90f;

        private void Update()
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "CollectionEffect";
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector3.one * 0.3f;
            Destroy(effect.GetComponent<Collider>());
            Destroy(effect, 0.2f);
            Destroy(gameObject);
        }
    }
}