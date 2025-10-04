using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileAreaChecker
{
    private Tilemap tilemap;
    private GameObject ignoreObject;

    public TileAreaChecker(Tilemap tilemap, GameObject ignoreObject = null)
    {
        this.tilemap = tilemap;
        this.ignoreObject = ignoreObject;
    }

    public List<Vector3Int> GetSquareCellsAtWorldPos(Vector3 worldPos, int sizeTiles)
    {
        var cells = new List<Vector3Int>();
        Vector3Int centerCell = tilemap.WorldToCell(worldPos);
        int half = sizeTiles / 2;
        int startX = centerCell.x - half;
        int startY = centerCell.y - half;

        for (int y = startY; y < startY + sizeTiles; y++)
            for (int x = startX; x < startX + sizeTiles; x++)
                cells.Add(new Vector3Int(x, y, centerCell.z));

        return cells;
    }

    public bool IsSquareAreaFree(Vector3 worldPos, int sizeTiles)
    {
        if (sizeTiles <= 0) sizeTiles = 1;
        Vector3 cellWorldSize = tilemap.cellSize;
        const float boxScale = 0.9f;

        var cells = GetSquareCellsAtWorldPos(worldPos, sizeTiles);

        foreach (var cell in cells)
        {
            GameObject inst = tilemap.GetInstantiatedObject(cell);
            if (inst != null && inst != ignoreObject)
                return false;

            Vector2 center = tilemap.GetCellCenterWorld(cell);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, cellWorldSize * boxScale, 0f, LayerMask.GetMask("Houses"));

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null && hits[i].gameObject != ignoreObject)
                    return false;
            }
        }

        return true;
    }
}
