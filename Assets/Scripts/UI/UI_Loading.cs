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

        AddressableManager.Instance.onLoadProgress += OnProgressChanged;

        AddressableManager.Instance.startLoad += Load;
        AddressableManager.Instance.startCatalogCheck += CheckCatalog;
        AddressableManager.Instance.startDownload += Download;
        AddressableManager.Instance.endLoad += EndLoad;

        loadingPercentText.gameObject.SetActive(false);
        loadingMBText.gameObject.SetActive(false);

        continueButton.onClick.AddListener(() => SceneManager.LoadScene("Main"));
        continueButton.interactable = false;
    }

    private void OnProgressChanged(float progress)
    {
        int percent = (int)(progress / 100);
        loadingBar.value = percent;
        loadingPercentText.gameObject.SetActive(true);
        loadingPercentText.SetText("{0}%", percent);
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
                yield return new WaitForSeconds(2f);

                AddressableManager.Instance.startDownload -= Download;

                EndLoad();

                break;
            }

            yield return null;
        }
    }

    private void EndLoad()
    {
        loadingText.SetText("Loading Complete!");
        loadingMBText.gameObject.SetActive(true);
        loadingMBText.SetText("Click Anywhere To Continue");
        continueButton.interactable = true;
    }
}
