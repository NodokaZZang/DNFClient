using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class GameMap2 : GameMap
{
    void Start()
    {
        AdminId = Managers.Instance.DataManager.PrevAdminId;
        PlayerId = Managers.Instance.DataManager.PrevPlayerId;
        IsAdmin = Managers.Instance.DataManager.PrevIsAdmin;
        UserCnt = Managers.Instance.DataManager.PrevUserCnt;

        int userIndex = 1;

        Dictionary<int, Define.PlayerData> prevPlayerDict = Managers.Instance.DataManager.PrevPlayerData;

        foreach (var playerInfoKV in prevPlayerDict)
        {
            Define.PlayerData playerData = playerInfoKV.Value;
            
            GameObject hpUi = GameObject.Find($"UserHPUI{userIndex}");
            PlayerHPController playerHpc = hpUi.GetComponent<PlayerHPController>();
            playerHpc.Init(playerData.username, playerData.playerType, true, playerData.hp, playerData.hpMax);
            PlayerHpcDict.Add(playerData.playerId, playerHpc);

            GameObject loadingUi = GameObject.Find($"UserTaskUI{userIndex}");
            PlayerLoadingController playerLoading = loadingUi.GetComponent<PlayerLoadingController>();
            playerLoading.Init(playerData.username, playerData.playerType, true, UserCnt);
            TaskDict.Add(playerData.playerId, playerLoading);

            if (playerData.hp <= 0)
            {
                GameObject.Find($"Username{userIndex}").SetActive(false);
            }
            else
            {
                GameObject playerGameObject = playerData.playerType == Define.PlayerType.SwordMan ? Managers.Instance.ResourceManager.Instantiate("Game/Player/SwordMan", transform) : Managers.Instance.ResourceManager.Instantiate("Game/Player/Gunner", transform);

                if (playerData.playerId == PlayerId)
                {
                    PlayerController pc = playerGameObject.AddComponent<PlayerController>();
                    pc.Init(PlayerStartPos, playerData.playerType);
                    pc.PlayerId = playerData.playerId;
                    pc.Username = GameObject.Find($"Username{userIndex}");
                    pc.Username.GetComponent<TMP_Text>().text = playerData.username;
                    pc.HP = playerData.hp;
                
                }
                else
                {
                    OtherPlayerController opc = playerGameObject.AddComponent<OtherPlayerController>();
                    opc.Init(PlayerStartPos, playerData.playerType);
                    opc.PlayerId = playerData.playerId;
                    opc.Username = GameObject.Find($"Username{userIndex}");
                    opc.Username.GetComponent<TMP_Text>().text = playerData.username;
                    opc.HP = playerData.hp;
                }

                CreatureController cc = playerGameObject.GetComponent<CreatureController>();

                PlayerDict.Add(playerData.playerId, cc);
            }
            userIndex++;
        }

        for (; userIndex < 5; userIndex++)
        {
            GameObject hpUi = GameObject.Find($"UserHPUI{userIndex}");
            hpUi.GetComponent<PlayerHPController>().Init("", Define.PlayerType.None, false);

            GameObject loadingUi = GameObject.Find($"UserTaskUI{userIndex}");
            loadingUi.GetComponent<PlayerLoadingController>().Init("", Define.PlayerType.None, false, 0);

            GameObject.Find($"Username{userIndex}").SetActive(false);
        }

        _cameraTarget = Managers.Instance.DataManager.PrevCameraTraget;

         PlayerDict.TryGetValue(_cameraTarget.playerId, out var tc);

        _cameraTarget.target = tc.gameObject;

        if (_cameraTarget.playerId != PlayerId)
            _viewText.gameObject.SetActive(true);


        MonsterSpawn();

        // 혼자서 실행하는건지 아닌지 판단
        // 만약에 혼자서 실행한다면 다른 유저들을 기다릴 필요가 없음
        GMS = Define.GameMapState.Loading;

        if (UserCnt == 0)
        {
            SinglePlayer = true;
            TaskDict.TryGetValue(PlayerId, out var playerLoadingController);
            playerLoadingController.TaskComplete();
        }

        else
            SinglePlayer = false;

        StartCoroutine(CoTaskSend());

        if (IsAdmin)
            StartCoroutine(CoTaskCompleteSend());
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
            if (cc.CellPos.x < 35) 
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
        UI.transform.Find("Fade").GetComponent<Fade>().FadeOut(Define.SceneType.GameScene3);
    }
}
