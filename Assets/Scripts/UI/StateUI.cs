using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class StateUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("상태창")]
    [SerializeField] private Image statusImage;
    [SerializeField] private Image turnImage;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Text statusText;

    // 공격, 대기, 아이템, 스킬 버튼 변수 선언
    [Header("버튼 세팅")]
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

        // 이미지 비활성화
        if (statusImage)
        {
            statusImage.gameObject.SetActive(false);
        }

        if (turnImage)
        {
            turnImage.gameObject.SetActive(false);
        }

        // 버튼 이벤트 할당
        CreateTurnImageButtons();
    }

    private Component FindObjectByName<T>(T[] objects, string name) where T : Component
    {
        foreach (var obj in objects)
        {
            if (obj.name == name)
            {
                return obj; // 원하는 객체를 찾으면 반환
            }
        }
        return null; // 찾지 못하면 null 반환
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"eventData : {eventData.position}");
        Debug.Log($"mousePosion : {Input.mousePosition}");
        statusImage.gameObject.SetActive(false);

        MovablePath(eventData.position);

        //Turn Image를 클릭하지 않은 다른 곳을 클릭하면 Turn Image를 끔
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

        // 상태창 띄우기(4분할로 나누어서 위치변경)
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
        // 커서 위치를 월드 좌표로 변환
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

        // 유닛의 상태 업데이트
        SetUnitStatus(worldCursorPosition);

        // world좌표 -> cell좌표(타일 좌표로 전환)
        Vector3Int tilePostion = tilemap.WorldToCell(worldCursorPosition);

        // 타일맵의 경계를 기준으로 커서의 위치가 어느 사분면에 있는지 계산
        Vector3Int tilemapCenter = tilemap.WorldToCell(tilemap.localBounds.center); // 타일맵 중앙 좌표

        // 사분면을 계산
        bool isLeft = tilePostion.x < tilemapCenter.x;
        bool isTop = tilePostion.y > tilemapCenter.y;

        // 최종적으로 상태 이미지를 나타낼 타일 좌표
        Vector3Int targetTilePosition = tilePostion + OffsetSetting(isLeft, isTop);

        // 타일맵 좌표 -> 월드 좌표
        Vector3 targetWorldPosition = tilemap.CellToWorld(targetTilePosition);
        uiImage.transform.position = targetWorldPosition;

        uiImage.gameObject.SetActive(true);
    }

    private Vector3Int OffsetSetting(bool isLeft, bool isTop)
    {
        // 오프셋 값 (각 구역마다 다르게 설정)
        Vector3Int offset = Vector3Int.zero;

        // 좌상단 (top-left)
        if (isLeft && isTop)
        {
            offset = new Vector3Int(2, 0, 0);
        }
        // 우상단 (top-right)
        else if (!isLeft && isTop)
        {
            offset = new Vector3Int(-1, 0, 0);
        }
        // 좌하단 (bottom-left)
        else if (isLeft && !isTop)
        {
            offset = new Vector3Int(2, 1, 0);
        }
        // 우하단 (bottom-right)
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
        // 버튼 이벤트 추가 (여기선 간단하게 Debug.Log로 예시)
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
