using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room 
{
    private int _roomSq;
    private string _title;
    private int _oreder;
    private int _status;
    private int _joinCnt;

    public Room (int roomSq, string title, int oreder, int status, int joinCnt)
    {
        _roomSq = roomSq;
        _title = title;
        _oreder = oreder;
        _status = status;
        _joinCnt = joinCnt;
    }

    public int RoomSQ { get { return _roomSq; } set { _roomSq = value; } }
    public string Title { get { return _title; } set { _title = value; } }
    public int Order { get { return _oreder; } set { _oreder = value; } }
    public int Status { get { return _status; } set { _status = value; } }
    public int JoinCnt { get { return _joinCnt; } set { _joinCnt = value; }  }
}
