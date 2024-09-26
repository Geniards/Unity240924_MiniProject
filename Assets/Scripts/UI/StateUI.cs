using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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


    private void Awake()
    {
        if (!statusImage || !turnImage)
        {
            Image[] images = FindObjectsOfType<Image>();

            if(!statusImage)
                statusImage = FindObjectByName(images, "Status BackGround Box") as Image;

            if(!turnImage)
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

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"eventData : {eventData.position}");
        Debug.Log($"mousePosion : {Input.mousePosition}");
        statusImage.gameObject.SetActive(false);

        MovablePath(eventData.position);

        //Turn Image�� Ŭ������ ���� �ٸ� ���� Ŭ���ϸ� Turn Image�� ��
        if (turnImage.gameObject.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(turnImage.rectTransform, eventData.position))
            {
                turnImage.gameObject.SetActive(false);
            }
        }
        else if (!turnImage.gameObject.activeSelf)
        {
            StatusLocation(eventData.position, turnImage);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"mouseEnter the Pos");

        // ����â ����(4���ҷ� ����� ��ġ����)
        if(!turnImage.gameObject.activeSelf)
            StatusLocation(eventData.position, statusImage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"mouseExit the Pos");
        statusImage.gameObject.SetActive(false);
    }

    private void StatusLocation(Vector2 cursorPos, Image uiImage)
    {
        // Ŀ�� ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

        // ������ ���� ������Ʈ
        SetUnitStatus(worldCursorPosition);

        // world��ǥ -> cell��ǥ(Ÿ�� ��ǥ�� ��ȯ)
        Vector3Int tilePostion = tilemap.WorldToCell(worldCursorPosition);

        // Ÿ�ϸ��� ��踦 �������� Ŀ���� ��ġ�� ��� ��и鿡 �ִ��� ���
        Vector3Int tilemapCenter = tilemap.WorldToCell(tilemap.localBounds.center); // Ÿ�ϸ� �߾� ��ǥ

        // ��и��� ���
        bool isLeft = tilePostion.x < tilemapCenter.x;
        bool isTop = tilePostion.y > tilemapCenter.y;

        // ���������� ���� �̹����� ��Ÿ�� Ÿ�� ��ǥ
        Vector3Int targetTilePosition = tilePostion + OffsetSetting(isLeft, isTop);

        // Ÿ�ϸ� ��ǥ -> ���� ��ǥ
        Vector3 targetWorldPosition = tilemap.CellToWorld(targetTilePosition);
        uiImage.transform.position = targetWorldPosition;

        uiImage.gameObject.SetActive(true);
    }

    private Vector3Int OffsetSetting(bool isLeft, bool isTop)
    {
        // ������ �� (�� �������� �ٸ��� ����)
        Vector3Int offset = Vector3Int.zero;

        // �»�� (top-left)
        if (isLeft && isTop)
        {
            offset = new Vector3Int(2, 0, 0);
        }
        // ���� (top-right)
        else if (!isLeft && isTop)
        {
            offset = new Vector3Int(-1, 0, 0);
        }
        // ���ϴ� (bottom-left)
        else if (isLeft && !isTop)
        {
            offset = new Vector3Int(2, 1, 0);
        }
        // ���ϴ� (bottom-right)
        else if (!isLeft && !isTop)
        {
            offset = new Vector3Int(-1, 1, 0);
        }

        return offset;  
    }

    private void SetUnitStatus(Vector3 worldCursorPosition)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);

        if(hitCollider)
        {
            Unit unit = hitCollider.GetComponent<Unit>();
            
            if(unit)
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
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

        Vector3Int tilePostion = tilemap.WorldToCell(worldCursorPosition);

        Debug.Log(tilePostion);

        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);

        if (hitCollider)
        {
            Unit unit = hitCollider.GetComponent<Unit>();

            if (unit)
            {
                unit.PathSearch(tilePostion);
            }
        }
    }
}
