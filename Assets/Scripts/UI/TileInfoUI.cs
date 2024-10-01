using UnityEngine;
using UnityEngine.UI;

public class TileInfoUI : MonoBehaviour
{
    public static TileInfoUI Instance;

    [Header("UI Elements")]
    [SerializeField] private Image background;     // UI�� ��� (������, ���� 45%)
    [SerializeField] private Text typeText;        // Ÿ�� ���� ǥ�� �ؽ�Ʈ
    [SerializeField] private Text stateText;       // �̵� ���� ���� �ؽ�Ʈ
    [SerializeField] private Vector3 offset;       // ��� offset


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // UI ��Ȱ��ȭ (ó������ UI�� ������ �ʰ�)
        background.gameObject.SetActive(false);
    }

    // Ÿ�� Ŭ�� �� UI�� �ش� Ÿ���� �� ĭ�� ǥ���ϴ� �޼���
    public void ShowTileInfo(Tile tile)
    {
        if (background.gameObject.activeSelf)
        {
            HideTileInfo();
        }
        else
        {
            // Ÿ�� ���¿� ���� UI ������Ʈ
            typeText.text = "Ÿ��: " + tile.tileState.ToString();
            stateText.text = tile.tileState == Tile.TileState.Blocked ? "�̵� �Ұ���" : "�̵� ����";

            // UI�� Ȱ��ȭ
            background.gameObject.SetActive(true);

            // Ŭ���� Ÿ���� ��ġ�� ���� UI ��ġ�� ����
            UpdateUIPosition(tile);
        }
        
    }

    // Ŭ���� Ÿ���� ��ġ�� �������� UI�� ��ġ�� �� ĭ�� ���� �����ϴ� �޼���
    private void UpdateUIPosition(Tile tile)
    {
        Vector3 tilePosition = tile.transform.position; // Ÿ���� ���� ��ǥ

        // ī�޶� ȭ���� 4�и��� �������� Ÿ�� ��ġ Ȯ��
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(tilePosition);
        bool isLeft = screenPosition.x < Screen.width / 2;
        bool isTop = screenPosition.y > Screen.height / 2;

        // ��и鿡 ���� UI ��ġ�� Ÿ���� �� ĭ�� ��ġ
        Vector3 newPosition;
        if (isLeft && isTop)
        {
            // �»��: UI�� Ÿ���� ������ �Ʒ��� ǥ��
            newPosition = new Vector3(tilePosition.x + offset.x, tilePosition.y - offset.y, tilePosition.z);
        }
        else if (!isLeft && isTop)
        {
            // ����: UI�� Ÿ���� ���� �Ʒ��� ǥ��
            newPosition = new Vector3(tilePosition.x - offset.x, tilePosition.y - offset.y, tilePosition.z);
        }
        else if (isLeft && !isTop)
        {
            // ���ϴ�: UI�� Ÿ���� ������ ���� ǥ��
            newPosition = new Vector3(tilePosition.x + offset.x, tilePosition.y + offset.y, tilePosition.z);
        }
        else
        {
            // ���ϴ�: UI�� Ÿ���� ���� ���� ǥ��
            newPosition = new Vector3(tilePosition.x - offset.x, tilePosition.y + offset.y, tilePosition.z);
        }

        // UI�� ���� ��ǥ�� �°� ��ġ ����
        background.rectTransform.position = newPosition;
    }

    // UI �����
    public void HideTileInfo()
    {
        background.gameObject.SetActive(false);
    }
}
