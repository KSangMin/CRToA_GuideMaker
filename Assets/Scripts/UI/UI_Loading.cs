using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Loading : UI
{
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI loadingPercentText;
    [SerializeField] private TextMeshProUGUI loadingMBText;

    [SerializeField] private Button continueButton;

    protected override void Start()
    {
        base.Start();

        AddressableManager.Instance.startLoad += Load;
        AddressableManager.Instance.startCatalogCheck += CheckCatalog;
        AddressableManager.Instance.startDownload += Download;
        AddressableManager.Instance.startLoadAsset += LoadAsset;
        AddressableManager.Instance.onLoadProgress += OnProgressChanged;
        AddressableManager.Instance.endLoad += EndLoad;

        loadingPercentText.gameObject.SetActive(false);
        loadingMBText.gameObject.SetActive(false);

        continueButton.onClick.AddListener(() => SceneManager.LoadScene("Main"));
        continueButton.interactable = false;
    }

    private void OnProgressChanged(float progress)
    {
        float percent = progress / 100;
        loadingBar.value = percent;
        loadingPercentText.gameObject.SetActive(true);
        loadingPercentText.SetText("{0}%", progress);
        Debug.Log($"progress: {progress}, percent: {percent}");
    }

    private void Load()
    {
        loadingText.SetText("Loading");
    }

    private void CheckCatalog()
    {
        loadingText.SetText("Checking Catalog");
    }

    private void Download()
    {
        loadingText.SetText("Downloading");

        StartCoroutine(Downloading());
    }

    private IEnumerator Downloading()
    {
        loadingPercentText.gameObject.SetActive(true);
        loadingMBText.gameObject.SetActive(true);

        float cur = 0f;

        while (true)
        {
            cur = AddressableManager.Instance.patchMap.Sum(x => x.Value);
            loadingBar.value = cur / AddressableManager.Instance.patchSize;
            loadingPercentText.SetText("{0}%"
                , (int)(cur / AddressableManager.Instance.patchSize) * 100);
            loadingMBText.SetText("{0}/{1}(MB)"
                , Util.ConversionToMB(cur)
                , Util.ConversionToMB(AddressableManager.Instance.patchSize));

            if (cur >= AddressableManager.Instance.patchSize)
            {
                loadingMBText.gameObject.SetActive(false);
                break;
            }

            yield return null;
        }
    }

    private void LoadAsset()
    {
        loadingBar.value = 0f;
        loadingText.SetText("Loading Assets");
    }

    private new void Clear()
    {
        AddressableManager.Instance.startLoad -= Load;
        AddressableManager.Instance.startCatalogCheck -= CheckCatalog;
        AddressableManager.Instance.startDownload -= Download;
        AddressableManager.Instance.startLoadAsset -= LoadAsset;
        AddressableManager.Instance.onLoadProgress -= OnProgressChanged;
        AddressableManager.Instance.endLoad -= EndLoad;
    }

    private void EndLoad()
    {
        Clear();

        loadingText.SetText("Loading Complete!");
        loadingMBText.SetText("100%");
        loadingMBText.gameObject.SetActive(true);
        loadingMBText.SetText("Click Anywhere To Continue");
        continueButton.interactable = true;
    }
}
