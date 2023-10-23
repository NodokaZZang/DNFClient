using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameMap1 : GameMap
{
    void Start()
    {
        byte[] bytes = new byte[1024];

        MemoryStream ms = new MemoryStream(bytes);
        BinaryWriter bw = new BinaryWriter(ms);

        int pktSize = 4;

        bw.Write((Int16)Define.PacketProtocol.C_T_S_GAMEINIT);
        bw.Write((Int16)pktSize);

        _net.SendPacket(bytes, pktSize);
    }

    public override void Init(int adminSq, int playerId, List<Define.PlayerInfo> playerInfos)
    {
        base.Init(adminSq, playerId, playerInfos);
    }

    public override void UpdateLoading()
    {
        base.UpdateLoading();
    }

    public override void MonsterSpawn()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");

        foreach (GameObject monsterGo in monsters) 
        {
            MonsterController mc = monsterGo.GetComponent<MonsterController>();
            int monsterId = mc.MonsterID;
            MonsterDict.Add(monsterId, mc);
        }
    }

    public override bool GameDefeat()
    {
        return base.GameDefeat();
    }

    public override void UpdateNextStage() 
    {
        base.UpdateNextStage();
        bool nextStage = true;
        foreach (var cc in PlayerDict.Values) 
        {
            if (cc.CellPos.x < 36) 
            {
                nextStage = false;
                break;
            }
        }

        if (nextStage && IsAdmin)
        {
            // 다음으로 넘어갈수 있냐 ?

            byte[] data = new byte[100];

            MemoryStream ms = new MemoryStream(data);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)(4);

            bw.Write((Int16)Define.PacketProtocol.UDP_NEXT_STAGE);
            bw.Write((Int16)pktHeader);

            _net.UDPBrodCast(data, pktHeader);

            NextStageDataPass();
        }
    }

    public override void NextStageDataPass()
    {
        Managers.Instance.DataManager.PrevPlayerData = new Dictionary<int, Define.PlayerData>();

        Managers.Instance.DataManager.PrevAdminId = AdminId;
        Managers.Instance.DataManager.PrevPlayerId = PlayerId;
        Managers.Instance.DataManager.PrevIsAdmin = IsAdmin;
        Managers.Instance.DataManager.PrevUserCnt = UserCnt;
        Managers.Instance.DataManager.PrevCameraTraget = new Define.CameraTarget();
        Managers.Instance.DataManager.PrevCameraTraget.playerId = _cameraTarget.playerId;


        foreach (var old in PlayerHpcDict) 
        {
            int playerId = old.Key;
            PlayerHPController phc = old.Value;
            
            Define.PlayerData tempPlayerData = new Define.PlayerData();
           
            tempPlayerData.playerId = playerId;
            tempPlayerData.hp = phc.HP;
            tempPlayerData.hpMax = phc.HP_MAX;
            tempPlayerData.playerType = phc.playerType;
            tempPlayerData.username = phc.playerUsername;

            Managers.Instance.DataManager.PrevPlayerData.Add(playerId, tempPlayerData);
        }

        GMS = Define.GameMapState.Stop;
        GameObject UI = GameObject.FindWithTag("fade");
        GameObject fadeGo = UI.transform.Find("Fade").gameObject;
        fadeGo.SetActive(true);
        UI.transform.Find("Fade").GetComponent<Fade>().FadeOut(Define.SceneType.GameScene2);
    }
}
