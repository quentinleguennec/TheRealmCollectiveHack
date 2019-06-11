using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Tengio
{
    [RequireComponent(typeof(Camera))]
    public class ScreenshotCapture : MonoBehaviour
    {
        [SerializeField]
        private int width = 2880;
        [SerializeField]
        private int height = 1620;
        [SerializeField]
        string screenshotPrefix = "";
        [SerializeField]
        KeyCode captureKey = KeyCode.C;

        private Camera mCamera;
        private string folderPath;
        
        private void Awake()
        {
            mCamera = GetComponent<Camera>();
            folderPath = Application.persistentDataPath + "/screenshots";
        }

        private void Start()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(captureKey))
            {
                TakeScreenshot();
                Debug.Log("Picture taken: " + GetScreenShotName());
            }
#endif
        }

        private string GetScreenShotName()
        {
            return string.Format("{0}/{1}{2}.png",
                                 folderPath,
                                 screenshotPrefix,
                                 System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        private void TakeScreenshot()
        {
            StartCoroutine(TakeScreenshotCoroutine());
        }

        private IEnumerator TakeScreenshotCoroutine()
        {
            yield return new WaitForEndOfFrame();
            RenderTexture rt = new RenderTexture(width, height, 24);
            mCamera.targetTexture = rt;
            mCamera.Render();
            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            mCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            yield return null;
            ScreenshotBuffer screenshot = new ScreenshotBuffer(screenShot, GetScreenShotName());
            File.WriteAllBytes(screenshot.filename, screenshot.tex2D.EncodeToPNG());
            Destroy(screenshot.tex2D);
            screenshot = null;

            //Thread thread = new Thread(param => {
            //    ScreenshotBuffer screenshot = param as ScreenshotBuffer;
            //    File.WriteAllBytes(screenshot.filename, screenshot.tex2D.EncodeToPNG());
            //    Destroy(screenshot.tex2D);
            //    screenshot = null;
            //    param = null;
            //});
            //thread.Start(new ScreenshotBuffer(screenShot, GetScreenShotName()));
            //thread = null;
        }

    }

    public class ScreenshotBuffer
    {
        public string filename { get; private set; }
        public Texture2D tex2D { get; private set; }

        public ScreenshotBuffer(Texture2D tex2D, string filename)
        {
            this.tex2D = tex2D;
            this.filename = filename;
        }
    }
}