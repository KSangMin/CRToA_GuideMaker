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
    private RectTransform colorPickerRect;

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

        backgroundColorImage.color = Util.HexToColor("#9A9A9AFF");
        fontColorImage.color = Color.white;
    }

    private void OpenBackgroundColorPicker()
    {
        OpenColorPicker();

        colorPickerRect.SetPosToNearTargetTopLeft(backgroundColorRect, new(-5, 5));
    }

    private void OpenFontColorPicker()
    {
        OpenColorPicker();

        colorPickerRect.SetPosToNearTargetTopLeft(fontColorRect, new(-5, 5));
    }

    private void OpenColorPicker()
    {
        colorPicker.gameObject.SetActive(true);
    }

    private void CloseColorPicker()
    {
        colorPicker.gameObject.SetActive(false);
    }
}
