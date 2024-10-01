using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraSpeed = 5f; 
    [SerializeField] private float edgeThreshold = 50f;
    [SerializeField] private Tilemap tilemap;

    [Header("줌 설정")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;  
    [SerializeField] private float maxZoom = 10f; 

    private Vector2Int minBounds;  
    private Vector2Int maxBounds;  
    private float halfHeight;
    private float halfWidth;
    private Camera cam;
    private Vector3 dragOrigin;

    private CinemachineFramingTransposer zoomTransposer;

    private void Start()
    {
        cam = Camera.main;
        zoomTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();


        // 타일맵 경계 계산
        BoundsInt bounds = tilemap.cellBounds;
        minBounds = (Vector2Int)bounds.min;
        maxBounds = (Vector2Int)bounds.max;

        // 카메라 화면 크기 계산
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // 마우스 커서를 화면 안에 가두기
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Update()
    {
        // 마우스 가장자리 이동
        CameraEdgeMovement();

        // 마우스 드래그로 카메라 이동
        DragCameraMovement();

        // 카메라 위치를 맵 경계 내로 제한
        ClampCameraPosition();

        // 카메라 줌인
        CameraZoom();
    }

    // 화면 가장자리에 마우스가 있으면 카메라가 이동
    private void CameraEdgeMovement()
    {
        Vector3 cameraPosition = virtualCamera.transform.position;
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

        virtualCamera.transform.position = cameraPosition;
    }

    // 마우스 드래그로 카메라 이동
    private void DragCameraMovement()
    {
        // 마우스 왼쪽 버튼을 누른 상태에서 드래그하면 카메라 이동
        if (Input.GetMouseButtonDown(0))
        {
            // 드래그 시작점 설정
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            virtualCamera.transform.position += difference;
        }
    }

    // 카메라의 위치를 맵 경계 내로 제한
    private void ClampCameraPosition()
    {
        Vector3 cameraPosition = virtualCamera.transform.position;

        // 맵 경계를 벗어나지 않도록 카메라 위치 제한
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        virtualCamera.transform.position = cameraPosition;
    }

    private void CameraZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0f)
        {
            float currentZoom = virtualCamera.m_Lens.OrthographicSize;
            currentZoom -= scrollData * zoomSpeed;

            // 최소, 최대 줌 값으로 제한
            virtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // 줌인/줌아웃할 때 카메라 크기가 변하므로 경계 재계산
            halfHeight = virtualCamera.m_Lens.OrthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
    }
}
