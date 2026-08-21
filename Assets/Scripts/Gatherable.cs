using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Olomu.Systems
{
    public class Gatherable : MonoBehaviour
    {
        public string itemName = "wood";
        public int amount = 1;
        public float gatherTime = 0.8f;
        public bool destroyOnGather = true;
        public float respawnSeconds = 45f;

        public string DisplayName
        {
            get
            {
                switch (itemName)
                {
                    case "wood": return "Wood";
                    case "stone": return "Stone";
                    case "food": return "Berries";
                    default: return itemName;
                }
            }
        }

        public bool IsAvailable { get; private set; } = true;

        private static readonly List<Gatherable> all = new List<Gatherable>();
        private Coroutine routine;

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);

        public static Gatherable FindNearest(Vector3 pos, float maxDist)
        {
            Gatherable best = null;
            float bestSqr = maxDist * maxDist;
            for (int i = 0; i < all.Count; i++)
            {
                Gatherable g = all[i];
                if (!g.IsAvailable) continue;
                float d = (g.transform.position - pos).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = g; }
            }
            return best;
        }

        public bool TryGather(Inventory inventory, ThirdPersonController player)
        {
            if (!IsAvailable || routine != null) return false;
            if (player != null) player.PlayAttack();
            routine = StartCoroutine(GatherRoutine(inventory));
            return true;
        }

        private IEnumerator GatherRoutine(Inventory inventory)
        {
            yield return new WaitForSeconds(gatherTime);
            if (inventory != null) inventory.AddItem(itemName, amount);
            Debug.Log("Gathered " + amount + " " + itemName);

            if (destroyOnGather)
            {
                IsAvailable = false;
                SetVisible(false);
                StartCoroutine(RespawnRoutine());
            }
            routine = null;
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnSeconds);
            IsAvailable = true;
            SetVisible(true);
        }

        private void SetVisible(bool on)
        {
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = on;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = on;
        }
    }
}
