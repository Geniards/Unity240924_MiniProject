using UnityEngine;
using UnityEngine.UI;

public class TileUIManager : MonoBehaviour
{
    public static TileUIManager Instance;

    [Header("UI Elements")]
    [SerializeField] private Image background;
    [SerializeField] private Text infoText;
    [SerializeField] private Vector3 offset;

    [Header("UI_Action Elements")]
    [SerializeField] private Image actionMenu;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button attackButton;

    private Unit lastUnit;
    private Tile lastTile;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // ��ư�� Ŭ�� �̺�Ʈ ����
        endTurnButton.onClick.AddListener(OnEndTurnButtonClick);

        // UI ����
        background.gameObject.SetActive(false);
        actionMenu.gameObject.SetActive(false);
    }

    // Ÿ�� ������ ǥ���ϴ� �޼��� (������ ���� ��)
    public void ShowTileInfo(Tile tile)
    {
        if (lastTile == tile && background.gameObject.activeSelf)
            return;

        lastTile = tile;
        lastUnit = null;

        infoText.text = "Ÿ�� Ÿ��: " + tile.tileState.ToString();
        infoText.text += tile.tileState == Tile.TileState.Blocked ? "\n�̵� �Ұ���" : "\n�̵� ����";

        background.gameObject.SetActive(true);
        UpdateUIPosition(background, tile.transform.position);
    }

    // ���� ������ ǥ���ϴ� �޼��� (������ ���� ��)
    public void ShowUnitInfo(Unit unit)
    {
        if (lastUnit == unit && background.gameObject.activeSelf)
            return;

        lastUnit = unit;
        lastTile = null;

        infoText.text = "�̸�: " + unit.stats.unitName;
        infoText.text += "\nHP: " + unit.stats.hp + " / " + unit.stats.maxHp;
        infoText.text += "\n���ݷ�: " + unit.stats.atk;
        infoText.text += "\n����: " + unit.stats.def;

        background.gameObject.SetActive(true);
        UpdateUIPosition(background, unit.currentTile.transform.position);
    }

    // UI�� ��ġ�� Ÿ���� ��ġ�� �������� �����ϴ� �޼���
    private void UpdateUIPosition(Image imageBagground, Vector3 tilePosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(tilePosition);
        bool isLeft = screenPosition.x < Screen.width / 2;
        bool isTop = screenPosition.y > Screen.height / 2;

        Vector3 newPosition;
        if (isLeft && isTop)
        {
            newPosition = new Vector3(tilePosition.x + offset.x, tilePosition.y - offset.y, tilePosition.z);
        }
        else if (!isLeft && isTop)
        {
            newPosition = new Vector3(tilePosition.x - offset.x, tilePosition.y - offset.y, tilePosition.z);
        }
        else if (isLeft && !isTop)
        {
            newPosition = new Vector3(tilePosition.x + offset.x, tilePosition.y + offset.y, tilePosition.z);
        }
        else
        {
            newPosition = new Vector3(tilePosition.x - offset.x, tilePosition.y + offset.y, tilePosition.z);
        }

        imageBagground.rectTransform.position = newPosition;
    }

    // UI �����
    public void HideInfo()
    {
        background.gameObject.SetActive(false);
        lastUnit = null;
        lastTile = null;
    }

    public void ShowActionMenu(Vector3 unitPosition)
    {
        actionMenu.gameObject.SetActive(true);
        UpdateUIPosition(actionMenu, unitPosition);
    }

    public void HideActionMenu()
    {
        actionMenu.gameObject.SetActive(false);
    }

    // �� ���� ��ư Ŭ�� �� ȣ��
    private void OnEndTurnButtonClick()
    {
        TurnManager.Instance.EndAllyTurn();
    }
}
