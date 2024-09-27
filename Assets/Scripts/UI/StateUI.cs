using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class StateUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("����â")]
    [SerializeField] private Image statusImage;
    [SerializeField] private Image turnImage;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Text statusText;


    // ����, ���, ������, ��ų ��ư ���� ����
    [Header("��ư ����")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button waitButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button skillButton;



    // ���� ���� ���� �߰�
    private Unit selectedUnit = null;
    private bool isMoveTargeting = false;

    private void Awake()
    {
        if (!statusImage || !turnImage)
        {
            Image[] images = FindObjectsOfType<Image>();

            if (!statusImage)
                statusImage = FindObjectByName(images, "Status BackGround Box") as Image;

            if (!turnImage)
                turnImage = FindObjectByName(images, "Turn BackGround Box") as Image;
        }

        if (!statusText)
        {
            Text[] texts = FindObjectsOfType<Text>();
            statusText = FindObjectByName(texts, "Status Text") as Text;
        }

        if (!tilemap)
        {
            Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
            tilemap = FindObjectByName(tilemaps, "BackGround") as Tilemap;
        }



        // �̹��� ��Ȱ��ȭ
        if (statusImage)
        {
            statusImage.gameObject.SetActive(false);
        }

        if (turnImage)
        {
            turnImage.gameObject.SetActive(false);
        }

        // ��ư �̺�Ʈ �Ҵ�
        CreateTurnImageButtons();
    }

    private Component FindObjectByName<T>(T[] objects, string name) where T : Component
    {
        foreach (var obj in objects)
        {
            if (obj.name == name)
            {
                return obj; // ���ϴ� ��ü�� ã���� ��ȯ
            }
        }
        return null; // ã�� ���ϸ� null ��ȯ
    }

    private void Update()
    {
        if (isMoveTargeting)
        {
            if (Input.GetMouseButtonDown(0))
                Move(Input.mousePosition);
        }

        if (turnImage.gameObject.activeSelf && Input.GetMouseButtonDown(1))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(turnImage.rectTransform, Input.mousePosition))
            {
                turnImage.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        statusImage.gameObject.SetActive(false);

        MovablePath(eventData.position);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ����â ����(4���ҷ� ����� ��ġ����)
        if (!turnImage.gameObject.activeSelf)
        {
            StatusLocation(eventData.position, statusImage);
            statusImage.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        statusImage.gameObject.SetActive(false);
    }

    private void StatusLocation(Vector2 cursorPos, Image uiImage)
    {
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

        // ������ ���� ������Ʈ
        SetUnitStatus(worldCursorPosition);

        Vector3Int tilePosition = tilemap.WorldToCell(worldCursorPosition);

        // Ÿ�ϸ��� ��踦 �������� Ŀ���� ��ġ�� ��� ��и鿡 �ִ��� ���
        Vector3Int tilemapCenter = tilemap.WorldToCell(tilemap.localBounds.center); // Ÿ�ϸ� �߾� ��ǥ

        // ��и��� ���
        bool isLeft = tilePosition.x < tilemapCenter.x;
        bool isTop = tilePosition.y > tilemapCenter.y;

        // ���������� ���� �̹����� ��Ÿ�� Ÿ�� ��ǥ
        Vector3 offset = OffsetSetting(isLeft, isTop);

        Vector3 targetWorldPosition = tilemap.CellToWorld(tilePosition) + offset;

        // ���� ��ġ ����
        uiImage.rectTransform.position = targetWorldPosition;
    }

    private Vector3 OffsetSetting(bool isLeft, bool isTop)
    {
        // ������ �� (�� �������� �ٸ��� ����)
        Vector3 offset = Vector3.zero;
        float tileWidth = tilemap.cellSize.x;  // Ÿ�� �ʺ�
        float tileHeight = tilemap.cellSize.y; // Ÿ�� ����
        float margin = 0.5f; // �̹����� �浹ü ������ ����

        // �»�� (top-left)
        if (isLeft && isTop)
        {
            offset = new Vector3(tileWidth * 2 + margin, tileHeight / 2 - margin, 0);
        }
        // ���� (top-right)
        else if (!isLeft && isTop)
        {
            offset = new Vector3(-tileWidth - margin, tileHeight / 2 - margin, 0);
        }
        // ���ϴ� (bottom-left)
        else if (isLeft && !isTop)
        {
            offset = new Vector3(tileWidth*2 + margin, tileHeight, 0);
        }
        // ���ϴ� (bottom-right)
        else if (!isLeft && !isTop)
        {
            offset = new Vector3(-tileWidth - margin, tileHeight, 0);
        }

        return offset;
    }


    private void SetUnitStatus(Vector3 worldCursorPosition)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);

        if (hitCollider)
        {
            Unit unit = hitCollider.GetComponent<Unit>();

            if (unit)
            {
                UpdateStaus(unit);
            }
        }
    }

    private void UpdateStaus(Unit unit)
    {
        statusText.text = unit.GetStatusName();
    }

    private void CreateTurnImageButtons()
    {
        // ��ư �̺�Ʈ �߰� (���⼱ �����ϰ� Debug.Log�� ����)
        attackButton.onClick.AddListener(() => Debug.Log("Attack"));
        waitButton.onClick.AddListener(() => Debug.Log("Wait"));
        itemButton.onClick.AddListener(() => Debug.Log("Item"));
        skillButton.onClick.AddListener(() => Debug.Log("Skill"));
    }

    private void MovablePath(Vector2 cursorPos)
    {
        // Ŀ�� ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;


        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);
        //Debug.Log($" hitCollider {hitCollider.name}");

        // world ��ǥ�� ������ Ÿ�� ��ǥ�� ��ȯ
        Vector3Int tilePosition = tilemap.WorldToCell(worldCursorPosition);
        tilePosition = new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), tilePosition.z);


        if (hitCollider)
        {
            Unit newSelectedUnit = hitCollider.GetComponent<Unit>();

            // ������ ������ ������ �־��ٸ� ���� ����
            if (selectedUnit)
            {
                selectedUnit.OnMoveComplete -= ShowTurnImage;
            }

            selectedUnit = newSelectedUnit;
            selectedUnit.OnMoveComplete += ShowTurnImage;  // �̺�Ʈ ����

            if (selectedUnit)
            {
                // ������ �̵� ������ ������ ǥ�õǾ� ���� �ʴٸ� ǥ��
                if (!isMoveTargeting)
                {
                    selectedUnit.PathSearch(tilePosition);
                    isMoveTargeting = true; // �̵� ��� ��� Ȱ��ȭ
                }
                // �̹� �̵� ������ ������ ǥ�õ� ���
                else
                {
                    // �̵� ���� ���� ��Ž��
                    selectedUnit.PathSearch(tilePosition);
                }
            }
        }
    }

    private void Move(Vector2 cursorPos)
    {
        // Ŀ�� ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);

        // world ��ǥ�� ������ Ÿ�� ��ǥ�� ��ȯ
        Vector3Int tilePosition = tilemap.WorldToCell(worldCursorPosition);
        tilePosition = new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), tilePosition.z);

        if (isMoveTargeting)
        {
            if (selectedUnit && selectedUnit.GetActionPoint() > 0)
            {
                // �̵� ������ Ÿ������ Ȯ��
                if (selectedUnit.GetvalidMoveTiles().Contains(tilePosition))
                {
                    selectedUnit.MoveTo(tilePosition);
                    StatusLocation(cursorPos, turnImage);   // �ൿâ ��ġ ����
                }
            }

        }
    }

    // �� �̹����� ǥ���ϴ� �޼���
    private void ShowTurnImage()
    {
        // �� �̹����� ǥ��
        turnImage.gameObject.SetActive(true);
        Debug.Log("�� �̹����� ǥ�õ˴ϴ�.");
    }

    public void StartTurn()
    {
        // �� ���� �� �ʿ��� ���� �߰�
        Debug.Log($"{name}�� ���Դϴ�.");
    }
}
