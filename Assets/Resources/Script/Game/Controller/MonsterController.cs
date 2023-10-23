using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : CreatureController
{
    public Define.MonsterType MonsterType { get; set; }
    public Define.MonsterPattern MonsterPattern = Define.MonsterPattern.Idel;
    public int MonsterID = 0;
    public Vector3Int DestPos { get; set; } = Vector3Int.zero;
    protected float _speed = 3.0f;
    internal void MonsterAISync(int monsterId, Vector3 monsterPos, Vector3Int monsterCellPos, Vector3Int destCellPos, Define.MonsterPattern pattern)
    {
        MonsterID = monsterId;
        transform.position = monsterPos;
        CellPos = monsterCellPos;
        DestPos = destCellPos;
        MonsterPattern = pattern;

        switch (MonsterPattern)
        {
            case Define.MonsterPattern.Idel:
                SM.SetSpeed(_speed);
                break;

            case Define.MonsterPattern.Patrol:
                SM.SetSpeed(0);
                break;

            case Define.MonsterPattern.Trace:
                SM.SetSpeed(_speed);
                break;

            case Define.MonsterPattern.Attack:
                SM.SetSpeed(_speed);
                break;
        }
    }
}
