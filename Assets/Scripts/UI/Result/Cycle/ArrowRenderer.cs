using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArrowRenderer : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private RectTransform lineRect;
    [SerializeField] private Image lineImage;
    [SerializeField] private RectTransform headRect;
    [SerializeField] private Image headImage;
    [SerializeField] private Button loopCountButton;
    [SerializeField] private TextMeshProUGUI loopCountText;
    [SerializeField] private float lineWidth = 12f;
    [SerializeField] private float headSize = 30f;
    [SerializeField] private float countSize = 24f;
    [SerializeField] private float textSpacing = 2f;

    #endregion

    #region Private Fields

    private CycleSlot _startSlot;
    private CycleSlot _endSlot;
    private RectTransform _parentRT;
    private RectTransform _startRT;
    private RectTransform _endRT;
    private RectTransform _loopCountBtnRT;
    
    private Vector2 _lastStartPos;
    private Vector2 _lastEndPos;

    #endregion

    #region Public Properties

    public string ArrowId { get; private set; }
    public int LoopCount { get; private set; } = 1;
    
    public float LineWidth => lineWidth;
    public float CountSize => countSize;
    public float TextSpacing => textSpacing;
    public RectTransform CountButtonRT => _loopCountBtnRT;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (loopCountButton != null)
        {
            loopCountButton.onClick.AddListener(OnLoopCountClicked);
            _loopCountBtnRT = loopCountButton.GetComponent<RectTransform>();
        }
    }

    private void LateUpdate()
    {
        UpdatePositions();
    }

    #endregion

    #region Public Methods

    public void Init(string id, CycleSlot startSlot, CycleSlot endSlot, Color color, int previousLoopCount)
    {
        ArrowId = id;
        LoopCount = previousLoopCount;
        
        if (lineImage != null)
        {
            lineImage.color = color;
        }
        
        if (headImage != null)
        {
            headImage.color = color;
        }
        
        if(loopCountButton != null)
        {
            loopCountButton.GetComponent<RectTransform>().sizeDelta = new(countSize, countSize);
        }

        if (loopCountText != null)
        {
            loopCountText.color = color;
        }
        
        UpdateLoopCountText();

        _startSlot = startSlot;
        _endSlot = endSlot;
        _parentRT = GetComponent<RectTransform>();
        _startRT = startSlot.IconRect;
        _endRT = endSlot.IconRect;
        
        UpdatePositions();
    }

    #endregion

    #region Private Methods

    private void UpdatePositions()
    {
        if (_startRT == null || _endRT == null || _parentRT == null)
        {
            return;
        }

        Vector2 startPos = _parentRT.InverseTransformPoint(_startRT.TransformPoint(_startRT.rect.center));
        Vector2 endPos = _parentRT.InverseTransformPoint(_endRT.TransformPoint(_endRT.rect.center));

        if (startPos == _lastStartPos && endPos == _lastEndPos)
        {
            return;
        }

        _lastStartPos = startPos;
        _lastEndPos = endPos;

        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;
        
        if (distance < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (lineRect != null)
        {
            lineRect.localPosition = startPos + dir * 0.5f;
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);
            lineRect.sizeDelta = new Vector2(distance, lineWidth);
        }

        if (headRect != null)
        {
            headRect.localPosition = endPos;
            headRect.localRotation = Quaternion.Euler(0, 0, angle);
            headRect.sizeDelta = new Vector2(headSize, headSize);
        }

        if (_loopCountBtnRT != null)
        {
            Vector2 normal = new Vector2(-dir.y, dir.x).normalized;
            
            // 각도와 무관하게 텍스트가 항상 선명 상단(화면 위쪽)에 위치하도록 Y축 보정
            if (normal.y < 0)
            {
                normal = -normal;
            }
            
            float dynamicOffset = (lineWidth * 0.5f) + (countSize * 0.5f) + textSpacing;
            _loopCountBtnRT.localPosition = startPos + dir * 0.5f + normal * dynamicOffset;
            _loopCountBtnRT.localRotation = Quaternion.identity;
        }
    }

    private void OnLoopCountClicked()
    {
        LoopCount++;
        if (LoopCount > 9)
        {
            LoopCount = 1;
        }
        
        UpdateLoopCountText();
    }

    private void UpdateLoopCountText()
    {
        if (loopCountText != null)
        {
            loopCountText.text = LoopCount.ToString();
        }
    }

    #endregion
}
