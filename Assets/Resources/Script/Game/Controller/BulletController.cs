using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    protected Vector3Int _cellPos = Vector3Int.zero;
    protected Vector3Int _destPos = Vector3Int.zero;
    protected Define.MoveDir _dir = Define.MoveDir.None;
    private int _intersection = 10;
    private int _speed = 10;
    private int _damage = 0;
    private Define.CreatureType _targetType = Define.CreatureType.None;
    private bool _left = false;
    private Network _net;
    public void Init(Network net, Define.CreatureType targetType, int damage, bool left, Vector3Int startPos)
    {
        _net = net;
        _cellPos = startPos;
        _damage = damage;
        _targetType = targetType;
        _left = left;

        switch (_targetType)
        {
            case Define.CreatureType.Player:
                InitPlayerBullet();
                break;

            case Define.CreatureType.OtherPlayer:
                InitPlayerBullet();
                break;

            case Define.CreatureType.Monster:
                InitMonsterBullet();
                break;
        }
    }

    private void InitPlayerBullet()
    {
        if (_left)
        {
            _dir = Define.MoveDir.Left;
            _destPos = new Vector3Int(_cellPos.x - _intersection, _cellPos.y, _cellPos.y);
            transform.position = Managers.Instance.MapManager.CovnertWorldPos(_cellPos) + new Vector3(-2.3f, 2.7f);
        }
        else
        {
            _dir = Define.MoveDir.Right;
            _destPos = new Vector3Int(_cellPos.x + _intersection, _cellPos.y, _cellPos.y);
            transform.position = Managers.Instance.MapManager.CovnertWorldPos(_cellPos) + new Vector3(2.3f, 2.7f);
        }
    }

    private void InitMonsterBullet()
    {
        if (_left)
        {
            _dir = Define.MoveDir.Left;
            _destPos = new Vector3Int(_cellPos.x - _intersection, _cellPos.y, _cellPos.y);
            transform.position = Managers.Instance.MapManager.CovnertWorldPos(_cellPos) + new Vector3(-1.5f, 1.5f);
        }
        else
        {
            _dir = Define.MoveDir.Right;
            _destPos = new Vector3Int(_cellPos.x + _intersection, _cellPos.y, _cellPos.y);
            transform.position = Managers.Instance.MapManager.CovnertWorldPos(_cellPos) + new Vector3(1.5f, 1.5f);
        }
    }

    void Update()
    {
        switch (_targetType)
        {
            case Define.CreatureType.Player:
                UpdatePlayerBullet();
                break;

            case Define.CreatureType.OtherPlayer:
                UpdateOthrePlayerBullet();
                break;

            case Define.CreatureType.Monster:
                UpdateMonsterBullet();
                break;
        }
    }

    private void UpdateOthrePlayerBullet()
    {
        try
        {
            Vector3 moveDir;
            Vector3Int nowPos = Managers.Instance.MapManager.CovnertCellPos(transform.position - new Vector3(0f, 2.7f));

            if (_dir == Define.MoveDir.Left)
                moveDir = Vector3.left;

            else
                moveDir = Vector3.right;

            bool deleteBullet = false;

            foreach (var monsgerGo in Managers.Instance.DataManager.GameMap.MonsterDict.Values)
            {
                MonsterController mc = monsgerGo.GetComponent<MonsterController>();

                Vector3Int monsterCellPos = mc.CellPos;

                if (nowPos.x == monsterCellPos.x && nowPos.y == monsterCellPos.y)
                {
                    Destroy(gameObject);
                    deleteBullet = true;
                    break;
                }
            }

            if (deleteBullet == false && nowPos.x == _destPos.x)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += moveDir * Time.deltaTime * _speed;
        }
        catch (Exception e)
        {
            Destroy(gameObject);
            Debug.LogException(e);
        }
    }

    private void UpdatePlayerBullet()
    {
        try
        {
            Vector3 moveDir;
            Vector3Int nowPos = Managers.Instance.MapManager.CovnertCellPos(transform.position - new Vector3(0f, 2.7f));

            if (_dir == Define.MoveDir.Left)
                moveDir = Vector3.left;

            else
                moveDir = Vector3.right;

            bool deleteBullet = false;

            foreach (var monsgerGo in Managers.Instance.DataManager.GameMap.MonsterDict.Values)
            {
                MonsterController mc = monsgerGo.GetComponent<MonsterController>();

                Vector3Int monsterCellPos = mc.CellPos;

                if (nowPos.x == monsterCellPos.x && nowPos.y == monsterCellPos.y)
                {
                    // 몬스터 피격
                    byte[] bytes = new byte[1024];
                    MemoryStream ms = new MemoryStream(bytes);
                    BinaryWriter bw = new BinaryWriter(ms);

                    Int16 pktHeader = (Int16)(8 + 4 + 2);

                    bw.Write((Int16)Define.PacketProtocol.UDP_MONSTERATTACKED);
                    bw.Write((Int16)pktHeader);
                    bw.Write((Int16)1);

                    mc.Attacked(_damage);
                    // MonsterID
                    bw.Write((Int32)mc.MonsterID);
                    // Damage 
                    bw.Write((Int32)_damage);

                    _net.UDPBrodCast(bytes, pktHeader);
                    Destroy(gameObject);
                    deleteBullet = true;
                    break;
                }
            }

            if (deleteBullet == false && nowPos.x == _destPos.x)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += moveDir * Time.deltaTime * _speed;
        }
        catch (Exception e)
        {
            Destroy(gameObject);
            Debug.Log(e);
        }
    }

    private void UpdateMonsterBullet()
    {
        try
        {
            Vector3 moveDir;
            Vector3Int nowPos = Managers.Instance.MapManager.CovnertCellPos(transform.position - new Vector3(0f, 1.5f));

            if (_dir == Define.MoveDir.Left)
                moveDir = Vector3.left;

            else
                moveDir = Vector3.right;

            bool deleteBullet = false;

            if (Managers.Instance.DataManager.GameMap.IsAdmin)
            {
                foreach (var playerGo in Managers.Instance.DataManager.GameMap.PlayerDict.Values)
                {
                    CreatureController cc = playerGo.GetComponent<CreatureController>();

                    Vector3Int playerCellPos = cc.CellPos;

                    if (nowPos.x == playerCellPos.x && nowPos.y == playerCellPos.y)
                    {
                        // 몬스터 피격
                        byte[] bytes = new byte[1024];
                        MemoryStream ms = new MemoryStream(bytes);
                        BinaryWriter bw = new BinaryWriter(ms);

                        Int16 pktHeader = (Int16)(8 + 4);

                        bw.Write((Int16)Define.PacketProtocol.UDP_PLAYERATTACKED);
                        bw.Write((Int16)pktHeader);

                        cc.Attacked(_damage);
                        // PlayerId
                        bw.Write(cc.PlayerId);
                        // Damage 
                        bw.Write((Int32)_damage);

                        _net.UDPBrodCast(bytes, pktHeader);
                        Destroy(gameObject);
                        deleteBullet = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (var playerGo in Managers.Instance.DataManager.GameMap.PlayerDict.Values)
                {
                    CreatureController cc = playerGo.GetComponent<CreatureController>();

                    Vector3Int playerCellPos = cc.CellPos;

                    if (nowPos.x == playerCellPos.x && nowPos.y == playerCellPos.y)
                    {
                        Destroy(gameObject);
                        deleteBullet = true;
                        break;
                    }
                }
            }

            if (deleteBullet == false && nowPos.x == _destPos.x)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += moveDir * Time.deltaTime * _speed;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Destroy(gameObject);
        }
    }
}
