using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Olomu.Systems
{
    public class OpeningSequence : MonoBehaviour
    {
        public CanvasGroup panel;
        public Text lineText;
        public Text hint;

        private static readonly string[] Lines =
        {
            "The village is peaceful this morning...",
            "Suddenly there is confusion. People are running.",
            "Sounds of fighting can be heard in the distance.",
            "Your father finds you.",
            "Father: \"Go. Don't look back. Survive, and find your way home.\"",
            "You must escape into the wilderness."
        };

        private static readonly float[] Durations = { 2.5f, 2.8f, 2.5f, 1.8f, 4.5f, 2.0f };

        public bool IsPlaying { get; private set; }
        public bool HasPlayed { get; private set; }

        public event System.Action SequenceFinished;
        private int index = -1;
        private float timer;

        private void OnEnable()
        {
            if (panel != null) panel.alpha = 1f;
            StartSequence();
        }

        public void StartSequence()
        {
            if (HasPlayed || IsPlaying) return;
            IsPlaying = true;
            HasPlayed = true;
            NextLine();
        }

        private void Update()
        {
            if (!IsPlaying) return;

            timer -= Time.unscaledDeltaTime;
            bool tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#if UNITY_EDITOR || UNITY_STANDALONE
            tapped |= Input.GetMouseButtonDown(0);
#endif
            if (timer <= 0f || tapped) NextLine();
        }

        private void NextLine()
        {
            index++;
            if (index >= Lines.Length)
            {
                Finish();
                return;
            }
            lineText.text = Lines[index];
            hint.text = index == Lines.Length - 1 ? "" : "tap to continue";
            timer = Durations[index];
        }

        private void Finish()
        {
            IsPlaying = false;
            if (panel != null)
            {
                panel.alpha = 0f;
                panel.blocksRaycasts = false;
                panel.interactable = false;
            }
            SequenceFinished?.Invoke();
        }
    }
}
