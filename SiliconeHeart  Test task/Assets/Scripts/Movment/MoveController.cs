using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class MoveController : MonoBehaviour
{
    public static event Action<Vector3, int> SavePosicion;

    [SerializeField] 
    private Tilemap tilemap;
    [SerializeField] 
    private int sizeInTiles;
    [SerializeField]
    public Vector2 TopLeftTail;
    [SerializeField]
    public Vector2 bottomRightTail;

    private PlayerInputAction _controls;
    private Camera _cam;



    private void Awake()
    {
        _cam = Camera.main;
        _controls = new PlayerInputAction(); 
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
        _controls.Player.MovmentKeyboard.performed += OnMoveKeyboard;
        _controls.Player.MovmentKeyboard.canceled += OnMoveKeyboard;

        _controls.Player.MovmentMouse.performed += OnPointMouse;
        _controls.Player.MovmentMouse.canceled += OnPointMouse;

        _controls.Player.Stap.canceled += OnClick;
        _controls.Player.Stap.canceled += OnClick;
    }

    private void OnDisable()
    {
        _controls.Player.MovmentKeyboard.performed -= OnMoveKeyboard;
        _controls.Player.MovmentKeyboard.canceled -= OnMoveKeyboard;

        _controls.Player.MovmentMouse.performed -= OnPointMouse;
        _controls.Player.MovmentMouse.canceled -= OnPointMouse;

        _controls.Player.Stap.canceled -= OnClick;
        _controls.Player.Stap.canceled -= OnClick;
        _controls.Player.Disable();
    }

    private void OnMoveKeyboard(InputAction.CallbackContext ctx)
    {
        transform.position = CheckNextPoss(transform.position + (Vector3)ctx.ReadValue<Vector2>());
    }

    private void OnPointMouse(InputAction.CallbackContext ctx)
    {

        Vector2 screenPos = ctx.ReadValue<Vector2>();
        float zDistance = Mathf.Abs(_cam.transform.position.z - transform.position.z);
        Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, zDistance);
        Vector3 worldPos = _cam.ScreenToWorldPoint(screenPoint);
        worldPos.z = transform.position.z;

        Vector3Int cell = tilemap.WorldToCell(worldPos);
        Vector3 center = tilemap.GetCellCenterWorld(cell) + Vector3.back;

        transform.position = CheckNextPoss(center);
    }
    private Vector3 CheckNextPoss(Vector3 CurrentPos)
    {
        Vector3 newPos = CurrentPos;

        newPos.x = Mathf.Clamp(newPos.x, TopLeftTail.x, bottomRightTail.x);
        newPos.y = Mathf.Clamp(newPos.y, bottomRightTail.y, TopLeftTail.y);

        return newPos;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        if (IsSquareAreaFree(transform.position, sizeInTiles))
        {
            SavePosicion.Invoke(transform.position, sizeInTiles/2);
            Destroy(gameObject.GetComponent<MoveController>());
        }
        else
        {
            Debug.Log("Ќельз€ строить здесь Ч место зан€то.");
        }
    }

    // ¬озвращает список клеток sizeTiles x sizeTiles, центрированных в worldPos
    private List<Vector3Int> GetSquareCellsAtWorldPos(Vector3 worldPos, int sizeTiles)
    {
        var cells = new List<Vector3Int>();
        Vector3Int centerCell = tilemap.WorldToCell(worldPos);
        int half = sizeTiles / 2;
        int startX = centerCell.x - half;
        int startY = centerCell.y - half;

        for (int y = startY; y < startY + sizeTiles; y++)
            for (int x = startX; x < startX + sizeTiles; x++)
                cells.Add(new Vector3Int(x, y, centerCell.z));

        return cells;
    }

    // ѕровер€ет, свободна ли квадратна€ область sizeTiles x sizeTiles, центрированна€ в worldPos.
    private bool IsSquareAreaFree(Vector3 worldPos, int sizeTiles)
    {
        if (sizeTiles <= 0) sizeTiles = 1;
        Vector3 cellWorldSize = tilemap.cellSize;
        const float boxScale = 0.9f; // немного меньше клетки, чтобы не цепл€ть соседние границы

        var cells = GetSquareCellsAtWorldPos(worldPos, sizeTiles);

        foreach (var cell in cells)
        {
            // ѕроверка инстанцированного объекта Tilemap (если используетс€)
            GameObject inst = tilemap.GetInstantiatedObject(cell);
            if (inst != null && inst != this.gameObject) return false;

            // ‘изическа€ проверка по центру клетки
            Vector2 center = tilemap.GetCellCenterWorld(cell);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, cellWorldSize * boxScale, 0f, LayerMask.GetMask("Houses"));
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null && hits[i].gameObject != this.gameObject)
                    return false;
            }
        }

        return true;
    }
}
