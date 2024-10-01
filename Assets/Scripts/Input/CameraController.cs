using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [Header("ī�޶� ����")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraSpeed = 5f; 
    [SerializeField] private float edgeThreshold = 50f;
    [SerializeField] private Tilemap tilemap;

    [Header("�� ����")]
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


        // Ÿ�ϸ� ��� ���
        BoundsInt bounds = tilemap.cellBounds;
        minBounds = (Vector2Int)bounds.min;
        maxBounds = (Vector2Int)bounds.max;

        // ī�޶� ȭ�� ũ�� ���
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // ���콺 Ŀ���� ȭ�� �ȿ� ���α�
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Update()
    {
        // ���콺 �����ڸ� �̵�
        CameraEdgeMovement();

        // ���콺 �巡�׷� ī�޶� �̵�
        DragCameraMovement();

        // ī�޶� ��ġ�� �� ��� ���� ����
        ClampCameraPosition();

        // ī�޶� ����
        CameraZoom();
    }

    // ȭ�� �����ڸ��� ���콺�� ������ ī�޶� �̵�
    private void CameraEdgeMovement()
    {
        Vector3 cameraPosition = virtualCamera.transform.position;
        Vector3 mousePosition = Input.mousePosition;

        // ���� ��
        if (mousePosition.x < edgeThreshold)
        {
            cameraPosition.x -= cameraSpeed * Time.deltaTime;
        }
        // ������ ��
        if (mousePosition.x > Screen.width - edgeThreshold)
        {
            cameraPosition.x += cameraSpeed * Time.deltaTime;
        }
        // �Ʒ� ��
        if (mousePosition.y < edgeThreshold)
        {
            cameraPosition.y -= cameraSpeed * Time.deltaTime;
        }
        // ���� ��
        if (mousePosition.y > Screen.height - edgeThreshold)
        {
            cameraPosition.y += cameraSpeed * Time.deltaTime;
        }

        virtualCamera.transform.position = cameraPosition;
    }

    // ���콺 �巡�׷� ī�޶� �̵�
    private void DragCameraMovement()
    {
        // ���콺 ���� ��ư�� ���� ���¿��� �巡���ϸ� ī�޶� �̵�
        if (Input.GetMouseButtonDown(0))
        {
            // �巡�� ������ ����
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            virtualCamera.transform.position += difference;
        }
    }

    // ī�޶��� ��ġ�� �� ��� ���� ����
    private void ClampCameraPosition()
    {
        Vector3 cameraPosition = virtualCamera.transform.position;

        // �� ��踦 ����� �ʵ��� ī�޶� ��ġ ����
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

            // �ּ�, �ִ� �� ������ ����
            virtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // ����/�ܾƿ��� �� ī�޶� ũ�Ⱑ ���ϹǷ� ��� ����
            halfHeight = virtualCamera.m_Lens.OrthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
    }
}
