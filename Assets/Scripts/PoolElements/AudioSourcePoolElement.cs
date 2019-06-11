using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tengio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourcePoolElement : PoolElement
    {
        public AudioClip[] audioClips;

        private static int nextAudioClipIndex = 0;

        private const float FADE_DURATION = 0.5f; // in s.

        private AudioSource audioSource;
        private Coroutine activateAndFadeInCoroutine;
        private Coroutine fadeOutAndDeactivateCoroutine;

        bool isActivating = false;

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += DeactivateOnSceneChange;

            if (audioSource != null)
            {
                audioSource.Play();
            }
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= DeactivateOnSceneChange;

            if (gameObject.activeSelf)
            {
                Deactivate(); // If parent is disabled then disable self.
            }
            else
            {
                if (fadeOutAndDeactivateCoroutine != null)
                {
                    StopCoroutine(fadeOutAndDeactivateCoroutine);
                }
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            }
        }

        private void DeactivateOnSceneChange(Scene _, Scene __)
        {
            // Because changing scene kills coroutines (even for objects that don't destroy on load).
            Deactivate();
        }

        public override void Initialize()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClips[nextAudioClipIndex];
            nextAudioClipIndex++;
            if (nextAudioClipIndex == audioClips.Length)
            {
                nextAudioClipIndex = 0;
            }
            Deactivate();
        }

        public override void Activate()
        {
            audioSource.Play();
            isActivating = false;
        }

        public override void Deactivate()
        {
            audioSource.Pause();
            isActivating = false;
        }

        public void FadeOutAndDeactivate()
        {
            if (fadeOutAndDeactivateCoroutine != null)
            {
                StopCoroutine(fadeOutAndDeactivateCoroutine);
            }
            fadeOutAndDeactivateCoroutine = StartCoroutine(FadeOutAndDeactivateEnumerator(FADE_DURATION));
        }

        public void ActivateAndFadeIn()
        {
            if (activateAndFadeInCoroutine != null)
            {
                StopCoroutine(activateAndFadeInCoroutine);
            }
            activateAndFadeInCoroutine = StartCoroutine(ActivateAndFadeInEnumerator(FADE_DURATION));
        }

        public override bool IsAvailable()
        {
            return !audioSource.isPlaying && !isActivating;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void AttachToTransform(Transform parent)
        {
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
        }

        private IEnumerator FadeOutAndDeactivateEnumerator(float duration)
        {
            float volume = audioSource.volume;
            while (volume > 0f)
            {
                audioSource.volume = Mathf.Clamp01(volume);
                volume -= Time.deltaTime / duration;
                yield return null;
            }
            audioSource.volume = 0f;
            Deactivate();
        }

        private IEnumerator ActivateAndFadeInEnumerator(float duration)
        {
            isActivating = true;
            yield return new WaitForSeconds(Random.Range(0f, 7f));

            float volume = audioSource.volume;
            while (volume < 1f)
            {
                audioSource.volume = Mathf.Clamp01(volume);
                volume += Time.deltaTime / duration;
                yield return null;
            }
            audioSource.volume = 1f;
            Activate();
        }
    }
}