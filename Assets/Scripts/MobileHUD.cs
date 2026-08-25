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
        public Image healthFill;

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
        public Button settingsButton;
        public Text statsText;

        [Header("Settings Panel")]
        public GameObject settingsPanel;
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Button settingsBackButton;

        public GameObject[] cinematicHidden;

        public void SetCinematicMode(bool on)
        {
            foreach (var go in cinematicHidden)
                if (go != null) go.SetActive(!on);
        }

        private ThirdPersonController player;
        private SurvivalNeeds survival;
        private Inventory inventory;
        private Interactor interactor;
        private SaveLoad saveLoad;
        private Health health;
        private AudioDirector audioDirector;
        private float toastTimer;
        private bool dead;

        private void ApplySafeArea()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            Rect safeArea = Screen.safeArea;
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = minAnchor + safeArea.size;
            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;
            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }

        private void Start()
        {
            ApplySafeArea();
            player = FindFirstObjectByType<ThirdPersonController>();
            audioDirector = FindFirstObjectByType<AudioDirector>();
            if (player != null)
            {
                survival = player.GetComponent<SurvivalNeeds>();
                inventory = player.GetComponent<Inventory>();
                interactor = player.GetComponent<Interactor>();
                health = player.GetComponent<Health>();
            }
            saveLoad = FindFirstObjectByType<SaveLoad>();

            jumpButton.onClick.AddListener(OnJump);
            interactButton.onClick.AddListener(OnInteract);
            eatButton.onClick.AddListener(OnEat);
            drinkButton.onClick.AddListener(OnDrink);
            pauseButton.onClick.AddListener(OnPause);
            resumeButton.onClick.AddListener(OnResume);
            saveButton.onClick.AddListener(OnSave);

            if (settingsButton != null) settingsButton.onClick.AddListener(OnOpenSettings);
            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnCloseSettings);

            if (masterSlider != null)
            {
                masterSlider.value = audioDirector != null ? audioDirector.GetMasterVolume() : 1f;
                masterSlider.onValueChanged.AddListener(v => audioDirector?.SetMasterVolume(v));
            }
            if (musicSlider != null)
            {
                musicSlider.value = audioDirector != null ? audioDirector.GetMusicVolume() : 0.85f;
                musicSlider.onValueChanged.AddListener(v => audioDirector?.SetMusicVolume(v));
            }
            if (sfxSlider != null)
            {
                sfxSlider.value = audioDirector != null ? audioDirector.GetSfxVolume() : 1f;
                sfxSlider.onValueChanged.AddListener(v => audioDirector?.SetSfxVolume(v));
            }

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (survival != null) survival.PlayerDied += OnPlayerDied;
            if (health != null)
            {
                health.Changed += _ => { };
                health.Died += OnPlayerDied;
            }
            if (inventory != null) inventory.InventoryChanged += RefreshInventory;
            RefreshInventory();
        }

        private Rect lastSafeArea;

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea)
            {
                lastSafeArea = Screen.safeArea;
                ApplySafeArea();
            }

            HandleDesktopHotkeys();

            if (pausePanel != null && !pausePanel.activeSelf && Time.timeScale == 0f)
                Time.timeScale = 1f;

            if (survival != null)
            {
                hungerFill.fillAmount = survival.Hunger / survival.maxHunger;
                thirstFill.fillAmount = survival.Thirst / survival.maxThirst;
            }

            if (healthFill != null && health != null)
                healthFill.fillAmount = health.Current / health.maxHealth;

            if (interactor != null)
            {
                if (interactor.CurrentEnemy != null)
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                    promptText.text = "A raider blocks your path! [Press E / Click to Attack]";
                    interactLabel.text = "ATTACK [E]";
#else
                    promptText.text = "A raider blocks your path!";
                    interactLabel.text = "ATTACK";
#endif
                    interactLabel.color = new Color(1f, 0.35f, 0.25f);
                    interactButton.interactable = true;
                    return;
                }
                interactLabel.color = Color.white;
                if (interactor.CurrentTarget != null)
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                    promptText.text = "Gather " + interactor.CurrentTarget.DisplayName + " [Press E]";
                    interactLabel.text = "GATHER [E]";
#else
                    promptText.text = "Gather " + interactor.CurrentTarget.DisplayName;
                    interactLabel.text = "GATHER";
#endif
                    interactButton.interactable = true;
                }
                else
                {
                    promptText.text = "";
#if UNITY_EDITOR || UNITY_STANDALONE
                    interactLabel.text = "INTERACT [E]";
#else
                    interactLabel.text = "INTERACT";
#endif
                    interactButton.interactable = interactor.CurrentDrinkSpot != null;
                }
            }

            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f) toastText.text = "";
            }
        }

        private void HandleDesktopHotkeys()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (Input.GetKeyDown(KeyCode.Space)) OnJump();
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)) OnInteract();
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.H)) OnEat();
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.J)) OnDrink();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (settingsPanel != null && settingsPanel.activeSelf) OnCloseSettings();
                else if (pausePanel != null && pausePanel.activeSelf) OnResume();
                else OnPause();
            }
#endif
        }

        private void OnJump() => player?.RequestJump();

        private void OnInteract()
        {
            if (interactor == null || inventory == null) return;

            if (interactor.CurrentEnemy != null)
            {
                bool killed = interactor.CurrentEnemy.TakeDamage(34f);
                ShowToast(killed ? "Raider defeated!" : "Strike!");
                return;
            }

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
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnOpenSettings()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void OnCloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
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
            if (health != null) health.ResetFull();
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
