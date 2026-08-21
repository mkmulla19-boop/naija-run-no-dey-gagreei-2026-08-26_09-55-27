using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Olomu.Systems
{
    public class CinematicDirector : MonoBehaviour
    {
        public enum Phase { Title, Establish, Life, Chaos, Father, Escape, Handoff, Playing }

        [Header("Camera")]
        public Camera cineCam;

        [Header("Targets")]
        public Transform villageFocus;
        public Transform campfire;
        public Transform playerT;
        public Light sun;
        public CineActor father;
        public CineActor[] raiders;

        [Header("UI")]
        public Text titleStudio;
        public Text titleMain;
        public Text subtitle;
        public Image blackOverlay;
        public RectTransform letterTop;
        public RectTransform letterBot;
        public Button skipButton;

        [Header("Systems")]
        public AudioDirector audioDirector;
        public MobileHUD hud;
        public ThirdPersonController playerController;

        public Phase Current { get; private set; }
        public bool IsPlaying { get; private set; } = true;
        public event Action Finished;

        private float tPhase;
        private Color sunColorOrig;
        private float sunIntensityOrig;
        private Vector3[] escapePath =
        {
            new Vector3(-9f, 0f, 7f),
            new Vector3(-17f, 0f, 4.5f),
            new Vector3(-22.5f, 0f, 2.5f)
        };
        private int escapeIdx;
        private static readonly Vector3[] chaosCamFrom = { new Vector3(12f, 4.2f, 14f), new Vector3(-6f, 3.2f, 16f) };
        private static readonly Vector3[] estabCamFrom = { new Vector3(30f, 27f, -32f), new Vector3(15f, 18f, -17f) };

        private void Start()
        {
            if (sun != null)
            {
                sunColorOrig = sun.color;
                sunIntensityOrig = sun.intensity;
            }
            if (playerController != null)
            {
                playerController.ControlsEnabled = false;
                playerController.CinematicControl = true;
            }
            foreach (var r in raiders) if (r != null) r.gameObject.SetActive(false);
            EnterPhase(Phase.Title);
            if (skipButton != null) skipButton.onClick.AddListener(Skip);
        }

        private void Update()
        {
            if (!IsPlaying || Current == Phase.Playing) return;
            tPhase += Time.unscaledDeltaTime;
            switch (Current)
            {
                case Phase.Title: UpdateTitle(); break;
                case Phase.Establish: MoveCamera(estabCamFrom[0], estabCamFrom[1], villageFocus.position, 8.5f, 0f); AdvanceAfter(8.5f, Phase.Life); break;
                case Phase.Life: MoveCamera(new Vector3(-8f, 2.3f, 11f), new Vector3(-2f, 1.8f, 6.5f), campfire.position, 7f, 0.02f); AdvanceAfter(7f, Phase.Chaos); break;
                case Phase.Chaos: MoveCamera(chaosCamFrom[0], chaosCamFrom[1], villageFocus.position, 7.5f, 0.32f); AdvanceAfter(7.5f, Phase.Father); break;
                case Phase.Father: UpdateFatherShot(); AdvanceAfter(9.5f, Phase.Escape); break;
                case Phase.Escape: UpdateEscapeShot(); break;
                case Phase.Handoff: UpdateHandoff(); break;
            }
        }

        private void SetPhase(Phase p) { Current = p; tPhase = 0f; }

        private void AdvanceAfter(float dur, Phase next)
        {
            if (tPhase >= dur) EnterPhase(next);
        }

        private void EnterPhase(Phase p)
        {
            SetPhase(p);
            switch (p)
            {
                case Phase.Title:
                    SetBlack(1f);
                    Letterbox(true);
                    if (hud != null) hud.SetCinematicMode(true);
                    if (audioDirector != null) audioDirector.StartMorning();
                    if (titleStudio != null) titleStudio.text = "M K M U L L A   G A M E   S T U D I O   P R E S E N T S";
                    if (titleMain != null) titleMain.text = "O L O M U";
                    break;

                case Phase.Establish:
                    StartCoroutine(FadeBlack(1f, 0f, 1.4f));
                    Say("");
                    break;

                case Phase.Life:
                    Say("The village is peaceful this morning...");
                    break;

                case Phase.Chaos:
                    Say("Suddenly there is confusion. People are running.");
                    if (audioDirector != null) audioDirector.PlayTensionHit();
                    if (sun != null) StartCoroutine(ShiftSun());
                    foreach (var r in raiders)
                        if (r != null) { r.gameObject.SetActive(true); r.Begin(); }
                    break;

                case Phase.Father:
                    Say("Your father finds you.");
                    if (father != null) father.Begin();
                    Invoke(nameof(FatherLine), 2.2f);
                    break;

                case Phase.Escape:
                    Say("Run.");
                    escapeIdx = 0;
                    if (raiders != null && raiders.Length > 0 && raiders[0] != null)
                    {
                        var pursuer = raiders[0];
                        pursuer.enabled = false;
                        pursuer.transform.position = playerT.position - Vector3.forward * 9f + Vector3.up * 1.05f;
                        pursuer.gameObject.SetActive(true);
                    }
                    break;

                case Phase.Handoff:
                    Say("");
                    if (raiders != null && raiders.Length > 0 && raiders[0] != null)
                        raiders[0].gameObject.SetActive(false);
                    ActivateInvasionWorld();
                    StartCoroutine(HandoffSequence());
                    break;
            }
        }

        private void FatherLine()
        {
            if (Current == Phase.Father)
                Say("Father: \"Go. Don't look back. Survive, and find your way home.\"");
        }

        private void UpdateTitle()
        {
            float a = tPhase;
            if (titleStudio != null)
            {
                float sa = Mathf.Clamp01(a / 1.0f) * Mathf.Clamp01((4.6f - a) / 0.8f);
                var c = titleStudio.color; c.a = sa; titleStudio.color = c;
            }
            if (titleMain != null)
            {
                float ma = Mathf.Clamp01((a - 1.2f) / 1.0f) * Mathf.Clamp01((4.6f - a) / 0.8f);
                var c = titleMain.color; c.a = ma; titleMain.color = c;
            }
            if (tPhase >= 5.2f) EnterPhase(Phase.Establish);
        }

        private void UpdateFatherShot()
        {
            Vector3 mid = (playerT.position + father.transform.position) / 2f;
            Vector3 from = playerT.position + new Vector3(3.2f, 2.0f, 3.6f);
            Vector3 to = playerT.position + new Vector3(2.2f, 1.6f, 2.4f);
            float k = Smooth(tPhase / 9.5f);
            Vector3 pos = Vector3.Lerp(from, to, k);
            ApplyHandheld(pos, mid, 0.06f);
        }

        private void UpdateEscapeShot()
        {
            DriveEscape();
            DrivePursuer();

            Vector3 fwd = Quaternion.Euler(0f, playerController.CurrentYaw, 0f) * Vector3.forward;
            Vector3 desired = playerT.position - fwd * 4.2f + Vector3.up * 2.2f;
            Vector3 pos = Vector3.Lerp(cineCam.transform.position, desired, 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
            Vector3 look = playerT.position + Vector3.up * 1.25f;
            cineCam.transform.position = pos;
            cineCam.transform.rotation = Quaternion.LookRotation(look - pos, Vector3.up);
            ApplyHandheld(pos, look, 0.10f);

            if (escapeIdx >= escapePath.Length && tPhase > 4f)
                EnterPhase(Phase.Handoff);
        }

        private void DrivePursuer()
        {
            if (raiders == null || raiders.Length == 0 || raiders[0] == null) return;
            var pursuer = raiders[0];
            var cc = pursuer.GetComponent<CharacterController>();
            Vector3 flat = playerT.position - pursuer.transform.position;
            flat.y = 0f;
            float dist = flat.magnitude;
            float gap = Mathf.Max(dist - 5.2f, 0f);
            if (cc != null && dist > 0.01f && gap > 0.05f)
                cc.Move(flat.normalized * Mathf.Min(gap / 0.35f, 6.8f) * Time.unscaledDeltaTime
                        + Vector3.down * 2.5f * Time.unscaledDeltaTime);
            pursuer.transform.rotation = Quaternion.Slerp(pursuer.transform.rotation,
                Quaternion.LookRotation(flat.normalized), 10f * Time.unscaledDeltaTime);

            if (tPhase > 1.5f && tPhase < 1.7f)
                Say("He's behind you!");
        }

        private void DriveEscape()
        {
            if (escapeIdx >= escapePath.Length)
            {
                playerController.SetCinematicInput(Vector2.zero);
                return;
            }
            Vector3 target = escapePath[escapeIdx];
            Vector3 flat = target - playerT.position;
            flat.y = 0f;
            if (flat.magnitude < 0.6f) { escapeIdx++; return; }
            float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
            playerController.SetYaw(yaw);
            playerController.SetCinematicInput(new Vector2(0f, 1f));
        }

        private Coroutine handoffRoutine;

        private void UpdateHandoff() { }

        private IEnumerator HandoffSequence()
        {
            yield return new WaitForSeconds(0.2f);

            var gameCam = playerController != null
                ? playerController.GetComponentInChildren<Camera>(true)
                : null;
            if (gameCam == null) gameCam = Camera.main;
            Vector3 p0 = cineCam.transform.position;
            Quaternion r0 = cineCam.transform.rotation;

            for (float t = 0f; t < 1.5f; t += Time.unscaledDeltaTime)
            {
                float k = Smooth(t / 1.5f);
                cineCam.transform.position = Vector3.Lerp(p0, gameCam.transform.position, k);
                cineCam.transform.rotation = Quaternion.Slerp(r0, gameCam.transform.rotation, k);
                yield return null;
            }

            gameCam.gameObject.SetActive(true);
            cineCam.gameObject.SetActive(false);

            if (hud != null) hud.SetCinematicMode(false);
            Letterbox(false);
            if (playerController != null)
            {
                playerController.CinematicControl = false;
                playerController.ControlsEnabled = true;
            }
            if (sun != null) StartCoroutine(RestoreSun());
            if (audioDirector != null) audioDirector.DuckMusic(0.55f, 3f);

            IsPlaying = false;
            Current = Phase.Playing;
            if (hud != null) StartCoroutine(TutorialToasts(hud));
            Finished?.Invoke();
        }

        private IEnumerator TutorialToasts(MobileHUD h)
        {
            yield return new WaitForSeconds(0.6f);
            h.ShowToast("Left side of screen: virtual joystick to move");
            yield return new WaitForSeconds(3.2f);
            h.ShowToast("Drag right side to look around");
            yield return new WaitForSeconds(3.2f);
            h.ShowToast("Gather wood and berries before night comes...");
        }

        private bool invasionActive;

        private void ActivateInvasionWorld()
        {
            if (invasionActive) return;
            invasionActive = true;
            if (sun != null) StartCoroutine(ShiftSun());
            if (raiders == null) return;
            foreach (var r in raiders)
            {
                if (r == null || !r.gameObject.activeSelf) continue;
                var ai = r.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    r.enabled = false;
                    ai.enabled = true;
                }
            }
        }

        public void Skip()
        {
            StopAllCoroutines();
            CancelInvoke(nameof(FatherLine));
            SetBlack(0f);
            EnterPhase(Phase.Escape);
            escapeIdx = escapePath.Length;
            playerController.SetCinematicInput(Vector2.zero);
            ActivateInvasionWorld();
            EnterPhase(Phase.Handoff);
        }

        private void MoveCamera(Vector3 from, Vector3 to, Vector3 look, float dur, float shake)
        {
            float k = Smooth(Mathf.Clamp01(tPhase / dur));
            Vector3 pos = Vector3.Lerp(from, to, k);
            ApplyHandheld(pos, look, shake);
        }

        private void ApplyHandheld(Vector3 pos, Vector3 look, float shake)
        {
            Vector3 jitter = shake > 0f
                ? new Vector3(
                    (Mathf.PerlinNoise(Time.unscaledTime * 9f, 0f) - 0.5f),
                    (Mathf.PerlinNoise(0f, Time.unscaledTime * 9f) - 0.5f),
                    0f) * shake
                : Vector3.zero;
            cineCam.transform.position = pos + jitter;
            cineCam.transform.rotation = Quaternion.LookRotation((look + jitter) - pos, Vector3.up);
        }

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        private void Say(string msg)
        {
            if (subtitle != null) subtitle.text = msg;
        }

        private void SetBlack(float a)
        {
            if (blackOverlay != null)
            {
                var c = blackOverlay.color; c.a = a; blackOverlay.color = c;
            }
        }

        private IEnumerator FadeBlack(float fromA, float toA, float time)
        {
            for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
            {
                SetBlack(Mathf.Lerp(fromA, toA, t / time));
                yield return null;
            }
            SetBlack(toA);
        }

        private void Letterbox(bool on)
        {
            if (letterTop != null) StartCoroutine(LetterAnim(letterTop, on ? 150f : 0f));
            if (letterBot != null) StartCoroutine(LetterAnim(letterBot, on ? 150f : 0f));
        }

        private IEnumerator LetterAnim(RectTransform bar, float targetH)
        {
            float startH = bar.sizeDelta.y;
            float time = 0.8f;
            for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
            {
                float h = Mathf.Lerp(startH, targetH, Smooth(t / time));
                bar.sizeDelta = new Vector2(bar.sizeDelta.x, h);
                yield return null;
            }
            bar.sizeDelta = new Vector2(bar.sizeDelta.x, targetH);
        }

        private IEnumerator ShiftSun()
        {
            Color target = new Color(1f, 0.55f, 0.3f);
            Vector3 targetV = new Vector3(target.r, target.g, target.b);
            Vector3 startV = new Vector3(sunColorOrig.r, sunColorOrig.g, sunColorOrig.b);
            for (float t = 0f; t < 1.6f; t += Time.deltaTime)
            {
                float k = t / 1.6f;
                Vector3 v = Vector3.Lerp(startV, targetV, k * 0.75f);
                sun.color = new Color(v.x, v.y, v.z);
                sun.intensity = Mathf.Lerp(sunIntensityOrig, sunIntensityOrig * 1.15f, k);
                yield return null;
            }
        }

        private IEnumerator RestoreSun()
        {
            Color start = sun.color;
            float startI = sun.intensity;
            for (float t = 0f; t < 2.5f; t += Time.deltaTime)
            {
                float k = t / 2.5f;
                sun.color = Color.Lerp(start, sunColorOrig, k);
                sun.intensity = Mathf.Lerp(startI, sunIntensityOrig, k);
                yield return null;
            }
        }
    }
}
