using SFB;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices; // WebGL 연동

public class OptionPanel : MonoBehaviour
{
    [SerializeField] private Button resetCycleButton;
    [SerializeField] private Button downloadButton;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadWebGLFile(byte[] array, int size, string fileName, string contentType);
#endif

    private void Awake()
    {
        resetCycleButton.onClick.AddListener(ResetCycle);
        downloadButton.onClick.AddListener(DownLoad);
    }

    private void DownLoad()
    {
        StartCoroutine(DownloadCoroutine());
    }

    IEnumerator DownloadCoroutine()
    {
        yield return new WaitForEndOfFrame();

        Texture2D targetTexture = UIManager.Instance.GetUI<UI_Result>().GetCycleTexture();
        SaveImage(targetTexture);
    }

    private void SaveImage(Texture2D targetTexture)
    {
        if (targetTexture == null)
        {
            Debug.LogError("저장할 텍스처가 없습니다!");
            return;
        }

        byte[] textureBytes = targetTexture.EncodeToPNG(); //기본값 png

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadWebGLFile(textureBytes, textureBytes.Length, "New Cycle.png", "image/png");

#else
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        };

        string path = StandaloneFileBrowser.SaveFilePanel("이미지 저장하기", "", "New Cycle", extensions);

        if (!string.IsNullOrEmpty(path))
        {
            // 3. 확장자에 따라 인코딩
            if (path.ToLower().EndsWith(".jpg") || path.ToLower().EndsWith(".jpeg"))
            {
                textureBytes = targetTexture.EncodeToJPG();
            }

            // 4. 파일 쓰기
            File.WriteAllBytes(path, textureBytes);
            Debug.Log($"이미지가 성공적으로 저장되었습니다: {path}");
        }
#endif
    }

    private void ResetCycle()
    {
        UIManager.Instance.GetUI<UI_Result>().ResetCycle();
    }
}
