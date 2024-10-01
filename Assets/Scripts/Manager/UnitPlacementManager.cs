using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitPlacementManager : MonoBehaviour
{
    [System.Serializable]
    public class UnitPlacement
    {
        public GameObject unitPrefab;       // 배치할 유닛의 프리팹
        public Vector2Int placementCoords;  // 배치할 좌표
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

            // 배치 좌표를 기반으로 타일 검색
            Tile tile = gridManager.GetTileAtPosition(placement.placementCoords);

            if (tile != null)
            {
                // 유닛을 타일 중앙에 배치
                Vector3 worldPosition = tilemap.GetCellCenterWorld(new Vector3Int(tile.coordinates.x, tile.coordinates.y, 0));
                unitInstance.transform.position = worldPosition;

                // 타일에 유닛 정보 업데이트
                tile.PlaceUnit(unitInstance);
                gridManager.UpdateTileUnitStatus(tile, true);

                // 유닛 좌표와 타일 설정
                unitComponent.unitCoordinates = tile.coordinates;
                unitComponent.currentTile = tile;
                unitComponent.team = team;
            }
            else
            {
                Debug.LogError($"타일에 유닛 존재 : {placement.placementCoords}");
            }
        }
    }
}
