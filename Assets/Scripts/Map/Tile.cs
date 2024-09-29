using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color selectColor;
    [SerializeField] private EventTrigger trigger;

    
    public Tile Parent;
    public TileInfo tileInfo;
    public Character character { get; private set; }
    public bool hasCharcter;
    public SpriteRenderer tileRenderer;

    private bool isClicking;

    public void SetSelected()
    {
        tileRenderer.material.color = selectColor;
    }

    public void ReleaseSelected()
    {
        tileRenderer.material.color = new Color(1, 1, 1, 0);
    }

    public void SetTileInfo(TileInfo infoTile)
    {
        tileInfo = infoTile;
    }

    public void SetCharacter(Character ch)
    {
        character = ch;
        hasCharcter = true;
        tileInfo.tileState = TileState.UNIT;
        ch.currentStand = this;
    }

    public void ReleaseCharacter()
    {
        character = null;
        hasCharcter = false;
        tileInfo.tileState = TileState.EMPTY;

    }

    public void SetSearchInfo(TileInfo infoTile)
    {
        tileInfo.SetSearchInfo(infoTile);
    }

    public void ClearSearch()
    {
        tileInfo.ClearSearchInfo();
    }

    public void ClickTile(PointerEventData data)
    {
        Map.Instance.CheckSelectTile(this);
        isClicking = true;
    }

    // 마우스를 타일 위에 가져갔을 때
    public void EnterTile(PointerEventData data)
    {
        if (isClicking) return;

        if (hasCharcter && character)
        {
            // 캐릭터 상태창 활성화 (상태를 갱신하고 화면에 표시)
            StateUI.Instance.OnPointerEnterWithUnit(character, data);
        }
    }

    // 마우스를 타일 밖으로 나갔을 때
    public void ExitTile(PointerEventData data)
    {
        // 상태창 닫기
        StateUI.Instance.OnPointerExit(data);
    }

    // 이동 완료 후 호출하는 메서드
    public void MoveComplete()
    {
        isClicking = false;
        // 이동 후 상태창 닫기 등 추가 작업 가능
    }

    private void Awake()
    {
        if (!tileRenderer)
        {
            tileRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (!trigger)
            trigger = GetComponent<EventTrigger>();

        if (!trigger)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }

        // 마우스 클릭 이벤트 추가
        EventTrigger.Entry entryClicker = new EventTrigger.Entry();
        entryClicker.eventID = EventTriggerType.PointerClick;
        entryClicker.callback.AddListener((data) => ClickTile((PointerEventData)data));
        trigger.triggers.Add(entryClicker);

        // 마우스 오버 이벤트 추가
        EventTrigger.Entry entryMouseEnter = new EventTrigger.Entry();
        entryMouseEnter.eventID = EventTriggerType.PointerEnter;
        entryMouseEnter.callback.AddListener((data) => EnterTile((PointerEventData)data));
        trigger.triggers.Add(entryMouseEnter);

        // 마우스 나가기 이벤트 추가
        EventTrigger.Entry entryMouseExit = new EventTrigger.Entry();
        entryMouseExit.eventID = EventTriggerType.PointerExit;
        entryMouseExit.callback.AddListener((data) => ExitTile((PointerEventData)data));
        trigger.triggers.Add(entryMouseExit);

        selectColor = tileRenderer.material.color;
        ReleaseSelected();
    }

}
