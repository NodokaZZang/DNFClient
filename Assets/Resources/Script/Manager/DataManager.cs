using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    public int RoomSQ { get; set; }
    public GameMap GameMap { get; set; } = null;

    public Dictionary<int, Define.PlayerData> PrevPlayerData;

    public int PrevAdminId = 0;
    public int PrevPlayerId = 0;
    public bool PrevIsAdmin = false;
    public int PrevUserCnt = 0;
    public Define.CameraTarget PrevCameraTraget;

    public string PublicIP { get; set; } = null;
}
