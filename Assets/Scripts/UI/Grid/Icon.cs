using UnityEngine;
using UnityEngine.UI;

public class Icon : MonoBehaviour
{
    private int _width = 1;
    private int _height = 1;
    private Image _icon;

    private void Awake()
    {
        _icon = GetComponent<Image>();
    }

    public void SetIcon(int w, int h, Sprite sprite)
    {
        _width = w;
        _height = h;
        _icon.sprite = sprite;

        RectTransform rect = GetComponent<RectTransform>();
        rect.localScale = new(w, h);
    }
}
