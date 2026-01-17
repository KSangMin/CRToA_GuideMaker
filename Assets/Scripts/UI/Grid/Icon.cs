using UnityEngine;
using UnityEngine.UI;

public class Icon : MonoBehaviour
{
    private int _width = 1;
    private int _height = 1;

    public void SetIcon(int w, int h, Sprite sprite)
    {
        _width = w;
        _height = h;
        GetComponent<Image>().sprite = sprite;

        RectTransform rect = GetComponent<RectTransform>();
        rect.localScale = new(w, h);
    }
}
