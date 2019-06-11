using UnityEngine;

namespace Tengio
{
    [RequireComponent(typeof(TextMesh))]
    public class FPSDisplay : MonoBehaviour
    {
        private TextMesh textMesh;

        private float deltaTime = 0.0f;

        private float updateInterval = 0.5F;
        private float accum = 0; // FPS accumulated over the interval
        private int frames = 0; // Frames drawn over the interval
        private float timeleft; // Left time for current interval

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
        }

        void Update()
        {
            //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            //float msec = deltaTime * 1000.0f;
            //float fps = 1.0f / deltaTime;
            //string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            //textMesh.text = text;





            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0.0)
            {
                // display two fractional digits (f2 format)
                float fps = accum / frames;
                string format = string.Format("{0:F2} FPS", fps);
                textMesh.text = format;

                if (fps < 87)
                    textMesh.color = Color.yellow;
                else
                    if (fps < 60)
                    textMesh.color = Color.red;
                else
                    textMesh.color = Color.green;
                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
    }
}
