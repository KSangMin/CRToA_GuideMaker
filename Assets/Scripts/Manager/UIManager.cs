using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    Dictionary<Type, UI> _sceneDict = new();
    List<UI> _stackableList = new(); 
    Stack<UI_Popup> _popupUIs = new();

    int popupOrder = 10;
    int stackOrder = 1;

    Transform _root;
    Transform Root
    {
        get
        {
            if(_root == null || _root.gameObject == null)
            {
                _root = new GameObject("@UI_Root").transform;
            }
            return _root;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideStackUI();
        }
    }

    public T GetUI<T>() where T : UI
    {
        Type uiType = typeof(T);

        if (_sceneDict.TryGetValue(uiType, out UI existingUI))
        {
            return existingUI as T;
        }
        Debug.Log($"There's No {uiType.Name} in UIManager");
        return null;
        //throw new InvalidOperationException($"There's No {uiType.Name} in UIManager");
    }

    public T ShowPopupUI<T>() where T : UI_Popup
    {
        Type uiType = typeof(T);

        T ui = Util.InstantiatePrefabAndGetComponent<T>(path: $"UI/{uiType.Name}", parent: Root);
        ui.SetCanvasOrder(popupOrder++);
        _popupUIs.Push(ui);

        return ui;
    }

    public void RemovePopupUI()
    {
        if (_popupUIs.Count <= 0) return;

        UI_Popup popup = _popupUIs.Pop();
        Destroy(popup.gameObject);
        popupOrder--;
        return;
    }

    public T HideUI<T>() where T : UI
    {
        Type uiType = typeof(T);

        if (_sceneDict.TryGetValue(uiType, out UI existingUI))
        {
            existingUI.Hide();
            for(int i = _stackableList.Count - 1; i >= 0; i--)
            {
                if(existingUI == _stackableList[i])
                {
                    _stackableList.RemoveAt(i);
                    if (_stackableList.Count == 0) stackOrder = 1;
                    break;
                }
            }
            return existingUI as T;
        }

        T ui = Util.InstantiatePrefabAndGetComponent<T>(path: $"UI/{uiType.Name}", parent: Root);
        _sceneDict[uiType] = ui;
        ui.Hide();

        return null;
    }

    public void HideStackUI()
    {
        if(_stackableList.Count <= 0) return;

        UI ui = _stackableList.Last();
        ui.Hide();
        _stackableList.Remove(ui);
    }

    public T ShowUI<T>(Transform par) where T : UI
    {
        if(par == null) return ShowUI<T>();

        Type uiType = typeof(T);

        if (_sceneDict.TryGetValue(uiType, out UI existingUI))
        {
            existingUI.Show();
            return existingUI as T;
        }

        T ui = Util.InstantiatePrefabAndGetComponent<T>(path: $"UI/{uiType.Name}", parent: par);
        _sceneDict[uiType] = ui;
        ui.Show();

        return ui;
    }

    public T ShowUI<T>() where T : UI
    {
        return ShowUI<T>(Root);
    }

    public T ShowStackUI<T>() where T : UI
    {
        T ui = ShowUI<T>();
        ui.SetCanvas(stackOrder++);

        for(int i = 0; i < _stackableList.Count; i++)
        {
            if(ui == _stackableList[i])
            {
                _stackableList.RemoveAt(i);
                break;
            }
        }

        _stackableList.Add(ui);

        return ui;
    }

    //다른 클래스들에서 호출하는 메서드
    public void RemoveUI<T>() where T: UI
    {
        Type uiType = typeof(T);

        if (_sceneDict.TryGetValue(uiType, out UI existingUI))
        {
            _sceneDict.Remove(uiType);
            Destroy(existingUI.gameObject);
            return;
        }
        else throw new InvalidOperationException($"There's No {uiType.Name} in UIManager");
    }

    //UI의 Close에서 호출하는 메서드
    public void RemoveUI(UI ui)
    {
        Type uiType = ui.GetType();

        if (_sceneDict.TryGetValue(uiType, out UI existingUI))
        {
            _sceneDict.Remove(uiType);
            Destroy(existingUI.gameObject);
            return;
        }
        else throw new InvalidOperationException($"There's No {uiType.Name} in UIManager");
    }

    public void RemoveAllUI()
    {
        foreach (UI ui in _sceneDict.Values.ToList())
        {
            ui.Clear();
            if (ui) Destroy(ui.gameObject);
        }
        _sceneDict.Clear();

        while (_popupUIs.Count > 0)
        {
            RemovePopupUI();
        }
    }

    public void Clear()
    {
        RemoveAllUI();
        _stackableList.Clear();
        _root = null;
    }
}
