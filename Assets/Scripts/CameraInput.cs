using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CameraInput : MonoBehaviour
{
    [Header("���콺 �̵� ����")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraSpeed = 5f;  // ī�޶� �̵� �ӵ�
    [SerializeField] private float edgeThreshold = 50f;  // �����ڸ����� ī�޶� �����̱� �����ϴ� �Ÿ� (�ȼ� ����)
    [SerializeField] private Tilemap tilemap;

    private Vector2Int minBounds;  // Ÿ�ϸ��� �ּ� ���
    private Vector2Int maxBounds;  // Ÿ�ϸ��� �ִ� ���
    private float halfHeight;
    private float halfWidth;
    private Camera cam;

    [Header("���� ���콺 ��ġ")]
    [SerializeField] private Color gizmoColor = new Color(1, 1, 1, 0.5f);
    private Vector3Int previousTilePos;




    void Start()
    {
        // ī�޶� ������Ʈ ��������
        cam = Camera.main;

        // Ÿ�ϸ��� ũ�⿡ ���� ��踦 �̿��� ī�޶��� �̵� ������ ����
        if(FindObjectOfType<Tilemap>().name  == "BackGround")
            tilemap = FindObjectOfType<Tilemap>();
        
        BoundsInt bounds = tilemap.cellBounds;

        minBounds = (Vector2Int)bounds.min;
        maxBounds = (Vector2Int)bounds.max;

        // ī�޶��� ȭ�� ũ�⸦ ��������
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // ���콺 Ŀ���� ȭ�� �ȿ� ���α�
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        // ����â
        //stateImage = GetComponent<Image>();
        //stateImage.enabled = false;
    }

    void Update()
    {
        Vector3 cameraPosition = virtualCamera.transform.position;

        // ȭ�� ���� ���콺�� ���� �� ī�޶� �̵�
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

        // ī�޶��� �̵� ������ Ÿ�ϸ� ��� ���� ����
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        // ī�޶� ��ġ ������Ʈ
        virtualCamera.transform.position = cameraPosition;

        MouseCursorController();
        CurrentCursorPos();
    }

    // ���콺 ��ġ �̵� -> ī�޶� �̵�
    private void MouseCursorController()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("���콺 ��������");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0))  // Ŭ���ϸ� �ٽ� ���
        {
            Debug.Log("���콺 ����");
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    // ���� Ŀ����ġ ǥ��
    private void CurrentCursorPos()
    {
        // ���콺�� ȭ���� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;    // 2D�̱⿡ ���� ǥ������ �ʰ� ����

        // world��ǥ -> cell��ǥ(Ÿ�� ��ǥ�� ��ȯ)
        Vector3Int tilePostion = tilemap.WorldToCell(mouseWorldPos);

        // ������ġ�� ������ġ�� ���ؼ� �ٸ���� ����
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
