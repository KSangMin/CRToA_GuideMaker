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

    private Coroutine downloadingCoroutine;

    protected override void Start()
    {
        base.Start();

        AddressableManager.Instance.startLoad += Load;
        AddressableManager.Instance.startCatalogCheck += CheckCatalog;
        AddressableManager.Instance.startDownload += Download;
        AddressableManager.Instance.startLoadAsset += LoadAsset;
        AddressableManager.Instance.onLoadProgress += OnProgressChanged;
        AddressableManager.Instance.endLoad += EndLoad;
        AddressableManager.Instance.loadFailed += LoadFailed;

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
        loadingPercentText.SetText("{0}%", Mathf.RoundToInt(progress));
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

        if (downloadingCoroutine != null)
        {
            StopCoroutine(downloadingCoroutine);
        }

        downloadingCoroutine = StartCoroutine(Downloading());
    }

    private IEnumerator Downloading()
    {
        loadingPercentText.gameObject.SetActive(true);
        loadingMBText.gameObject.SetActive(true);

        float cur = 0f;

        while (true)
        {
            cur = AddressableManager.Instance.patchMap.Sum(x => x.Value);
            float patchSize = AddressableManager.Instance.patchSize;
            float progress = patchSize > 0 ? Mathf.Clamp01(cur / patchSize) : 1f;

            loadingBar.value = progress;
            loadingPercentText.SetText("{0}%", Mathf.RoundToInt(progress * 100f));
            loadingMBText.SetText("{0}/{1}(MB)"
                , Util.ConversionToMB(cur)
                , Util.ConversionToMB(AddressableManager.Instance.patchSize));

            if (progress >= 1f)
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

    public override void Clear()
    {
        if (downloadingCoroutine != null)
        {
            StopCoroutine(downloadingCoroutine);
            downloadingCoroutine = null;
        }

        AddressableManager.Instance.startLoad -= Load;
        AddressableManager.Instance.startCatalogCheck -= CheckCatalog;
        AddressableManager.Instance.startDownload -= Download;
        AddressableManager.Instance.startLoadAsset -= LoadAsset;
        AddressableManager.Instance.onLoadProgress -= OnProgressChanged;
        AddressableManager.Instance.endLoad -= EndLoad;
        AddressableManager.Instance.loadFailed -= LoadFailed;
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void EndLoad()
    {
        Clear();

        loadingText.SetText("Loading Complete!");
        loadingBar.value = 1f;
        loadingPercentText.SetText("100%");
        loadingMBText.gameObject.SetActive(true);
        loadingMBText.SetText("Click Anywhere To Continue");
        continueButton.interactable = true;
    }

    private void LoadFailed(string message)
    {
        Clear();

        loadingText.SetText("Loading Failed");
        loadingPercentText.gameObject.SetActive(true);
        loadingPercentText.SetText("0%");
        loadingMBText.gameObject.SetActive(true);
        loadingMBText.SetText(message);
        continueButton.interactable = false;
    }
}
