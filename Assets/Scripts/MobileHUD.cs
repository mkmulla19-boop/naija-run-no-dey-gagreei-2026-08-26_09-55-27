using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Olomu.Systems
{
    public class MobileHUD : MonoBehaviour
    {
        [Header("Bars")]
        public Image hungerFill;
        public Image thirstFill;

        [Header("Text")]
        public Text inventoryText;
        public Text promptText;
        public Text toastText;

        [Header("Buttons")]
        public Button jumpButton;
        public Button interactButton;
        public Text interactLabel;
        public Button eatButton;
        public Button drinkButton;
        public Button pauseButton;

        [Header("Panels")]
        public GameObject pausePanel;
        public Button resumeButton;
        public Button saveButton;
        public Text statsText;

        private ThirdPersonController player;
        private SurvivalNeeds survival;
        private Inventory inventory;
        private Interactor interactor;
        private SaveLoad saveLoad;
        private float toastTimer;
        private bool dead;

        private void Start()
        {
            player = FindFirstObjectByType<ThirdPersonController>();
            if (player != null)
            {
                survival = player.GetComponent<SurvivalNeeds>();
                inventory = player.GetComponent<Inventory>();
                interactor = player.GetComponent<Interactor>();
            }
            saveLoad = FindFirstObjectByType<SaveLoad>();

            jumpButton.onClick.AddListener(OnJump);
            interactButton.onClick.AddListener(OnInteract);
            eatButton.onClick.AddListener(OnEat);
            drinkButton.onClick.AddListener(OnDrink);
            pauseButton.onClick.AddListener(OnPause);
            resumeButton.onClick.AddListener(OnResume);
            saveButton.onClick.AddListener(OnSave);

            pausePanel.SetActive(false);

            if (survival != null) survival.PlayerDied += OnPlayerDied;
            if (inventory != null) inventory.InventoryChanged += RefreshInventory;
            RefreshInventory();
        }

        private void Update()
        {
            if (survival != null)
            {
                hungerFill.fillAmount = survival.Hunger / survival.maxHunger;
                thirstFill.fillAmount = survival.Thirst / survival.maxThirst;
            }

            if (interactor != null)
            {
                if (interactor.CurrentTarget != null)
                {
                    promptText.text = "Gather " + interactor.CurrentTarget.DisplayName;
                    interactLabel.text = "GATHER";
                    interactButton.interactable = true;
                }
                else
                {
                    promptText.text = "";
                    interactLabel.text = "INTERACT";
                    interactButton.interactable = interactor.CurrentDrinkSpot != null;
                }
            }

            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f) toastText.text = "";
            }
        }

        private void OnJump() => player?.RequestJump();

        private void OnInteract()
        {
            if (interactor == null || inventory == null) return;

            if (interactor.CurrentTarget != null && interactor.TryGather(inventory))
            {
                ShowToast("Gathering " + interactor.CurrentTarget.DisplayName + "...");
                return;
            }

            if (interactor.CurrentDrinkSpot != null && survival != null &&
                interactor.CurrentDrinkSpot.TryDrink(survival))
            {
                ShowToast("You drink from the river.");
            }
        }

        private void OnEat()
        {
            if (inventory == null || survival == null) return;
            if (inventory.RemoveItem("food", 1))
            {
                survival.Eat(30f);
                ShowToast("Ate berries (+30 hunger)");
                RefreshInventory();
            }
            else ShowToast("No food in your bag.");
        }

        private void OnDrink()
        {
            if (interactor != null && interactor.CurrentDrinkSpot != null && survival != null &&
                interactor.CurrentDrinkSpot.TryDrink(survival))
            {
                ShowToast("You drink from the river.");
            }
            else ShowToast("Find water to drink.");
        }

        private void OnPause()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            if (statsText != null && inventory != null)
            {
                var items = inventory.GetAllItems();
                var sb = new System.Text.StringBuilder("Bag:\n");
                foreach (var kv in items) sb.Append("- ").Append(kv.Key).Append(" x").Append(kv.Value).Append('\n');
                statsText.text = sb.ToString();
            }
        }

        private void OnResume()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnSave()
        {
            if (saveLoad != null && player != null)
                saveLoad.Save(player.transform, survival, inventory);
            ShowToast("Game saved.");
        }

        private void OnPlayerDied()
        {
            if (dead) return;
            dead = true;
            ShowToast("You collapsed... The village elder finds you and nurses you back.");
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(3.5f);
            if (player != null) player.Teleport(new Vector3(0f, 1.2f, 6f));
            if (survival != null) survival.Revive();
            dead = false;
        }

        public void RefreshInventory()
        {
            if (inventory == null) return;
            var items = inventory.GetAllItems();
            if (items.Count == 0) { inventoryText.text = ""; return; }
            var sb = new System.Text.StringBuilder();
            foreach (var kv in items) sb.Append(kv.Key).Append(" x").Append(kv.Value).Append('\n');
            inventoryText.text = sb.ToString().TrimEnd();
        }

        public void ShowToast(string msg)
        {
            toastText.text = msg;
            toastTimer = 2.2f;
        }
    }
}
