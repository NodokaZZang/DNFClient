using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class GameMap3 : GameMap
{
    private bool _coGameVictory = false;
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
        if (_coGameVictory == false)
        {
            StartCoroutine(CoGameVictory());
        }
    }

    IEnumerator CoGameVictory() 
    {
        _coGameVictory = true;

        StopCoroutine(MonsterAI());
        _resultMessage.text = "임 무 성 공";
        _resultMessage.color = Color.blue;
        _resultMessage.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        if (IsAdmin)
        {
            byte[] bytes = new byte[1024];
            MemoryStream ms = new MemoryStream(bytes);
            BinaryWriter bw = new BinaryWriter(ms);

            int pktSize = 4;

            bw.Write((Int16)Define.PacketProtocol.C_T_S_GAMEEND);
            bw.Write((Int16)pktSize);

            _net.SendPacket(bytes, pktSize);
        }

        GameObject UI = GameObject.FindWithTag("fade");
        GameObject fadeGo = UI.transform.Find("Fade").gameObject;
        fadeGo.SetActive(true);
        UI.transform.Find("Fade").GetComponent<Fade>().FadeOut(Define.SceneType.LobbyScene);
        yield break;
    }
}
