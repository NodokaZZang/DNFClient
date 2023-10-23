using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class PacketHandler
{
    internal void Handler(ArraySegment<byte> segment)
    {
        ArraySegment<byte> pktCodeByte = new ArraySegment<byte>(segment.Array, 0, sizeof(Int16));
        ArraySegment<byte> pktSizeByte = new ArraySegment<byte>(segment.Array, sizeof(Int16), sizeof(Int16));
    
        Define.PacketProtocol pktCode = (Define.PacketProtocol) BitConverter.ToInt16(pktCodeByte);
        Int16 pktSize = BitConverter.ToInt16(pktSizeByte);
        int dataSize = pktSize - sizeof(Int32);
        ArraySegment<byte> dataPtr = new ArraySegment<byte>(segment.Array, sizeof(Int32), dataSize);

        switch (pktCode) 
        {
            case Define.PacketProtocol.S_T_C_LOGIN:
                PacketHandler_S_T_C_LOGIN(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_USERLIST:
                PacketHandler_S_T_C_USERLIST(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_CHATSEND_LOBBY:
                PacketHandler_S_T_C_CHATSEND_LOBBY(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_ENTEROOM:
                PacketHandler_S_T_C_ENTEROOM(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_ROOMLIST:
                PacketHandler_S_T_C_ROOMLIST(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_WATINGROOMINIT:
                PacketHandler_S_T_C_WATINGROOMINIT(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_WATINGROOMOUTUSER:
                PacketHandler_S_T_C_WATINGROOMOUTUSER(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_S_CHATSEND_WATINGROOM:
                PacketHandler_S_T_S_CHATSEND_WATINGROOM(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_WATINGROOMADMIN:
                PacketHandler_S_T_C_WATINGROOMADMIN(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_GAMESTART:
                PacketHandler_S_T_C_GAMESTART(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_GAMEINIT:
                PacketHandler_S_T_C_GAMEINIT(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.S_T_C_IPPORTINFO:
                PacketHandler_UDP_IPPORTINFO(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_TASK_PING:
                PacketHandler_UDP_TASK_PING(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_PLAYERSYNC:
                PacketHandler_UDP_PLAYERSYNC(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_MONSTERSYNC:
                PacketHandler_UDP_MONSTERSYNC(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_MONSTERATTACKED:
                PacketHandler_UDP_MONSTERATTACKED(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_PLAYERATTACKED:
                PacketHandler_UDP_PLAYERATTACKED(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_HPHEEL:
                PacketHandler_UDP_HPHEEL(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_TASK_PING_CALLBACK:
                PacketHandler_UDP_TASK_PING_CALLBACK(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_TASK_UP_PING:
                PacketHandler_UDP_TASK_UP_PING(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_NEXT_STAGE:
                PacketHandler_UDP_NEXT_STAGE(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_TASK_COMPLETE_PING:
                PacketHandler_UDP_TASK_COMPLETE_PING(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_TASK_COMPLETE_PING_CALLBACK:
                PacketHandler_UDP_TASK_COMPLETE_PING_CALLBACK(dataPtr, dataSize);
                break;

            case Define.PacketProtocol.UDP_LOADING_END:
                PacketHandler_UDP_LOADING_END(dataPtr, dataSize);
                break;
        }
    }

    private void PacketHandler_UDP_LOADING_END(ArraySegment<byte> dataPtr, int dataSize)
    {
        Managers.Instance.DataManager.GameMap.GameStart = true;
    }

    private void PacketHandler_UDP_TASK_COMPLETE_PING_CALLBACK(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();

        Managers.Instance.DataManager.GameMap.UDP_TASK_COMPLETE_PING_CALLBACK(playerId);
    }

    private void PacketHandler_UDP_TASK_COMPLETE_PING(ArraySegment<byte> dataPtr, int dataSize)
    {
        Managers.Instance.DataManager.GameMap.GameIsTaskComplete();
    }

    private void PacketHandler_UDP_NEXT_STAGE(ArraySegment<byte> dataPtr, int dataSize)
    {
        Managers.Instance.DataManager.GameMap.NextStageDataPass();
    }

    private void PacketHandler_UDP_TASK_UP_PING(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 from = br.ReadInt32();
        Int32 to = br.ReadInt32();
        Managers.Instance.DataManager.GameMap.UDP_TASK_UP_PING(from, to);
    }

    private void PacketHandler_UDP_TASK_PING_CALLBACK(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();
        Managers.Instance.DataManager.GameMap.UDP_TASK_PING_CALLBACK(playerId);
    }

    private void PacketHandler_UDP_HPHEEL(ArraySegment<byte> dataPtr, int dataSize)
    {
        Managers.Instance.DataManager.GameMap.HPHeel();
    }

    private void PacketHandler_UDP_PLAYERATTACKED(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();
        Int32 damage = br.ReadInt32();

        Managers.Instance.DataManager.GameMap.PlayerAttacedSync(playerId, damage);
    }

    private void PacketHandler_UDP_MONSTERATTACKED(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int16 monsterCnt = br.ReadInt16();

        for (int i = 0; i < monsterCnt; i++)
        {
            int monsterId = br.ReadInt32();
            int damage = br.ReadInt32();

            Managers.Instance.DataManager.GameMap.MonsterAttackedSync(monsterId, damage);
        }
    }

    private void PacketHandler_UDP_MONSTERSYNC(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int16 monsterCnt = br.ReadInt16();

        for (int i = 0; i < monsterCnt; i++)
        {
            Int32 monsterId = br.ReadInt32();

            float monsterPosx = br.ReadSingle();
            float monsterPosy = br.ReadSingle();
            float monsterPosz = br.ReadSingle();

            Vector3 monsterPos = new Vector3(monsterPosx, monsterPosy, monsterPosz);

            Int32 monsterCellPosx = br.ReadInt32();
            Int32 monsterCellPosy = br.ReadInt32();
            Int32 monsterCellPosz = br.ReadInt32();

            Vector3Int monsterCellPos = new Vector3Int(monsterCellPosx, monsterCellPosy, monsterCellPosz);

            Int32 monsterDestPosx = br.ReadInt32();
            Int32 monsterDestPosy = br.ReadInt32();
            Int32 monsterDestPosz = br.ReadInt32();

            Vector3Int destCellPos = new Vector3Int(monsterDestPosx, monsterDestPosy, monsterDestPosz);

            Define.MonsterPattern monsterPattern = (Define.MonsterPattern)br.ReadInt32();
            Managers.Instance.DataManager.GameMap.MonsterAISync(monsterId, monsterPos, monsterCellPos, destCellPos, monsterPattern);
        }
    }

    private void PacketHandler_UDP_PLAYERSYNC(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int playerID = br.ReadInt32();

        float tsX = br.ReadSingle();
        float tsY = br.ReadSingle();
        float tsZ = br.ReadSingle();

        Vector3 ts = new Vector3(tsX, tsY, tsZ);

        Define.CreatureState SM = (Define.CreatureState)br.ReadInt32();
        Define.MoveDir DIR = (Define.MoveDir)br.ReadInt32();
        bool Left = (bool)br.ReadBoolean();

        int cellPosx = br.ReadInt32();
        int cellPosy = br.ReadInt32();
        int cellPosz = br.ReadInt32();

        Vector3Int cell = new Vector3Int(cellPosx, cellPosy, cellPosz);

        float jumpStartPosx = br.ReadSingle();
        float jumpStartPosy = br.ReadSingle();
        float jumpStartPosz = br.ReadSingle();

        Vector3 jumpStartPos = new Vector3(jumpStartPosx, jumpStartPosy, jumpStartPosz);

        bool attackFalg = br.ReadBoolean();

        float shadowPosx = br.ReadSingle();
        float shadowPosy = br.ReadSingle();
        float shadowPosz = br.ReadSingle();

        Vector3 shadowPos = new Vector3(shadowPosx, shadowPosy, shadowPosz);

        Managers.Instance.DataManager.GameMap.PlayerSync(playerID, ts, SM, DIR, Left, cell, jumpStartPos, attackFalg, shadowPos);
    }

    private void PacketHandler_S_T_C_GAMEINIT(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 myPlayerId = br.ReadInt32();
        Int32 adminPlayerId = br.ReadInt32();
        Int32 playerCnt = br.ReadInt32();

        List<Define.PlayerInfo> playerInfos = new List<Define.PlayerInfo>();

        for (int i = 0; i < playerCnt; i++)
        {
            Int32 playerId = br.ReadInt32();
            Int32 playerPick = br.ReadInt32();

            Define.PlayerInfo playerInfo = new Define.PlayerInfo();
            playerInfo.playerId = playerId;
            playerInfo.playerType = (Define.PlayerType)playerPick;

            Int32 ipStrSize = br.ReadInt32();

            byte[] ipBytes = br.ReadBytes(ipStrSize);
            string ipStr = Encoding.Unicode.GetString(ipBytes);

            Int32 localIpStrSize = br.ReadInt32();

            byte[] localIpBytes = br.ReadBytes(localIpStrSize);
            string localIpStr = Encoding.Unicode.GetString(localIpBytes);

            Int32 port = br.ReadInt32();

            playerInfo.port = port;
            playerInfo.ipStr = ipStr;
            playerInfo.localIpStr = localIpStr;

            Int32 usernameSize = br.ReadInt32();

            byte[] usernameByte = br.ReadBytes(usernameSize);
            string username = Encoding.Unicode.GetString(usernameByte);

            playerInfo.username = username;

            playerInfos.Add(playerInfo);
        }

        Managers.Instance.DataManager.GameMap.Init(adminPlayerId, myPlayerId, playerInfos);
    }

    private void PacketHandler_UDP_TASK_PING(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();
        Managers.Instance.DataManager.GameMap.UDP_TASK_PING_RECV(playerId);
    }

    private void PacketHandler_UDP_IPPORTINFO(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 ipStrSize = br.ReadInt32();
        byte[] ipBytes = br.ReadBytes(ipStrSize);
        string ip = Encoding.Unicode.GetString(ipBytes);
        Int32 port = br.ReadInt32();

        GameObject.FindGameObjectWithTag("net").GetComponent<Network>().SendUDPINFO(ip, port);
    }

    private void PacketHandler_S_T_C_GAMESTART(ArraySegment<byte> dataPtr, int dataSize)
    {
        GameObject UI = GameObject.FindWithTag("fade");
        GameObject fadeGo = UI.transform.Find("Fade").gameObject;
        fadeGo.SetActive(true);
        UI.transform.Find("Fade").GetComponent<Fade>().FadeOut(Define.SceneType.GameScene1);
    }

    private void PacketHandler_S_T_C_WATINGROOMADMIN(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 adminSQ = br.ReadInt32();
        GameObject go = GameObject.FindGameObjectWithTag("fade");
        UI_WatingRoom wa = go.GetComponent<UI_WatingRoom>();

        wa.ISAdmin(adminSQ);
    }

    private void PacketHandler_S_T_S_CHATSEND_WATINGROOM(ArraySegment<byte> dataPtr, int dataSize)
    {
        GameObject chatBox = GameObject.Find("ChatBox");
        UI_WatingRoomChat chatUI = chatBox.GetComponent<UI_WatingRoomChat>();

        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 strSize = br.ReadInt32();
        byte[] text = br.ReadBytes(strSize);
        string msg = Encoding.Unicode.GetString(text);
        chatUI.Push(msg);
    }

    private void PacketHandler_S_T_C_WATINGROOMOUTUSER(ArraySegment<byte> dataPtr, int dataSize)
    {
        Managers.Instance.SceneManagerEx.LoadScene(Define.SceneType.LobbyScene);
    }

    private void PacketHandler_S_T_C_WATINGROOMINIT(ArraySegment<byte> dataPtr, int dataSize)
    {
        GameObject go = GameObject.FindGameObjectWithTag("fade");
        UI_WatingRoom wa = go.GetComponent<UI_WatingRoom>();
        wa.Sync1(dataPtr, dataSize);
    }

    private void PacketHandler_S_T_C_ROOMLIST(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 roomListSize = br.ReadInt32();
        List<Room> roomList = new List<Room>();

        for (int i = 0; i < roomListSize; i++)
        {
            Int32 roomSq = br.ReadInt32();
            Int32 sq = br.ReadInt32();
            Int32 status = br.ReadInt32();
            Int32 strLen = br.ReadInt32();
            byte[] data = br.ReadBytes(strLen);
            string title = Encoding.Unicode.GetString(data);
            Int32 joinCnt = br.ReadInt32();
            Room room = new Room(roomSq, title, sq, status, joinCnt);
            roomList.Add(room);
        }

        Int32 pageCount = br.ReadInt32();
        Int32 pageNum = br.ReadInt32();

        GameObject roomListGo = GameObject.Find("RoomList");
        UI_RoomList lobby = roomListGo.GetComponent<UI_RoomList>();
        lobby.RoomListSync(roomList, pageCount, pageNum);
    }

    private void PacketHandler_S_T_C_ENTEROOM(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 roomSQ = br.ReadInt32();
        Managers.Instance.DataManager.RoomSQ = roomSQ;

        GameObject UI = GameObject.FindWithTag("fade");
        GameObject fadeGo = UI.transform.Find("Fade").gameObject;
        fadeGo.SetActive(true);
        UI.transform.Find("Fade").GetComponent<Fade>().FadeOut(Define.SceneType.WatingScene);
    }

    private void PacketHandler_S_T_C_CHATSEND_LOBBY(ArraySegment<byte> dataPtr, int dataSize)
    {
        GameObject chatBox = GameObject.Find("ChatBox");
        UI_LobbyChat chatUI = chatBox.GetComponent<UI_LobbyChat>();

        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count); 
        BinaryReader br = new BinaryReader(ms);

        Int32 strSize = br.ReadInt32();
        byte[] text = br.ReadBytes(strSize);
        string msg = Encoding.Unicode.GetString(text);

        chatUI.Push(msg);
    }

    private void PacketHandler_S_T_C_LOGIN(ArraySegment<byte> dataPtr, int dataSize) 
    {
        GameObject UI = GameObject.FindWithTag("fade");
        GameObject fadeGo = UI.transform.Find("Fade").gameObject;
        fadeGo.SetActive(true);
        UI.transform.Find("Fade").GetComponent<Fade>().FadeOut(Define.SceneType.LobbyScene);
    }

    private void PacketHandler_S_T_C_USERLIST(ArraySegment<byte> dataPtr, int dataSize) 
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count); 
        BinaryReader br = new BinaryReader(ms);

        Int32 userListSize = br.ReadInt32();
        List<string> usernameList = new List<string>();

        for (int i = 0; i < userListSize; i++) 
        {
            Int32 strLen = br.ReadInt32();
            byte[] text = br.ReadBytes(strLen);
            string username = Encoding.Unicode.GetString(text);
            usernameList.Add(username);
        }

        Int32 pageCount = br.ReadInt32();
        Int32 pageNum = br.ReadInt32();

        Int32 usernameSize = br.ReadInt32();
        byte[] usernameByte = br.ReadBytes(usernameSize);
        string myName= Encoding.Unicode.GetString(usernameByte);

        TMP_Text myNameText = GameObject.Find("Myname").GetComponent<TMP_Text>();
        myNameText.text = $"´Ð³×ÀÓ: {myName}";

        GameObject userListGo = GameObject.Find("UserListBox");
        UI_UserLIst lobby = userListGo.GetComponent<UI_UserLIst>();
        lobby.UserListSync(usernameList ,pageCount, pageNum);
    }
}
