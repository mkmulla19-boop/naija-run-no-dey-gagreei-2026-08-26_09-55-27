using System.Collections;
using UnityEngine;

namespace Olomu.Systems
{
    public class AudioDirector : MonoBehaviour
    {
        public AudioSource music;
        public AudioSource ambience;
        public AudioSource sfx;

        public AudioClip bed;
        public AudioClip tensionHit;
        public AudioClip morningAmbience;

        private float masterVol = 1f;
        private float musicVol = 0.85f;
        private float ambienceVol = 0.55f;
        private float sfxVol = 1f;
        private bool morningStarted;

        private void Awake()
        {
            masterVol = PlayerPrefs.GetFloat("Vol_Master", 1f);
            musicVol = PlayerPrefs.GetFloat("Vol_Music", 0.85f);
            ambienceVol = PlayerPrefs.GetFloat("Vol_Ambience", 0.55f);
            sfxVol = PlayerPrefs.GetFloat("Vol_Sfx", 1f);
        }

        public void StartMorning()
        {
            if (morningStarted) return;
            morningStarted = true;
            ApplyVolumes();
            if (ambience != null && morningAmbience != null)
            {
                ambience.clip = morningAmbience;
                ambience.loop = true;
                ambience.volume = ambienceVol * masterVol;
                ambience.Play();
            }
            if (music != null && bed != null)
            {
                music.clip = bed;
                music.loop = true;
                music.volume = 0f;
                music.Play();
                StartCoroutine(FadeIn(music, 4f, musicVol * masterVol));
            }
        }

        private void Start()
        {
            StartMorning();
        }

        public void SetMasterVolume(float v)
        {
            masterVol = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("Vol_Master", masterVol);
            ApplyVolumes();
        }

        public void SetMusicVolume(float v)
        {
            musicVol = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("Vol_Music", musicVol);
            ApplyVolumes();
        }

        public void SetAmbienceVolume(float v)
        {
            ambienceVol = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("Vol_Ambience", ambienceVol);
            ApplyVolumes();
        }

        public void SetSfxVolume(float v)
        {
            sfxVol = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("Vol_Sfx", sfxVol);
            ApplyVolumes();
        }

        public float GetMasterVolume() => masterVol;
        public float GetMusicVolume() => musicVol;
        public float GetAmbienceVolume() => ambienceVol;
        public float GetSfxVolume() => sfxVol;

        private void ApplyVolumes()
        {
            if (music != null) music.volume = musicVol * masterVol;
            if (ambience != null) ambience.volume = ambienceVol * masterVol;
            if (sfx != null) sfx.volume = sfxVol * masterVol;
        }

        public void PlayTensionHit()
        {
            if (sfx != null && tensionHit != null) sfx.PlayOneShot(tensionHit, 0.95f * sfxVol * masterVol);
        }

        public void DuckMusic(float target, float seconds)
        {
            if (music != null) StartCoroutine(FadeTo(music, target * masterVol, seconds));
        }

        private IEnumerator FadeIn(AudioSource src, float time, float target)
        {
            float start = src.volume;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                src.volume = Mathf.Lerp(start, target, t / time);
                yield return null;
            }
            src.volume = target;
        }

        private IEnumerator FadeTo(AudioSource src, float target, float time)
        {
            float start = src.volume;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                src.volume = Mathf.Lerp(start, target, t / time);
                yield return null;
            }
            src.volume = target;
        }
    }
}
