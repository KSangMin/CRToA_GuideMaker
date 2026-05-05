using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectMenu : MonoBehaviour
{
    [SerializeField] private GameObject selectPanelPrefab;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;

    [SerializeField] private GameObject specialPanelPrefab;

    private void Start()
    {
        SetPanels();
    }

    private void SetPanels()
    {
        SetSpecialPanel();
        SetCookiePanels();
    }

    private void SetSpecialPanel()
    {
        SpecialPanel specialPanel = Instantiate(specialPanelPrefab, content).GetComponent<SpecialPanel>();
        specialPanel.SetPanel(scrollRect);
    }

    private void SetCookiePanels()
    {
        StartCoroutine(SetCookiePanelsCoroutine());
    }

    private IEnumerator SetCookiePanelsCoroutine()
    {
        List<CookieData> cookieDataList = AddressableManager.Instance.GetAllCookieData();
        List<CookieData> orderedCookieDataList = cookieDataList.OrderBy(data => data.order).ToList();

        for (int i = orderedCookieDataList.Count - 1; i >= 0; i--)
        {
            SelectPanel panel = Instantiate(selectPanelPrefab, content).GetComponent<SelectPanel>();
            panel.SetPanel(orderedCookieDataList[i], scrollRect);
        }

        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }
}
