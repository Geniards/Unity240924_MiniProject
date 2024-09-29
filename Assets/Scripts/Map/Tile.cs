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

    // ���콺�� Ÿ�� ���� �������� ��
    public void EnterTile(PointerEventData data)
    {
        if (isClicking) return;

        if (hasCharcter && character)
        {
            // ĳ���� ����â Ȱ��ȭ (���¸� �����ϰ� ȭ�鿡 ǥ��)
            StateUI.Instance.OnPointerEnterWithUnit(character, data);
        }
    }

    // ���콺�� Ÿ�� ������ ������ ��
    public void ExitTile(PointerEventData data)
    {
        // ����â �ݱ�
        StateUI.Instance.OnPointerExit(data);
    }

    // �̵� �Ϸ� �� ȣ���ϴ� �޼���
    public void MoveComplete()
    {
        isClicking = false;
        // �̵� �� ����â �ݱ� �� �߰� �۾� ����
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

        // ���콺 Ŭ�� �̺�Ʈ �߰�
        EventTrigger.Entry entryClicker = new EventTrigger.Entry();
        entryClicker.eventID = EventTriggerType.PointerClick;
        entryClicker.callback.AddListener((data) => ClickTile((PointerEventData)data));
        trigger.triggers.Add(entryClicker);

        // ���콺 ���� �̺�Ʈ �߰�
        EventTrigger.Entry entryMouseEnter = new EventTrigger.Entry();
        entryMouseEnter.eventID = EventTriggerType.PointerEnter;
        entryMouseEnter.callback.AddListener((data) => EnterTile((PointerEventData)data));
        trigger.triggers.Add(entryMouseEnter);

        // ���콺 ������ �̺�Ʈ �߰�
        EventTrigger.Entry entryMouseExit = new EventTrigger.Entry();
        entryMouseExit.eventID = EventTriggerType.PointerExit;
        entryMouseExit.callback.AddListener((data) => ExitTile((PointerEventData)data));
        trigger.triggers.Add(entryMouseExit);

        selectColor = tileRenderer.material.color;
        ReleaseSelected();
    }

}
