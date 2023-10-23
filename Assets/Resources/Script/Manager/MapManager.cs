using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager 
{
    public Grid CurrentGrid { get; private set; }
    public Tilemap CollisionTileMap;

    public Vector3Int CovnertCellPos(Vector3 pos) 
    {
        return CurrentGrid.WorldToCell(pos);    
    }

    public Vector3 CovnertWorldPos(Vector3Int pos)
    {
        return CurrentGrid.CellToWorld(pos);
    }


    public bool CanGo(Vector3 destPos)
    {
        Vector3Int cellPos = CurrentGrid.WorldToCell(destPos);

        if (CollisionTileMap.GetTile(cellPos) == null)
            return true;
        else
            return false;
    }

    public bool CanGo2(Vector3Int cellPos)
    {
        if (CollisionTileMap.GetTile(cellPos) == null)
            return true;
        else
            return false;
    }

    public void LoadMap(int mapId) 
    {
        DestroyMap();

        string mapName = "Map_" + mapId.ToString("000");

        GameObject go = Resources.Load<GameObject>($"Prefabs/Game/Map/{mapName}");
        GameObject map = Object.Instantiate(go);
        map.name = "Map";

        GameObject collision = Utils.FindChild(map, "Collision", true);

        if (collision != null)
        {
            CollisionTileMap = Utils.FindChild<Tilemap>(map, "Collision", true);
            collision.SetActive(false);
        }

        CurrentGrid = map.GetComponent<Grid>();
    }

    public void DestroyMap() 
    {
        GameObject map = GameObject.Find("Map");

        if (map != null)
        {
            GameObject.Destroy(map);
            CurrentGrid = null;
        }
    }
}
