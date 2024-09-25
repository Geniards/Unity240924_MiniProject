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
    [SerializeField] private Image stateImage;
    [SerializeField] private Image turnImage;
    [SerializeField] private Tilemap tilemap;

    // ����, ���, ������, ��ų ��ư ���� ����
    [Header("��ư ����")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button waitButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button skillButton;


    private void Awake()
    {
        if (FindObjectOfType<Image>().name == "State BackGround Box")
        {
            stateImage = FindObjectOfType<Image>();
            stateImage.gameObject.SetActive(false);
        }

        if (FindObjectOfType<Image>().name == "Turn BackGround Box")
        {
            turnImage = FindObjectOfType<Image>();
            turnImage.gameObject.SetActive(false);

            // ��ư ���� �� �Ҵ�
            CreateTurnImageButtons();
        }

        if (FindObjectOfType<Tilemap>().name == "BackGround")
        {
            tilemap = FindObjectOfType<Tilemap>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"eventData : {eventData.position}");
        Debug.Log($"mousePosion : {Input.mousePosition}");
        stateImage.gameObject.SetActive(false);

        // Turn Image�� Ŭ������ ���� �ٸ� ���� Ŭ���ϸ� Turn Image�� ��
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
        StatusLocation(eventData.position, stateImage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"mouseExit the Pos");
        stateImage.gameObject.SetActive(false);
    }

    private void StatusLocation(Vector2 cursorPos, Image uiImage)
    {
        // Ŀ�� ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

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

    private void CreateTurnImageButtons()
    {
        // ��ư�� UI�� �����ϰ� �θ� turnImage�� ����
        attackButton = CreateButton("Attack", new Vector2(0, 100));
        waitButton = CreateButton("Wait", new Vector2(0, 50));
        itemButton = CreateButton("Item", new Vector2(0, 0));
        skillButton = CreateButton("Skill", new Vector2(0, -50));

        // ��ư �̺�Ʈ �߰� (���⼱ �����ϰ� Debug.Log�� ����)
        attackButton.onClick.AddListener(() => Debug.Log("Attack"));
        waitButton.onClick.AddListener(() => Debug.Log("Wait"));
        itemButton.onClick.AddListener(() => Debug.Log("Item"));
        skillButton.onClick.AddListener(() => Debug.Log("Skill"));
    }

    private Button CreateButton(string buttonText, Vector2 anchoredPosition)
    {
        // ��ư ����
        GameObject buttonObj = new GameObject(buttonText);
        buttonObj.transform.SetParent(turnImage.transform, false);

        // ��ư ������Ʈ �߰�
        Button button = buttonObj.AddComponent<Button>();
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();

        // ��ư ������ Ű��� (300x100)
        rectTransform.sizeDelta = new Vector2(300, 100);
        rectTransform.anchoredPosition = anchoredPosition; // ��ư ��ġ ����

        // ColorBlock�� �����Ͽ� ������ ȿ�� �߰�
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;  // �⺻ ����
        colors.highlightedColor = Color.yellow;  // ���� ���� (���콺 ���� ��)
        colors.pressedColor = Color.red;  // ������ �� ����
        colors.selectedColor = Color.green;  // ���õǾ��� �� ���� (���Ƿ� ����)
        colors.disabledColor = Color.gray;  // ��Ȱ��ȭ ����
        colors.colorMultiplier = 1f;  // ���� ��ȭ ��� (�Ϲ������� 1f�� ����)
        button.colors = colors;

        // �ؽ�Ʈ ����
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");  // �⺻ ��Ʈ ����
        text.color = Color.black;

        return button;
    }
}
