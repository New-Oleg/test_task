using System;
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
    private TileAreaChecker _areaChecker;
    private bool _canBuild;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        _cam = Camera.main;
        _controls = new PlayerInputAction();
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        // Создаём экземпляр помощника для проверки области
        _areaChecker = new TileAreaChecker(tilemap, gameObject);
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
        _controls.Player.MovmentKeyboard.performed += OnMoveKeyboard;
        _controls.Player.MovmentKeyboard.canceled += OnMoveKeyboard;

        _controls.Player.MovmentMouse.performed += OnPointMouse;
        _controls.Player.MovmentMouse.canceled += OnPointMouse;

        _controls.Player.Stap.canceled += OnClick;
    }

    private void Start()
    {
    }

    private void OnDisable()
    {
        _controls.Player.MovmentKeyboard.performed -= OnMoveKeyboard;
        _controls.Player.MovmentKeyboard.canceled -= OnMoveKeyboard;

        _controls.Player.MovmentMouse.performed -= OnPointMouse;
        _controls.Player.MovmentMouse.canceled -= OnPointMouse;

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

    private Vector3 CheckNextPoss(Vector3 currentPos)
    {
        Vector3 newPos = currentPos;
        newPos.x = Mathf.Clamp(newPos.x, TopLeftTail.x, bottomRightTail.x);
        newPos.y = Mathf.Clamp(newPos.y, bottomRightTail.y, TopLeftTail.y);
        return newPos;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {


        if (_canBuild) 
        {
            SavePosicion.Invoke(transform.position, sizeInTiles / 2);
            spriteRenderer.color = Color.white;
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        if (_areaChecker.IsSquareAreaFree(transform.position, sizeInTiles))
        {
            spriteRenderer.color = Color.green;
            _canBuild = true;
        }
        else
        {
            _canBuild = false;
            spriteRenderer.color = Color.red;
        }
    }
}
