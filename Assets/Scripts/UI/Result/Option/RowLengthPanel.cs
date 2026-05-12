using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RowLengthPanel : MonoBehaviour
{
    [SerializeField] private Slider rowLengthSlider;
    [SerializeField] private TextMeshProUGUI minValueText;
    [SerializeField] private TextMeshProUGUI maxValueText;
    [SerializeField] private TextMeshProUGUI curValueText;
    [SerializeField] private IntEventChannel onRowLengthChangedEvent;

    private int _minValue = 4;
    private int _maxValue = 12;
    private int _originalValue = 8;
    private int _curValue;
    public int CurValue => _curValue;

    private void Awake()
    {
        _curValue = _originalValue;

        rowLengthSlider.onValueChanged.AddListener(OnSliderValueChanged);

        ResetSlider();
    }

    private void ResetSlider()
    {
        rowLengthSlider.minValue = _minValue;
        rowLengthSlider.maxValue = _maxValue;
        rowLengthSlider.value = _curValue;
    }

    private void OnSliderValueChanged(float value)
    {
        int newValue = Mathf.Clamp((int)value, _minValue, _maxValue);

        if(newValue != _curValue)
        {
            _curValue = newValue;
            SetCurValueText();
            onRowLengthChangedEvent.RaiseEvent(_curValue);
        }
    }

    private void SetCurValueText()
    {
        curValueText.SetText($"{_curValue}");
    }

    public void ResetLength()
    {
        _curValue = _originalValue;
        SetSliderToCurValue();
        SetCurValueText();
        onRowLengthChangedEvent.RaiseEvent(_curValue);
    }

    private void SetSliderToCurValue()
    {
        rowLengthSlider.value = _curValue;
    }
}
