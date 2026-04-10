using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;
using System.Collections;

public class OptionPanel : MonoBehaviour
{
    [SerializeField] private Button downloadButton;

    private void Awake()
    {
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

        // 1. 확장자 필터 설정 (jpg, png 선택 가능)
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        };

        // 2. 저장 경로 받아오기
        string path = StandaloneFileBrowser.SaveFilePanel("이미지 저장하기", "", "MyImage", extensions);

        if (!string.IsNullOrEmpty(path))
        {
            // 3. 확장자에 따라 인코딩
            byte[] textureBytes;
            if (path.ToLower().EndsWith(".jpg") || path.ToLower().EndsWith(".jpeg"))
            {
                textureBytes = targetTexture.EncodeToJPG();
            }
            else
            {
                textureBytes = targetTexture.EncodeToPNG(); // 기본값 PNG
            }

            // 4. 파일 쓰기
            File.WriteAllBytes(path, textureBytes);
            Debug.Log($"이미지가 성공적으로 저장되었습니다: {path}");
        }
    }
}
