using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KEController : MonoBehaviour
{
    protected Vector3Int _cellPos = Vector3Int.zero;
    protected Vector3Int _destPos = Vector3Int.zero;
    protected Define.MoveDir _dir = Define.MoveDir.None;
    private int _intersection = 10;
    private int _speed = 10;
    private int _damage = 0;
    private bool _left = false;
    private Network _net;
    private HashSet<int> _playerIdAttack = new HashSet<int>();
    protected SpriteRenderer CSpriteRenderer { get; set; }

    public void Init(Network net, int damage, bool left, Vector3Int startPos)
    {
        CSpriteRenderer = GetComponent<SpriteRenderer>();
        _net = net;
        _cellPos = startPos;
        _damage = damage;
        _left = left;

        Init();
    }

    private void Init()
    {
        if (_left)
        {
            CSpriteRenderer.flipX = true;
            _dir = Define.MoveDir.Left;
            _destPos = new Vector3Int(_cellPos.x - _intersection, _cellPos.y, _cellPos.y);
            transform.position = Managers.Instance.MapManager.CovnertWorldPos(_cellPos) + new Vector3(0.0f, 0.5f);
        }
        else
        {
            CSpriteRenderer.flipX = false;
            _dir = Define.MoveDir.Right;
            _destPos = new Vector3Int(_cellPos.x + _intersection, _cellPos.y, _cellPos.y);
            transform.position = Managers.Instance.MapManager.CovnertWorldPos(_cellPos) + new Vector3(0.0f, 0.5f);
        }
    }

    void Update()
    {

        Vector3 moveDir;
        Vector3Int nowPos = Managers.Instance.MapManager.CovnertCellPos(transform.position - new Vector3(0.0f, 0.5f));

        if (_dir == Define.MoveDir.Left)
            moveDir = Vector3.left;

        else
            moveDir = Vector3.right;

        if (Managers.Instance.DataManager.GameMap.IsAdmin)
        {
            foreach (var cc in Managers.Instance.DataManager.GameMap.PlayerDict.Values)
            {
                if (_playerIdAttack.Contains(cc.PlayerId)) continue;

                Vector3Int playerCellPos = cc.CellPos;

                if (nowPos.x == playerCellPos.x && nowPos.y == playerCellPos.y)
                {
                    _playerIdAttack.Add(cc.PlayerId);

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
                }
            }
        }

        if (nowPos.x == _destPos.x)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += moveDir * Time.deltaTime * _speed;
    }
}
