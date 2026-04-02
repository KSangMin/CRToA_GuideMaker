using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectMenu : MonoBehaviour
{
    [SerializeField] private GameObject _panelPrefab;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;

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
        //다른 기능 추가 예정
    }

    private void SetCookiePanels()
    {
        List<CookieData> cookieDataList = AddressableManager.Instance.GetAllCookieData();

        foreach(CookieData cookieData in cookieDataList)
        {
            SelectPanel panel = Instantiate(_panelPrefab, content).GetComponent<SelectPanel>();
            panel.SetPanel(cookieData, scrollRect);
        }
    }
}
