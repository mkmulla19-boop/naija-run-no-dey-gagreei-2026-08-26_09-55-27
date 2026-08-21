using System;
using System.Collections;
using UnityEngine;

namespace Olomu.Systems
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public ThirdPersonController Player { get; private set; }
        public SurvivalNeeds Survival { get; private set; }
        public Inventory Inventory { get; private set; }
        public SaveLoad SaveSystem { get; private set; }

        public float autosaveInterval = 30f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            Player = FindFirstObjectByType<ThirdPersonController>();
            SaveSystem = FindFirstObjectByType<SaveLoad>();

            if (Player != null)
            {
                Survival = Player.GetComponent<SurvivalNeeds>();
                Inventory = Player.GetComponent<Inventory>();
            }

            bool loaded = false;
            if (SaveSystem != null && Player != null && Survival != null && Inventory != null)
                loaded = SaveSystem.Load(Player.transform, Survival, Inventory);

            var opening = FindFirstObjectByType<OpeningSequence>();
            if (opening != null) opening.SequenceFinished += () => StartCoroutine(AutosaveLoop());
            else StartCoroutine(AutosaveLoop());

            Debug.Log(loaded ? "Olomu: save loaded." : "Olomu: new game.");
        }

        private IEnumerator AutosaveLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(autosaveInterval);
                if (Player != null && Survival != null && Inventory != null && Survival.IsAlive)
                    SaveSystem?.Save(Player.transform, Survival, Inventory);
            }
        }

        public void SaveGame()
        {
            if (Player != null && Survival != null && Inventory != null)
                SaveSystem?.Save(Player.transform, Survival, Inventory);
        }
    }
}
