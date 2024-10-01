using UnityEngine;
using UnityEngine.UI;

public class TileInfoUI : MonoBehaviour
{
    public static TileInfoUI Instance;

    [Header("UI Elements")]
    [SerializeField] private Image background;     // UI의 배경 (검은색, 투명도 45%)
    [SerializeField] private Text typeText;        // 타일 상태 표시 텍스트
    [SerializeField] private Text stateText;       // 이동 가능 여부 텍스트
    [SerializeField] private Vector3 offset;       // 배경 offset


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // UI 비활성화 (처음에는 UI가 보이지 않게)
        background.gameObject.SetActive(false);
    }

    // 타일 클릭 시 UI를 해당 타일의 옆 칸에 표시하는 메서드
    public void ShowTileInfo(Tile tile)
    {
        if (background.gameObject.activeSelf)
        {
            HideTileInfo();
        }
        else
        {
            // 타일 상태에 따라 UI 업데이트
            typeText.text = "타입: " + tile.tileState.ToString();
            stateText.text = tile.tileState == Tile.TileState.Blocked ? "이동 불가능" : "이동 가능";

            // UI를 활성화
            background.gameObject.SetActive(true);

            // 클릭한 타일의 위치에 따라 UI 위치를 조정
            UpdateUIPosition(tile);
        }
        
    }

    // 클릭한 타일의 위치를 기준으로 UI의 위치를 옆 칸에 맞춰 조정하는 메서드
    private void UpdateUIPosition(Tile tile)
    {
        Vector3 tilePosition = tile.transform.position; // 타일의 월드 좌표

        // 카메라 화면의 4분면을 기준으로 타일 위치 확인
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(tilePosition);
        bool isLeft = screenPosition.x < Screen.width / 2;
        bool isTop = screenPosition.y > Screen.height / 2;

        // 사분면에 따라 UI 위치를 타일의 옆 칸에 배치
        Vector3 newPosition;
        if (isLeft && isTop)
        {
            // 좌상단: UI는 타일의 오른쪽 아래로 표시
            newPosition = new Vector3(tilePosition.x + offset.x, tilePosition.y - offset.y, tilePosition.z);
        }
        else if (!isLeft && isTop)
        {
            // 우상단: UI는 타일의 왼쪽 아래로 표시
            newPosition = new Vector3(tilePosition.x - offset.x, tilePosition.y - offset.y, tilePosition.z);
        }
        else if (isLeft && !isTop)
        {
            // 좌하단: UI는 타일의 오른쪽 위로 표시
            newPosition = new Vector3(tilePosition.x + offset.x, tilePosition.y + offset.y, tilePosition.z);
        }
        else
        {
            // 우하단: UI는 타일의 왼쪽 위로 표시
            newPosition = new Vector3(tilePosition.x - offset.x, tilePosition.y + offset.y, tilePosition.z);
        }

        // UI를 월드 좌표에 맞게 위치 설정
        background.rectTransform.position = newPosition;
    }

    // UI 숨기기
    public void HideTileInfo()
    {
        background.gameObject.SetActive(false);
    }
}
