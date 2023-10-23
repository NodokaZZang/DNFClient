using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Define 
{
    public enum SceneType : int 
    {
        BaseScene,
        LoginScene,
        LobbyScene,
        WatingScene,
        GameScene1,
        GameScene2,
        GameScene3,
    }

    public struct Header
    {
        public UInt16 _pktId;
        public UInt16 _pktSize;
    };

    public struct CameraTarget
    {
        public int playerId;
        public GameObject target;
    }

    public enum PacketProtocol : Int16
    {
        C_T_S_LOGIN,
        C_T_S_USERLIST,
        C_T_S_ROOMLIST,
        S_T_C_USERLIST,
        S_T_C_ROOMLIST,
        S_T_C_LOGIN,
        C_T_S_CHATSEND_LOBBY,
        S_T_C_CHATSEND_LOBBY,
        C_T_S_CREATEROOM,
        C_T_S_ENTEROOM,
        S_T_C_ENTEROOM,
        C_T_S_WATINGROOMINIT,
        S_T_C_WATINGROOMINIT,
        C_T_S_WATINGROOMOUTUSER,
        S_T_C_WATINGROOMOUTUSER,
        C_T_S_CHATSEND_WATINGROOM,
        S_T_S_CHATSEND_WATINGROOM,
        C_T_S_USERPICK,
        C_T_S_USERREADY,
        S_T_C_WATINGROOMADMIN,
        S_T_C_GAMESTART,
        C_T_S_GAMEINIT,
        S_T_C_GAMEINIT,
        S_T_C_GAMESTAGEINIT,
        C_T_S_PLAYERPOSSYNC,
        S_T_C_PLAYERPOSSYNC,
        C_T_S_MONSTERSYNC,
        S_T_C_MONSTERSYNC,
        C_T_S_PLAYERATTACK,
        S_T_C_PLAYERATTACK,
        UDP_PLAYERSYNC,
        C_T_S_UDPIPPORTSEND,
        UDP_MONSTERSPAWN,
        UDP_MONSTERSYNC,
        UDP_MONSTERATTACKED,
        UDP_PLAYERATTACKED,
        C_T_S_GAMEEND,
        UDP_HPHEEL,
        UDP_HPHEELSPAWN,
        C_T_S_IPPORTINFO,
        S_T_C_IPPORTINFO,
        C_T_S_UDPPING,
        UDP_PING,
        UDP_TASK_PING,
        UDP_TASK_PING_CALLBACK,
        UDP_TASK_UP_PING,
        UDP_NEXT_STAGE,
        UDP_TASK_COMPLETE_PING,
        UDP_TASK_COMPLETE_PING_CALLBACK,
        UDP_LOADING_END,
        UDP_LOADING_END_CALL_BACK,
    };

    public enum CreatureState : int
    {
        None = 0,
        Idel = 1,
        Move = 2,
        Jump = 3,
        Attack1 = 4,
        Attack2 = 5,
        Attack3 = 6,
        Attacked = 7,
        Die = 8,
        Skill = 9,
    }

    public enum MoveDir : int
    {
        None = 0,
        Up = 1,
        Down = 2,
        Right = 3,
        Left = 4,
    }

    public enum PlayerType : int 
    {
        None = 0,
        SwordMan = 1,
        Gunner = 2
    }

    public enum SortOrder : int 
    {
        Player = 4,
        Monster = 4
    }

    public enum MonsterPattern : int 
    {
        Idel,
        Patrol,
        Trace,
        Attack,
        Skill1,
        Skill2,
        Skill3,
    }

    public enum MonsterType : int 
    {
        GuardMan,
        GunMan,
        Knight,
    }

    public struct PlayerInfo 
    {
        public int playerId;
        public PlayerType playerType;
        public string ipStr ;
        public int port;
        public string username;
        public string localIpStr;
    }

    public struct PlayerData 
    {
        public int playerId;
        public int hp;
        public int hpMax;
        public PlayerType playerType;
        public string username;
    }

    public struct PlayerUDPInfo 
    {
        public IPEndPoint ipep;
        public int playerId;
    }

    public enum CreatureType : int 
    {
        None,
        Player,
        OtherPlayer,
        Monster,
    }

    public enum GameMapState : int 
    {
        None,
        Init,
        Loading,
        Play,
        End,
        Stop,
        Defeat,
        NextStage,
    }
}

