using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Olomu.Systems
{
    [Serializable]
    public class SaveData
    {
        public float posX, posY, posZ;
        public float hunger, thirst;
        public List<string> itemNames = new List<string>();
        public List<int> itemCounts = new List<int>();
        public string savedAt = "";
    }

    public class SaveLoad : MonoBehaviour
    {
        public string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "olomu_save.json");

        public void Save(Transform player, SurvivalNeeds survival, Inventory inventory)
        {
            SaveData data = new SaveData
            {
                posX = player.position.x,
                posY = player.position.y,
                posZ = player.position.z,
                hunger = survival.Hunger,
                thirst = survival.Thirst,
                savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            foreach (var kv in inventory.GetAllItems())
            {
                data.itemNames.Add(kv.Key);
                data.itemCounts.Add(kv.Value);
            }

            try
            {
                System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
                Debug.Log("Game saved: " + SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Save failed: " + e.Message);
            }
        }

        public bool Load(Transform player, SurvivalNeeds survival, Inventory inventory)
        {
            if (!System.IO.File.Exists(SavePath)) return false;

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(SavePath));
                var tp = player.GetComponent<ThirdPersonController>();
                tp?.Teleport(new Vector3(data.posX, data.posY + 0.1f, data.posZ));

                var dict = new Dictionary<string, int>();
                for (int i = 0; i < data.itemNames.Count && i < data.itemCounts.Count; i++)
                    dict[data.itemNames[i]] = data.itemCounts[i];

                inventory.RestoreAll(dict);
                survival.Restore(data.hunger, data.thirst);
                Debug.Log("Game loaded.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Load failed: " + e.Message);
                return false;
            }
        }
    }
}
