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



    // 상태 관리 변수 추가
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
        // 상태창 띄우기(4분할로 나누어서 위치변경)
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

        // 유닛의 상태 업데이트
        SetUnitStatus(worldCursorPosition);

        Vector3Int tilePosition = tilemap.WorldToCell(worldCursorPosition);

        // 타일맵의 경계를 기준으로 커서의 위치가 어느 사분면에 있는지 계산
        Vector3Int tilemapCenter = tilemap.WorldToCell(tilemap.localBounds.center); // 타일맵 중앙 좌표

        // 사분면을 계산
        bool isLeft = tilePosition.x < tilemapCenter.x;
        bool isTop = tilePosition.y > tilemapCenter.y;

        // 최종적으로 상태 이미지를 나타낼 타일 좌표
        Vector3 offset = OffsetSetting(isLeft, isTop);

        Vector3 targetWorldPosition = tilemap.CellToWorld(tilePosition) + offset;

        // 최종 위치 설정
        uiImage.rectTransform.position = targetWorldPosition;
    }

    private Vector3 OffsetSetting(bool isLeft, bool isTop)
    {
        // 오프셋 값 (각 구역마다 다르게 설정)
        Vector3 offset = Vector3.zero;
        float tileWidth = tilemap.cellSize.x;  // 타일 너비
        float tileHeight = tilemap.cellSize.y; // 타일 높이
        float margin = 0.5f; // 이미지와 충돌체 사이의 간격

        // 좌상단 (top-left)
        if (isLeft && isTop)
        {
            offset = new Vector3(tileWidth * 2 + margin, tileHeight / 2 - margin, 0);
        }
        // 우상단 (top-right)
        else if (!isLeft && isTop)
        {
            offset = new Vector3(-tileWidth - margin, tileHeight / 2 - margin, 0);
        }
        // 좌하단 (bottom-left)
        else if (isLeft && !isTop)
        {
            offset = new Vector3(tileWidth*2 + margin, tileHeight, 0);
        }
        // 우하단 (bottom-right)
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
        // 버튼 이벤트 추가 (여기선 간단하게 Debug.Log로 예시)
        attackButton.onClick.AddListener(() => Debug.Log("Attack"));
        waitButton.onClick.AddListener(() => Debug.Log("Wait"));
        itemButton.onClick.AddListener(() => Debug.Log("Item"));
        skillButton.onClick.AddListener(() => Debug.Log("Skill"));
    }

    private void MovablePath(Vector2 cursorPos)
    {
        // 커서 위치를 월드 좌표로 변환
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;


        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);
        //Debug.Log($" hitCollider {hitCollider.name}");

        // world 좌표를 정수형 타일 좌표로 변환
        Vector3Int tilePosition = tilemap.WorldToCell(worldCursorPosition);
        tilePosition = new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), tilePosition.z);


        if (hitCollider)
        {
            Unit newSelectedUnit = hitCollider.GetComponent<Unit>();

            // 이전에 선택한 유닛이 있었다면 구독 해제
            if (selectedUnit)
            {
                selectedUnit.OnMoveComplete -= ShowTurnImage;
            }

            selectedUnit = newSelectedUnit;
            selectedUnit.OnMoveComplete += ShowTurnImage;  // 이벤트 구독

            if (selectedUnit)
            {
                // 이전에 이동 가능한 범위가 표시되어 있지 않다면 표시
                if (!isMoveTargeting)
                {
                    selectedUnit.PathSearch(tilePosition);
                    isMoveTargeting = true; // 이동 대상 모드 활성화
                }
                // 이미 이동 가능한 범위가 표시된 경우
                else
                {
                    // 이동 가능 범위 재탐색
                    selectedUnit.PathSearch(tilePosition);
                }
            }
        }
    }

    private void Move(Vector2 cursorPos)
    {
        // 커서 위치를 월드 좌표로 변환
        Vector3 worldCursorPosition = Camera.main.ScreenToWorldPoint(cursorPos);
        worldCursorPosition.z = 0;

        Collider2D hitCollider = Physics2D.OverlapPoint(worldCursorPosition);

        // world 좌표를 정수형 타일 좌표로 변환
        Vector3Int tilePosition = tilemap.WorldToCell(worldCursorPosition);
        tilePosition = new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), tilePosition.z);

        if (isMoveTargeting)
        {
            if (selectedUnit && selectedUnit.GetActionPoint() > 0)
            {
                // 이동 가능한 타일인지 확인
                if (selectedUnit.GetvalidMoveTiles().Contains(tilePosition))
                {
                    selectedUnit.MoveTo(tilePosition);
                    StatusLocation(cursorPos, turnImage);   // 행동창 위치 세팅
                }
            }

        }
    }

    // 턴 이미지를 표시하는 메서드
    private void ShowTurnImage()
    {
        // 턴 이미지를 표시
        turnImage.gameObject.SetActive(true);
        Debug.Log("턴 이미지가 표시됩니다.");
    }

    public void StartTurn()
    {
        // 턴 시작 시 필요한 로직 추가
        Debug.Log($"{name}의 턴입니다.");
    }
}
