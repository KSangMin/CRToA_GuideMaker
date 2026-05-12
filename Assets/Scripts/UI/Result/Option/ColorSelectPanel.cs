using UnityEngine;
using UnityEngine.UI;

public class ColorSelectPanel : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Button backgroundColorButton;
    [SerializeField] private Image backgroundColorImage;
    private RectTransform backgroundColorRect;

    [Header("Font")]
    [SerializeField] private Button fontColorButton;
    [SerializeField] private Image fontColorImage;
    private RectTransform fontColorRect;

    [Header("ColorPicker")]
    [SerializeField] private FlexibleColorPicker colorPicker;
    [SerializeField] private Button closeColorPickerButton;
    [SerializeField] private Button colorPreviewAsCloseColorPickerButton;
    [SerializeField] private ColorEventChannel onBackgroundColorChanged;
    [SerializeField] private ColorEventChannel onFontColorChanged;
    private RectTransform colorPickerRect;

    private Color _originalBackgroundColor = Util.HexToColor("#9A9A9AFF");
    private Color _curBackgroundColor;
    public Color CurBackgroundColor => _curBackgroundColor;

    private Color _originalFontColor = Color.white;
    private Color _curFontColor;
    public Color CurFontColor => _curFontColor;

    private bool curTargetisBackground = true;

    private void Awake()
    {
        backgroundColorRect = backgroundColorImage.GetComponent<RectTransform>();
        fontColorRect = fontColorImage.GetComponent<RectTransform>();
        colorPickerRect = colorPicker.GetComponent<RectTransform>();

        colorPicker.gameObject.SetActive(false);

        backgroundColorButton.onClick.AddListener(OpenBackgroundColorPicker);
        fontColorButton.onClick.AddListener(OpenFontColorPicker);
        closeColorPickerButton.onClick.AddListener(CloseColorPicker);
        colorPreviewAsCloseColorPickerButton.onClick.AddListener(CloseColorPicker);

        _curBackgroundColor = _originalBackgroundColor;
        _curFontColor = _originalFontColor;

        backgroundColorImage.color = _curBackgroundColor;
        fontColorImage.color = _curFontColor;
    }

    private void OpenBackgroundColorPicker()
    {
        curTargetisBackground = true;

        OpenColorPickerWithColor(_curBackgroundColor);
        colorPickerRect.SetPosToNearTargetTopLeft(backgroundColorRect, new(-5, 5));
    }

    private void OpenFontColorPicker()
    {
        curTargetisBackground = false;

        OpenColorPickerWithColor(_curFontColor);
        colorPickerRect.SetPosToNearTargetTopLeft(fontColorRect, new(-5, 5));
    }

    private void SetImageColor(Color color)
    {
        if (!colorPicker.gameObject.activeSelf)
        {
            return;
        }

        if(curTargetisBackground)
        {
            SetBackgroundColor(color);
        }
        else
        {
            SetFontColor(color);
        }
    }

    private void SetBackgroundColor(Color color)
    {
        _curBackgroundColor = color;
        backgroundColorImage.color = color;
        onBackgroundColorChanged.RaiseEvent(color);
    }

    private void SetFontColor(Color color)
    {
        _curFontColor = color;
        fontColorImage.color = color;
        onFontColorChanged.RaiseEvent(color);
    }

    private void OpenColorPickerWithColor(Color targetColor)
    {
        colorPicker.onColorChange.RemoveListener(SetImageColor);
        colorPicker.gameObject.SetActive(true);
        colorPicker.color = targetColor;
        colorPicker.onColorChange.AddListener(SetImageColor);
    }

    private void CloseColorPicker()
    {
        colorPicker.gameObject.SetActive(false);
    }

    public void ResetColor()
    {
        CloseColorPicker();

        SetBackgroundColor(_originalBackgroundColor);
        SetFontColor(_originalFontColor);
    }
}
