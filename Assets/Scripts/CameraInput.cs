using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CameraInput : MonoBehaviour
{
    [Header("마우스 이동 세팅")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraSpeed = 5f;  // 카메라 이동 속도
    [SerializeField] private float edgeThreshold = 50f;  // 가장자리에서 카메라가 움직이기 시작하는 거리 (픽셀 단위)
    [SerializeField] private Tilemap tilemap;

    private Vector2Int minBounds;  // 타일맵의 최소 경계
    private Vector2Int maxBounds;  // 타일맵의 최대 경계
    private float halfHeight;
    private float halfWidth;
    private Camera cam;

    [Header("현재 마우스 위치")]
    [SerializeField] private Color gizmoColor = new Color(1, 1, 1, 0.5f);
    private Vector3Int previousTilePos;




    void Start()
    {
        // 카메라 컴포넌트 가져오기
        cam = Camera.main;

        // 타일맵의 크기에 따른 경계를 이용해 카메라의 이동 범위를 설정
        if(FindObjectOfType<Tilemap>().name  == "BackGround")
            tilemap = FindObjectOfType<Tilemap>();
        
        BoundsInt bounds = tilemap.cellBounds;

        minBounds = (Vector2Int)bounds.min;
        maxBounds = (Vector2Int)bounds.max;

        // 카메라의 화면 크기를 가져오기
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // 마우스 커서를 화면 안에 가두기
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        // 상태창
        //stateImage = GetComponent<Image>();
        //stateImage.enabled = false;
    }

    void Update()
    {
        Vector3 cameraPosition = virtualCamera.transform.position;

        // 화면 끝에 마우스가 있을 때 카메라 이동
        Vector3 mousePosition = Input.mousePosition;

        // 왼쪽 끝
        if (mousePosition.x < edgeThreshold)
        {
            cameraPosition.x -= cameraSpeed * Time.deltaTime;
        }
        // 오른쪽 끝
        if (mousePosition.x > Screen.width - edgeThreshold)
        {
            cameraPosition.x += cameraSpeed * Time.deltaTime;
        }
        // 아래 끝
        if (mousePosition.y < edgeThreshold)
        {
            cameraPosition.y -= cameraSpeed * Time.deltaTime;
        }
        // 위쪽 끝
        if (mousePosition.y > Screen.height - edgeThreshold)
        {
            cameraPosition.y += cameraSpeed * Time.deltaTime;
        }

        // 카메라의 이동 범위를 타일맵 경계 내로 제한
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        // 카메라 위치 업데이트
        virtualCamera.transform.position = cameraPosition;

        MouseCursorController();
        CurrentCursorPos();
    }

    // 마우스 위치 이동 -> 카메라 이동
    private void MouseCursorController()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("마우스 고정해제");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0))  // 클릭하면 다시 잠금
        {
            Debug.Log("마우스 고정");
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    // 현재 커서위치 표시
    private void CurrentCursorPos()
    {
        // 마우스의 화면의 좌표를 월드 좌표로 전환
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;    // 2D이기에 따로 표기하지 않고 리셋

        // world좌표 -> cell좌표(타일 좌표로 전환)
        Vector3Int tilePostion = tilemap.WorldToCell(mouseWorldPos);

        // 이전위치랑 현재위치를 비교해서 다를경우 갱신
        if(tilePostion != previousTilePos)
            previousTilePos = tilePostion;
    }

    private void StateUI()
    {
        //stateImage.transform.position
    }


    private void OnDrawGizmos()
    {
        if (!tilemap) return;

        Vector3 tileWorldPos = tilemap.GetCellCenterWorld(previousTilePos);

        Gizmos.color = gizmoColor;
        
        Gizmos.DrawCube(tileWorldPos, tilemap.cellSize);
    }
}
