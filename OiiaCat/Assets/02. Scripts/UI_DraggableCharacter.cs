using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 요소 드래그 가능하게 만드는 컴포넌트
/// </summary>
public class UI_DraggableCharacter : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Canvas _canvas;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 필요한 처리
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_rectTransform != null && _canvas != null)
        {
            // 마우스/터치 이동에 따라 캐릭터 위치 변경
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 시 필요한 처리
    }
}