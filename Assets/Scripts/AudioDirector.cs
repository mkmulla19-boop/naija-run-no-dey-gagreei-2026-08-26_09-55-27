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

        public void StartMorning()
        {
            if (ambience != null && morningAmbience != null)
            {
                ambience.clip = morningAmbience;
                ambience.loop = true;
                ambience.volume = 0.55f;
                ambience.Play();
            }
            if (music != null && bed != null)
            {
                music.clip = bed;
                music.loop = true;
                music.volume = 0f;
                music.Play();
                StartCoroutine(FadeIn(music, 4f, 0.85f));
            }
        }

        public void PlayTensionHit()
        {
            if (sfx != null && tensionHit != null) sfx.PlayOneShot(tensionHit, 0.95f);
        }

        public void DuckMusic(float target, float seconds)
        {
            if (music != null) StartCoroutine(FadeTo(music, target, seconds));
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
