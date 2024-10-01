using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitPlacementManager : MonoBehaviour
{
    [System.Serializable]
    public class UnitPlacement
    {
        public GameObject unitPrefab;       // ��ġ�� ������ ������
        public Vector2Int placementCoords;  // ��ġ�� ��ǥ
    }

    public UnitPlacement[] allyUnits; 
    public UnitPlacement[] enemyUnits;
    public Tilemap tilemap;           
    public GridManager gridManager;   

    public void PlaceUnitsOnGrid()
    {
        PlaceUnits(allyUnits, Team.Ally);
        PlaceUnits(enemyUnits, Team.Enemy);
    }

    private void PlaceUnits(UnitPlacement[] units, Team team)
    {
        foreach (var placement in units)
        {
            GameObject unitInstance = Instantiate(placement.unitPrefab);
            Unit unitComponent = unitInstance.GetComponent<Unit>();

            // ��ġ ��ǥ�� ������� Ÿ�� �˻�
            Tile tile = gridManager.GetTileAtPosition(placement.placementCoords);

            if (tile != null)
            {
                // ������ Ÿ�� �߾ӿ� ��ġ
                Vector3 worldPosition = tilemap.GetCellCenterWorld(new Vector3Int(tile.coordinates.x, tile.coordinates.y, 0));
                unitInstance.transform.position = worldPosition;

                // Ÿ�Ͽ� ���� ���� ������Ʈ
                tile.PlaceUnit(unitInstance);
                gridManager.UpdateTileUnitStatus(tile, true);

                // ���� ��ǥ�� Ÿ�� ����
                unitComponent.unitCoordinates = tile.coordinates;
                unitComponent.currentTile = tile;
                unitComponent.team = team;
            }
            else
            {
                Debug.LogError($"Ÿ�Ͽ� ���� ���� : {placement.placementCoords}");
            }
        }
    }
}
