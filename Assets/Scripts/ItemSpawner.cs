using UnityEngine;

namespace NaijaRun.Environment
{
    public sealed class ItemSpawner : MonoBehaviour
    {
        private static readonly Color CoinColor = new Color(1f, 0.84f, 0f);
        private static readonly Color FuelColor = new Color(0f, 0.7f, 0.1f);

        public GameObject CreateNairaCoin(Vector3 position)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "NairaCoin_Rough";
            coin.transform.SetParent(transform);
            coin.transform.position = position;
            coin.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ConfigureCollectible(coin, CoinColor, CollectibleType.NairaCoin);
            return coin;
        }

        public GameObject CreateNaijaFuel(Vector3 position)
        {
            GameObject fuel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fuel.name = "NaijaFuel_Rough";
            fuel.transform.SetParent(transform);
            fuel.transform.position = position;
            fuel.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
            ConfigureCollectible(fuel, FuelColor, CollectibleType.NaijaFuel);
            return fuel;
        }

        private static void ConfigureCollectible(GameObject item, Color color, CollectibleType type)
        {
            item.GetComponent<Renderer>().material.color = color;
            item.GetComponent<Collider>().isTrigger = true;
            CollectibleItem collectible = item.AddComponent<CollectibleItem>();
            collectible.type = type;
        }
    }
}