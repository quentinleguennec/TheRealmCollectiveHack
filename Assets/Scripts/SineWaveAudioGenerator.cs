using UnityEngine;

namespace Tengio
{
    [RequireComponent(typeof(AudioSource))]
    public class SineWaveAudioGenerator : MonoBehaviour
    {
        public float sampleRate = 44100;
        public float waveLengthInSeconds = 2.0f;

        private AudioSource audioSource;
        private int timeIndex = 0;

        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!audioSource.isPlaying)
                {
                    timeIndex = 0;
                    audioSource.Play();
                }
                else
                {
                    audioSource.Stop();
                }
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            for (int i = 0; i < data.Length; i += channels)
            {
                data[i] = CreateSine(timeIndex, 440f, sampleRate);

                timeIndex++;
                if (timeIndex >= (sampleRate * waveLengthInSeconds))
                {
                    timeIndex = 0;
                }
            }
        }

        private float CreateSine(int timeIndex, float frequency, float sampleRate)
        {
            return Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate);
        }
    }
}