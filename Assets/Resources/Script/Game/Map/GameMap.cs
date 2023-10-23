using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameMap : MonoBehaviour
{
    public int PlayerId { get; set; }
    public int AdminId { get; set; }
    public bool IsAdmin { get; set; } = false;
    public Define.GameMapState GMS { get; set; } = Define.GameMapState.Stop;
    public GameObject HpHeelObject;
    public int UserCnt { get; set; }
    public Vector3Int PlayerStartPos = new Vector3Int(-8, -3, -3);
    public Dictionary<int, CreatureController> PlayerDict { get; set; } = new Dictionary<int, CreatureController>();
    public Dictionary<int, PlayerHPController> PlayerHpcDict { get; set; } = new Dictionary<int, PlayerHPController>();    
    public Dictionary<int, MonsterController> MonsterDict { get; set; } = new Dictionary<int, MonsterController>();
    public Dictionary<int, PlayerLoadingController> TaskDict { get; set; } = new Dictionary<int, PlayerLoadingController>();
    protected Define.CameraTarget _cameraTarget = new Define.CameraTarget();
    protected TMP_Text _resultMessage;
    public TMP_Text _viewText;
    protected Network _net;
    protected GameObject _loadingUI;
    public GameObject _SkillIcon;
    public TMP_Text _NextStageText;
    private bool _comonsterAi = false;
    protected HashSet<int> _playerTaskCheck = new HashSet<int>();
    protected HashSet<int> _playerTaskCompleteCheck = new HashSet<int>();
    protected Dictionary<int, HashSet<int>> _playerTaskCheckOther = new Dictionary<int, HashSet<int>>(); 
    protected bool _coGameDefeat = false;
    public bool GameStart = false;
    public bool SinglePlayer { get; set; }
    private bool _loadingEnd = false;
    private bool _coGameStart = false;
    void Awake()
    {
        Managers.Instance.DataManager.GameMap = this;
        GMS = Define.GameMapState.Stop;
        PlayerStartPos = new Vector3Int(-8, -3, -3);

        _SkillIcon = GameObject.Find("SkillIcon");
        _NextStageText = GameObject.Find("NextStageText").GetComponent<TMP_Text>();
        _NextStageText.gameObject.SetActive(false);

        _net = GameObject.FindWithTag("net").GetComponent<Network>();
    
        GameObject UI = GameObject.FindWithTag("fade");

        _resultMessage = UI.transform.Find("ResultMessage").GetComponent<TMP_Text>();
        _viewText = UI.transform.Find("ViewText").GetComponent<TMP_Text>();

        _loadingUI = GameObject.Find("Loading");
    }

    void Update()
    {
        switch (GMS)
        {
            case Define.GameMapState.Init:
            case Define.GameMapState.None:
                break;

            case Define.GameMapState.Loading:
                UpdateLoading();
                break;

            case Define.GameMapState.Play:
                UpdatePlay();
                break;

            case Define.GameMapState.Defeat:
                UpdateDefeat();
                break;

            case Define.GameMapState.NextStage:
                UpdateNextStage();
                break;
        }
    }

    private void UpdateDefeat()
    {
        if (_coGameDefeat == false) 
        {
            StartCoroutine(CoGameDefeat());
        } 
    }

    private void UpdatePlay()
    {
        if (GameDefeat()) 
        {
            GMS = Define.GameMapState.Defeat;
            
            if (IsAdmin)
                StopCoroutine(MonsterAI());
            
            return;
        }

        if (MonsterDict.Count == 0) 
        {
            GMS = Define.GameMapState.NextStage;

            return;
        }

        if (IsAdmin && _comonsterAi == false) 
        {
            StartCoroutine(MonsterAI());
        }
    }

    public virtual void Init(int adminSq, int playerId, List<Define.PlayerInfo> playerInfos)
    {
        PlayerId = playerId;
        AdminId = adminSq;

        if (PlayerId == AdminId)
            IsAdmin = true;

        List<Define.PlayerUDPInfo> ipList = new List<Define.PlayerUDPInfo>();
        string publicIp = Managers.Instance.DataManager.PublicIP;

        // UDP 세팅
        foreach (Define.PlayerInfo playerInfo in playerInfos)
        {
            if (playerInfo.playerId == PlayerId)
                continue;

            int playerID = playerInfo.playerId;
            string playerIp = playerInfo.ipStr;

            //if (playerIp == publicIp)
            //    playerIp = playerInfo.localIpStr;

            if (publicIp.Equals(playerIp)) // publicIp == playerIp
                playerIp = playerInfo.localIpStr;

            IPAddress ip = IPAddress.Parse(playerIp); // 여기서 에러 발생 "" "737894424240"
            IPEndPoint ipep = new IPEndPoint(ip, playerInfo.port);
            
            Define.PlayerUDPInfo playerUDP = new Define.PlayerUDPInfo();
            playerUDP.ipep = ipep;
            playerUDP.playerId = playerID;

            ipList.Add(playerUDP);
        }

        _net.UDPInit(ipList);
        int userIndex = 1;

        UserCnt = ipList.Count;

        foreach (Define.PlayerInfo playerInfo in playerInfos)
        {
            GameObject hpUi = GameObject.Find($"UserHPUI{userIndex}");
            PlayerHPController playerHpc = hpUi.GetComponent<PlayerHPController>();
            playerHpc.Init(playerInfo.username, playerInfo.playerType, true);
            PlayerHpcDict.Add(playerInfo.playerId, playerHpc);

            GameObject loadingUi = GameObject.Find($"UserTaskUI{userIndex}");
            PlayerLoadingController playerLoading = loadingUi.GetComponent<PlayerLoadingController>();
            playerLoading.Init(playerInfo.username, playerInfo.playerType, true, UserCnt);
            TaskDict.Add(playerInfo.playerId, playerLoading);

            SpawnPlayer(playerInfo, userIndex);
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

    internal void UDP_TASK_COMPLETE_PING_CALLBACK(int playerId)
    {
        if (_playerTaskCompleteCheck.Contains(playerId)) return;

        _playerTaskCompleteCheck.Add(playerId); 
    }

    internal void GameIsTaskComplete()
    {
        bool complete = true;

        foreach (var prcc in TaskDict.Values)
        {
            if (prcc.ISTaskComplete() == false)
            {
                complete = false;
                break;
            }
        }

        if (complete == false) return;

        // 모든 로딩이 완료됨
        byte[] data = new byte[100];

        MemoryStream ms = new MemoryStream(data);
        BinaryWriter bw = new BinaryWriter(ms);

        Int16 pktHeader = (Int16)(4 + 4);

        bw.Write((Int16)Define.PacketProtocol.UDP_TASK_COMPLETE_PING_CALLBACK);
        bw.Write((Int16)pktHeader);

        bw.Write((Int32)PlayerId);

        _net.UDPPlayerSend(AdminId, data, pktHeader);
    }

    internal void UDP_TASK_UP_PING(int from, int to)
    {
        if (_playerTaskCheckOther.ContainsKey(to) == false) 
        {
            _playerTaskCheckOther.Add(to, new HashSet<int>());
            // 1 {2, 3, 4} 
            // 2 {3, 4, 1}
            // 3 {1, 2, 4}
            // 4 {1, 2, 3,}

        }
        // to: 1 from :3 
        _playerTaskCheckOther.TryGetValue(to, out var set);

        if (set.Contains(from) == true) return;

        set.Add(from);

        TaskDict.TryGetValue(to, out var playerLoadingController);
        playerLoadingController.TaskComplete();
    }

    internal void UDP_TASK_PING_CALLBACK(int playerId)
    {
        if (GMS != Define.GameMapState.Loading)
            return;

        if (_playerTaskCheck.Contains(playerId) == false) 
        {
            _playerTaskCheck.Add(playerId);

            TaskDict.TryGetValue(PlayerId, out var playerLoadingController);
            playerLoadingController.TaskComplete();
        }
       
        byte[] data = new byte[100];

        MemoryStream ms = new MemoryStream(data);
        BinaryWriter bw = new BinaryWriter(ms);

        Int16 pktHeader = (Int16)(4 + 4 + 4);

        bw.Write((Int16)Define.PacketProtocol.UDP_TASK_UP_PING);
        bw.Write((Int16)pktHeader);
        bw.Write((Int32)playerId); // from
        bw.Write((Int32)PlayerId); // to

        _net.UDPBrodCast(data, pktHeader);
    }

    internal void HPHeel()
    {
        foreach (var playerGo in Managers.Instance.DataManager.GameMap.PlayerDict.Values)
        {
            CreatureController cc = playerGo.GetComponent<CreatureController>();

            if (cc.SM.State == Define.CreatureState.Die)
                continue;

            cc.HpUP(250);
        }

        GameObject heel = GameObject.FindGameObjectWithTag("heel");
        Destroy(heel);
    }

    internal void PlayerAttacedSync(int playerId, int damage)
    {
        PlayerDict.TryGetValue(playerId, out var playerGameObject);
        CreatureController cc = playerGameObject.GetComponent<CreatureController>();
        cc.Attacked(damage);
    }

    internal void MonsterAttackedSync(int monsterId, int damage)
    {
        try
        {
            MonsterDict.TryGetValue(monsterId, out var monsterGameObject);
            MonsterController mc = monsterGameObject.GetComponent<MonsterController>();
            mc.Attacked(damage);
        }
        catch (Exception e) 
        {
            Debug.Log(e);
        }
    }

    protected void SpawnPlayer(Define.PlayerInfo playerInfo, int index) 
    {
        GameObject playerGameObject = playerInfo.playerType == Define.PlayerType.SwordMan ? Managers.Instance.ResourceManager.Instantiate("Game/Player/SwordMan",transform) : Managers.Instance.ResourceManager.Instantiate("Game/Player/Gunner", transform);

        if (playerInfo.playerId == PlayerId)
        {
            PlayerController pc = playerGameObject.AddComponent<PlayerController>();
            pc.Init(PlayerStartPos, playerInfo.playerType);
            pc.PlayerId = playerInfo.playerId;
            pc.Username = GameObject.Find($"Username{index}");
            pc.Username.GetComponent<TMP_Text>().text = playerInfo.username;
            
            Define.CameraTarget target = new Define.CameraTarget();
            target.playerId = PlayerId;
            target.target = playerGameObject;
            _cameraTarget = target;
        }
        else
        {
            OtherPlayerController opc = playerGameObject.AddComponent<OtherPlayerController>();
            opc.Init(PlayerStartPos, playerInfo.playerType);
            opc.PlayerId = playerInfo.playerId;
            opc.Username = GameObject.Find($"Username{index}");
            opc.Username.GetComponent<TMP_Text>().text = playerInfo.username;
        }

        CreatureController cc = playerGameObject.GetComponent<CreatureController>();

        PlayerDict.Add(playerInfo.playerId, cc);
    }

    internal void MonsterAISync(int monsterId, Vector3 monsterPos, Vector3Int monsterCellPos, Vector3Int destCellPos, Define.MonsterPattern monsterPattern)
    {
        MonsterDict.TryGetValue(monsterId, out var monsterGO);

        if (monsterGO == null)
            return;

        var mc = monsterGO.GetComponent<MonsterController>();
        mc.MonsterAISync(monsterId, monsterPos, monsterCellPos, destCellPos, monsterPattern);
    }

    internal void PlayerSync(int playerID, Vector3 ts, Define.CreatureState sM, Define.MoveDir dIR, bool left, Vector3Int cell, Vector3 jumpStartPos, bool attackFalg, Vector3 shadowPos)
    {;
        PlayerDict.TryGetValue(playerID, out var cc);

        if (cc == null) return;

        OtherPlayerController opc = cc.gameObject.GetComponent<OtherPlayerController>();
        opc.PlayerSync(ts, sM, dIR, left, cell, jumpStartPos, attackFalg, shadowPos);
    }

    public void UDP_TASK_PING_RECV(int playerId)
    {
        //if (GMS != Define.GameMapState.Loading)
        //    return;

        //if (_playerTaskCheck.Contains(playerId)) return;

        //_playerTaskCheck.Add(playerId);

        //TaskDict.TryGetValue(PlayerId, out var playerLoadingController);
        //playerLoadingController.TaskComplete();

        // TODO 내 퍼센트가 올랐다라는걸 전송

        // Ping 콜백
        byte[] data = new byte[100];

        MemoryStream ms = new MemoryStream(data);
        BinaryWriter bw = new BinaryWriter(ms);

        Int16 pktHeader = (Int16)(4 + 4);

        bw.Write((Int16)Define.PacketProtocol.UDP_TASK_PING_CALLBACK);
        bw.Write((Int16)pktHeader);

        bw.Write((Int32)PlayerId);

        _net.UDPBrodCast(data, pktHeader);
    }

    public virtual void UpdateLoading() 
    {
        if (SinglePlayer)
        {
            StopCoroutine(CoTaskSend());
            StopCoroutine(CoTaskCompleteSend());
            StartCoroutine(CoLoadingEnd());
            GMS = Define.GameMapState.None;
        }
        else
        {
            bool complete = true;

            foreach (var prcc in TaskDict.Values) 
            {
                if (prcc.ISTaskComplete() == false)
                {
                    complete = false;
                    break;
                }        
            }

            if (complete == false) return;

            // 이중점검 모두들 전부 로딩이 다 완료 되었는지 확인

            if (IsAdmin) 
            {
                // 어드민인 경우 모두들 다 로딩이 완료 되었는지 확인후 
                // 게임 시작
                bool taskComplete = true;
                foreach (int playerId in PlayerHpcDict.Keys) 
                {
                    if (playerId == AdminId)
                        continue;

                    if (_playerTaskCompleteCheck.Contains(playerId) == false) 
                    {
                        taskComplete = false;
                        break;
                    }
                }

                if (taskComplete) 
                {
                    // 패킷 전송 게임을 시작해라 !!! 
                    if (_coGameStart == false)
                        StartCoroutine(CoGameStartSend());
                    GameStart = true;
                }   
            }

            if (GameStart) 
            {
                StopCoroutine(CoTaskSend());

                if (IsAdmin)
                { 
                    StopCoroutine(CoTaskCompleteSend());
                    StopCoroutine(CoGameStartSend());
                }

                StartCoroutine(CoLoadingEnd());
            }
        }
    }

    public IEnumerator CoGameStartSend() 
    {
        _coGameStart = true;

        for (int i = 0; i < 4; i++) 
        {

            byte[] data = new byte[100];

            MemoryStream ms = new MemoryStream(data);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)(4);

            bw.Write((Int16)Define.PacketProtocol.UDP_LOADING_END);
            bw.Write((Int16)pktHeader);
            _net.UDPBrodCast(data, pktHeader);

            yield return new WaitForSeconds(0.23f);
        }
    }

    public virtual void MonsterSpawn() 
    {
        
    }

    public virtual bool GameDefeat() 
    {
        return PlayerDict.Count == 0;
    }

    public virtual void UpdateNextStage() 
    {
        _NextStageText.gameObject.SetActive(true);
    }

    public virtual void GameVictory()
    {
        
    }

    public void CameraSwap(int deadPlayerId)
    {
        if (deadPlayerId != _cameraTarget.playerId)
            return;

        foreach (var g in Managers.Instance.DataManager.GameMap.PlayerDict)
        {
            _viewText.gameObject.SetActive(true);

            CreatureController cc = g.Value;
            int playerId = g.Key;

            Define.CameraTarget target = new Define.CameraTarget();
            target.playerId = playerId;
            target.target = cc.gameObject;
            _cameraTarget = target;
            break;
        }
    }

    protected IEnumerator CoLoadingEnd() 
    {
        yield return new WaitForSeconds(1.0f);
        _loadingUI.SetActive(false);
        GMS = Define.GameMapState.Play;
    }

    protected IEnumerator CoTaskSend() 
    {
        while (true) 
        {
            yield return new WaitForSeconds(2.0f);

            // Ping 보내기
            byte[] data = new byte[100];

            MemoryStream ms = new MemoryStream(data);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)(4 + 4);

            bw.Write((Int16)Define.PacketProtocol.UDP_TASK_PING);
            bw.Write((Int16)pktHeader);

            bw.Write((Int32)PlayerId);

            _net.UDPBrodCast(data, pktHeader);
        }
    }

    protected IEnumerator CoTaskCompleteSend()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            // Ping 보내기
            byte[] data = new byte[100];

            MemoryStream ms = new MemoryStream(data);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)(4 + 4);

            bw.Write((Int16)Define.PacketProtocol.UDP_TASK_COMPLETE_PING);
            bw.Write((Int16)pktHeader);

            bw.Write((Int32)PlayerId);

            _net.UDPBrodCast(data, pktHeader);
        }
    }

    protected IEnumerator MonsterAI()
    {
        _comonsterAi = true;

        while (_comonsterAi)
        {
            List<MonsterController> list = new List<MonsterController>();

            foreach (var monster in MonsterDict)
            {
                MonsterController mc = monster.Value;
                mc.UpdateAI();
                list.Add(mc);
            }

            byte[] bytes = new byte[2048];
            MemoryStream ms = new MemoryStream(bytes);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 monsterCnt = (Int16)list.Count;
            Int16 pktHeader = (Int16)((monsterCnt * 44) + 2 + 4);

            bw.Write((Int16)Define.PacketProtocol.UDP_MONSTERSYNC);
            bw.Write((Int16)pktHeader);
            bw.Write((Int16)monsterCnt);

            foreach (var monster in list)
            {
                // 몬스터 아이디
                bw.Write((Int32)monster.MonsterID);

                // 현재 좌표
                Vector3 nowPos = monster.transform.position;
                bw.Write((float)nowPos.x);
                bw.Write((float)nowPos.y);
                bw.Write((float)nowPos.z);

                Vector3Int nowCellPos = monster.CellPos;
                bw.Write((Int32)nowCellPos.x);
                bw.Write((Int32)nowCellPos.y);
                bw.Write((Int32)nowCellPos.z);

                // 목적 좌표
                Vector3Int destPos = monster.DestPos;
                bw.Write((Int32)destPos.x);
                bw.Write((Int32)destPos.y);
                bw.Write((Int32)destPos.z);

                // 현재 패턴
                Define.MonsterPattern nowPattern = monster.MonsterPattern;
                bw.Write((Int32)nowPattern);
            }

            _net.UDPBrodCast(bytes, pktHeader);

            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator CoGameDefeat() 
    {
        _coGameDefeat = true;

        _resultMessage.text = "임 무 실 패";
        _resultMessage.color = Color.red;
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
    }

    public virtual void NextStageDataPass() 
    {
        
    }

    void LateUpdate()
    {
        if (_cameraTarget.target == null) return;

        const float MapXSizeMin = -8.6f;
        const float MapXSizeMax = 37.2f;

        float height = Camera.main.orthographicSize;
        float width = height * Screen.width / Screen.height;

        float maxLeftPos = _cameraTarget.target.transform.position.x - width;
        float maxRightPos = _cameraTarget.target.transform.position.x + width;

        if (maxLeftPos <= MapXSizeMin)
        {
            Camera.main.transform.position = new Vector3(MapXSizeMin + width, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
        else if (maxRightPos >= MapXSizeMax)
        {
            Camera.main.transform.position = new Vector3(MapXSizeMax - width, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
        else
        {
            Camera.main.transform.position = new Vector3(_cameraTarget.target.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
    }
}
