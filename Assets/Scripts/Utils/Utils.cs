using System;
using System.Collections;
using UnityEngine;

namespace Tengio
{
    public static class Utils
    {
        public static IEnumerator LerpMaterialEmission(Material material, float loopDuration)
        {
            return LerpMaterialEmission(new Material[1] { material }, loopDuration);
        }

        public static IEnumerator LerpMaterialEmission(Material[] materials, float loopDuration)
        {
            float t = 0F; // Always between 0 and 1;
            float minimum = 0.4F;
            float maximum = 1F;
            float emissionScale = 0F;
            Color baseColor = Color.white;

            while (true)
            {
                emissionScale = Mathf.Lerp(minimum, maximum, t);
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].SetColor("_EmissionColor", baseColor * emissionScale);
                }
                //DynamicGI.SetEmissive(barrelsRenderer, baseColor * emissionScale); // If we want it to light up the environment.
                t += Time.deltaTime * 2 / loopDuration;
                if (t > 1F)
                {
                    t = 0F;
                    float temp = maximum;
                    maximum = minimum;
                    minimum = temp;
                }
                yield return null;
            }
        }

        public static void ExecuteAfterDelay(float delay, MonoBehaviour monoBehaviour, Action function)
        {
            monoBehaviour.StartCoroutine(ExecuteAfterDelayIEnumerator(delay, function));
        }

        public static Vector2 RandomInRing(float innerRingRadius, float outerRingRadius)
        {
            float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            float distance = Mathf.Sqrt(UnityEngine.Random.Range(innerRingRadius * innerRingRadius, outerRingRadius * outerRingRadius));
            return new Vector2(distance * Mathf.Cos(angle), distance * Mathf.Sin(angle));
        }

        private static IEnumerator ExecuteAfterDelayIEnumerator(float delay, Action function)
        {
            yield return new WaitForSeconds(delay);
            function();
        }

        public static IEnumerator SoundFadeOutEnumerator(AudioSource audioSource, float duration, Action callback = null)
        {
            float volume = audioSource.volume;
            while (volume > 0f)
            {
                audioSource.volume = Mathf.Clamp01(volume);
                volume -= Time.deltaTime / duration;
                yield return null;
            }
            audioSource.volume = 0f;
            audioSource.Pause();
            callback?.Invoke();
        }

        public static IEnumerator SoundFadeInEnumerator(AudioSource audioSource, float duration, float maxVolume = 1f, Action callback = null)
        {
            float volume;
            if (audioSource.isPlaying)
            {
                volume = audioSource.volume;
            }
            else
            {
                volume = 0f;
                audioSource.volume = 0f;
                audioSource.Play();
            }
            while (volume < maxVolume)
            {
                audioSource.volume = Mathf.Clamp01(volume);
                volume += Time.deltaTime / duration;
                yield return null;
            }
            audioSource.volume = maxVolume;
            callback?.Invoke();
        }
    }
}
