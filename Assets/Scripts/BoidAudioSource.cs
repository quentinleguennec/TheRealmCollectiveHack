using System;
using System.Collections;
using UnityEngine;

namespace Tengio
{
    [RequireComponent(typeof(AudioSource))]
    public class BoidAudioSource : MonoBehaviour
    {
        public AudioClip FishAudioClip { get; set; }
        public AudioClip FlyingAudioClip { get; set; }
        public AudioClip PerchedAudioClip { get; set; }
        public AudioClip ShapeTransitionAudioClip { get; set; }

        private const float FadeDuration = 0.5f; // In s.

        private AudioSource audioSource;
        private float maxVolume;
        private Coroutine fadeInCoroutine;
        private Coroutine fadeOutCoroutine;
        private Coroutine transitionShapeCoroutine;

        public void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            maxVolume = audioSource.volume;
        }

        public void Start()
        {
            audioSource.loop = true;
            audioSource.clip = FishAudioClip;
        }

        public void PlayFish()
        {
            audioSource.Stop();
            audioSource.clip = FishAudioClip;
            FadeIn();
        }

        public void PlayFishToPerched()
        {
            StopAllCoroutines();
            PlayShapeTransition(() =>
            {
                audioSource.clip = PerchedAudioClip;
                audioSource.Play();
            });
        }

        public void PlayFlyingToPerched()
        {
            StopAllCoroutines();
            PlayShapeTransition(() =>
            {
                audioSource.clip = PerchedAudioClip;
                audioSource.Play();
            });
        }

        public void PlayPerchedToFlying()
        {
            StopAllCoroutines();
            FadeOut(() =>
            {
                audioSource.Stop();
                audioSource.clip = FlyingAudioClip;
                audioSource.Play();
                FadeIn();
            });
        }

        private void PlayShapeTransition(Action callback = null)
        {
            if (transitionShapeCoroutine != null) StopCoroutine(transitionShapeCoroutine);
            transitionShapeCoroutine = StartCoroutine(PlayShapeTransitionEnumerator(callback));
        }

        private void FadeOut(Action callback = null)
        {
            if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = StartCoroutine(Utils.SoundFadeOutEnumerator(audioSource, FadeDuration, callback));
        }

        private void FadeIn(Action callback = null)
        {
            if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = StartCoroutine(Utils.SoundFadeInEnumerator(audioSource, FadeDuration, maxVolume, callback));
        }

        private IEnumerator PlayShapeTransitionEnumerator(Action callback = null)
        {
            yield return Utils.SoundFadeOutEnumerator(audioSource, FadeDuration, callback);
            audioSource.Stop();
            audioSource.clip = ShapeTransitionAudioClip;
            audioSource.volume = maxVolume;
            audioSource.Play();
            yield return new WaitForSeconds(ShapeTransitionAudioClip.length);
            audioSource.Stop();
            callback?.Invoke();
        }
    }
}