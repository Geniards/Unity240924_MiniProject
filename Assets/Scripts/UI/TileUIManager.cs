using System.Collections.Generic;
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

        // UI 숨김
        background.gameObject.SetActive(false);
        actionMenu.gameObject.SetActive(false);
    }

    // 타일 정보를 표시하는 메서드 (유닛이 없을 때)
    public void ShowTileInfo(Tile tile)
    {
        if (lastTile == tile && background.gameObject.activeSelf)
            return;

        lastTile = tile;
        lastUnit = null;

        infoText.text = "타일 타입: " + tile.tileState.ToString();
        infoText.text += tile.tileState == Tile.TileState.Blocked ? "\n이동 불가능" : "\n이동 가능";

        background.gameObject.SetActive(true);
        UpdateUIPosition(background, tile.transform.position);
    }

    // 유닛 정보를 표시하는 메서드 (유닛이 있을 때)
    public void ShowUnitInfo(Unit unit)
    {
        if (lastUnit == unit && background.gameObject.activeSelf)
            return;

        lastUnit = unit;
        lastTile = null;

        infoText.text = "이름: " + unit.stats.unitName;
        infoText.text += "\nHP: " + unit.stats.hp + " / " + unit.stats.maxHp;
        infoText.text += "\n공격력: " + unit.stats.atk;
        infoText.text += "\n방어력: " + unit.stats.def;

        background.gameObject.SetActive(true);
        UpdateUIPosition(background, unit.currentTile.transform.position);
    }

    // UI의 위치를 타일의 위치를 기준으로 조정하는 메서드
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

    // UI 숨기기
    public void HideInfo()
    {
        background.gameObject.SetActive(false);
        lastUnit = null;
        lastTile = null;
    }

    public void ShowActionMenu(Vector3 unitPosition, Unit unit)
    {
        actionMenu.gameObject.SetActive(true);
        UpdateUIPosition(actionMenu, unitPosition);

        // 공격 버튼에 클릭 이벤트 추가
        attackButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(() => OnAttackButtonClick(unit));

        // 버튼에 클릭 이벤트 연결
        endTurnButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.AddListener(() => OnEndTurnButtonClick(unit));
    }

    public void HideActionMenu()
    {
        actionMenu.gameObject.SetActive(false);
    }

    // 턴 종료 버튼 클릭 시 호출
    private void OnEndTurnButtonClick(Unit unit)
    {
        TurnManager.Instance.NotifyUnitMovementFinished(unit);
        HideActionMenu();
    }
    // 공격 버튼
    private void OnAttackButtonClick(Unit unit)
    {
        Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();
        if (selectedUnit != null)
        {
            List<Tile> attackableTiles = GridManager.Instance.FindAttackableTiles(selectedUnit.currentTile, selectedUnit.stats.attackRange);
            foreach (Tile tile in attackableTiles)
            {
                tile.UpdateTileState(Tile.TileState.Attackable);  // 공격 가능 타일을 표시
            }
        }
        HideActionMenu();
        TurnManager.Instance.ChangeState(TurnManager.TurnState.UnitAttack);
    }
}
