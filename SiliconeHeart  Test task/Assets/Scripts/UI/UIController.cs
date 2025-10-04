using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{

    public static event Action<Vector3> OnSelectDestroyObject;

    [SerializeField]
    private RectTransform SelectHousePanel;

    public float _movePercent = 0.3f;
    public float _duration = 0.5f;

    private Vector2 _hiddenPos;
    private Vector2 _shownPos;
    private bool _isShown = false;
    private Tween _currentTween;
    private bool _onDelete;

    void Start()
    {
        _hiddenPos = SelectHousePanel.anchoredPosition;
        float moveDistance = SelectHousePanel.rect.height * _movePercent;
        _shownPos = _hiddenPos + Vector2.up * moveDistance;
    }

    public void OnChuseCreateMode()
    {
        if (_onDelete) _onDelete = false;

        _isShown = !_isShown;

        if (_currentTween != null && _currentTween.IsActive()) _currentTween.Kill();

        Vector2 target = _isShown ? _shownPos : _hiddenPos;

        _currentTween = DOTween.To(() => SelectHousePanel.anchoredPosition,
            x => SelectHousePanel.anchoredPosition = x, target, _duration)
                              .SetEase(Ease.OutCubic)
                              .SetId(SelectHousePanel);
    }

    public void SelectHouse(GameObject house)
    {
        GameObject.Instantiate(house, Vector3.zero, Quaternion.identity);
        OnChuseCreateMode();
    }

    public void OnChuseDeleteMode()
    {
        if (_isShown == true) OnChuseCreateMode();
        _onDelete = !_onDelete;
    }




    private void Update()
    {
        if (_onDelete && Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

            // один коллайдер под курсором
            Collider2D col = Physics2D.OverlapPoint(point2D);
            if (col != null)
            {
                OnSelectDestroyObject.Invoke(col.transform.position);
                Destroy(col.gameObject);
                _onDelete = false;
            }

        }
    }

}
    