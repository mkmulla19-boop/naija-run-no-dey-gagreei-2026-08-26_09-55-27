using UnityEngine;

namespace NaijaRun.Core
{
    public sealed class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        [SerializeField] private AudioClip afrobeatLoop;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip slideSound;
        [SerializeField] private AudioClip laneSwitchSound;

        private AudioSource musicSource;
        private AudioSource effectsSource;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = gameObject.AddComponent<AudioSource>();
            effectsSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            if (afrobeatLoop == null)
                afrobeatLoop = Resources.Load<AudioClip>("Audio/NaijaRun_voice_preview");

            if (afrobeatLoop != null)
            {
                musicSource.clip = afrobeatLoop;
                musicSource.Play();
            }
        }

        public static void PlayJump() => Play(instance == null ? null : instance.jumpSound);
        public static void PlaySlide() => Play(instance == null ? null : instance.slideSound);
        public static void PlayLaneSwitch() => Play(instance == null ? null : instance.laneSwitchSound);

        private static void Play(AudioClip clip)
        {
            if (instance != null && clip != null)
                instance.effectsSource.PlayOneShot(clip);
        }
    }
}