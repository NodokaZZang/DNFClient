using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class KnightController : MonsterController
{
    private Network _net;
    private bool _coAttack = false;
    private bool _coAttacked = false;
    private bool _coDied = false;
    private GameObject _shadow;
    private int _monsterId;
    private int _attack = 120;
    private int _targetId = 0;
    private MonsterHPController HpC;
    private int _skillDamage1 = 100;
    private bool _coSkill1 = false;

    public override void Init()
    {
        HpC = transform.GetChild(1).GetComponent<MonsterHPController>();
        _hp = 200;
        _speed = 4.0f;
        MonsterType = Define.MonsterType.Knight;
        CSpriteRenderer.sortingOrder = (int)Define.SortOrder.Monster;
        _shadow = Utils.FindChild(gameObject, "Shadow");
        _net = GameObject.FindWithTag("net").GetComponent<Network>();
        HpC.Init(_hp);
    }

    public void Init(int monsterId, Vector3Int startPos)
    {
        _monsterId = monsterId;
        CellPos = startPos;
        Vector3 worldPos = Managers.Instance.MapManager.CovnertWorldPos(CellPos);
        Vector3 myPos = new Vector3(worldPos.x, worldPos.y, worldPos.y);
        transform.position = new Vector3(myPos.x, myPos.y, myPos.y);
    }

    public override void UpdateAnimation()
    {
        switch (SM.State)
        {
            case Define.CreatureState.None:
            case Define.CreatureState.Idel:
                CAnimator.Play("KnightIdel");
                break;

            case Define.CreatureState.Move:
                CAnimator.Play("KnightMove");
                break;

            case Define.CreatureState.Jump:
                break;

            case Define.CreatureState.Attack1:
                CAnimator.Play("KnightAttack");
                break;

            case Define.CreatureState.Attack2:
                break;

            case Define.CreatureState.Attack3:
                break;

            case Define.CreatureState.Attacked:
                CAnimator.Play("KnightAttacked");
                break;

            case Define.CreatureState.Die:
                CAnimator.Play("KnightDead");
                break;

            case Define.CreatureState.Skill:
                CAnimator.Play("KnightSkill");
                break;
        }
    }

    public override void UpdateIdle()
    {

    }

    public override void UpdateMove()
    {
        Vector3Int nowCellPos = CellPos;

        if (DetectEnemy(nowCellPos))
            return;

        Vector3Int moveCellDir = DestPos - CellPos;
        Vector3Int moveVector = Vector3Int.zero;
        Vector3 moveVector3 = Vector3Int.zero;
        if (moveCellDir.x < 0)
        {
            Dir = Define.MoveDir.Left;
            moveVector = Vector3Int.left;
            moveVector3 = Vector3.left;
            Left = true;
        }
        else if (moveCellDir.x > 0)
        {
            Dir = Define.MoveDir.Right;
            moveVector = Vector3Int.right;
            moveVector3 = Vector3.right;
            Left = false;
        }
        else if (moveCellDir.y < 0)
        {
            Dir = Define.MoveDir.Down;
            moveVector = Vector3Int.down;
            moveVector3 = Vector3.down;
        }
        else if (moveCellDir.y > 0)
        {
            Dir = Define.MoveDir.Up;
            moveVector = Vector3Int.up;
            moveVector3 = Vector3.up;
        }
        else
        {
            Dir = Define.MoveDir.None;
        }

        Vector3Int targetPos = CellPos + moveVector;

        Vector3 worldPos = Managers.Instance.MapManager.CovnertWorldPos(targetPos);
        Vector3 startPos = new Vector3(worldPos.x, worldPos.y, worldPos.y);
        Vector3 destPos = startPos + new Vector3(0.5f, 0.25f, 0.25f);
        Vector3 moveDir = destPos - transform.position;

        Vector3 movePos = transform.position + (moveVector3 * Time.deltaTime * _speed);

        if (Managers.Instance.MapManager.CanGo(movePos))
        {
            if (moveDir.magnitude < Time.deltaTime * _speed)
            {
                transform.position = destPos;
            }
            else
            {
                transform.position += moveDir.normalized * Time.deltaTime * _speed;
            }
        }
        else
        {
            // 지나갈수 없음
            SM.SetSpeed(0);
        }

        CellPos = Managers.Instance.MapManager.CovnertCellPos(transform.position);
    }

    private bool DetectEnemy(Vector3Int nowCellPos)
    {
        bool ret = false;
        int skillRange = 5;
        int swordRange = 2;
        bool attackFlag = false;
        bool skillFlag = false;
        bool swordFlag = false;


        foreach (var playerPair in Managers.Instance.DataManager.GameMap.PlayerDict)
        {
            CreatureController cc = playerPair.Value;

            Vector3Int playerCellPos = Managers.Instance.MapManager.CovnertCellPos(cc.transform.position);

            if (playerCellPos.y != nowCellPos.y)
                continue;


            if (playerCellPos.x <= nowCellPos.x && playerCellPos.x >= nowCellPos.x - swordRange)
            {
                attackFlag = true;
                Left = true;
                _targetId = cc.PlayerId;
                ret = true;
                swordFlag = true;
                break;
            }

            if (playerCellPos.x >= nowCellPos.x && playerCellPos.x <= nowCellPos.x + swordRange)
            {
                attackFlag = true;
                Left = false;
                _targetId = cc.PlayerId;
                ret = true;
                swordFlag = true;
                break;
            }


            if (playerCellPos.x <= nowCellPos.x && playerCellPos.x >= nowCellPos.x - skillRange)
            {
                attackFlag = true;
                Left = true;
                _targetId = cc.PlayerId;
                ret = true;
                skillFlag = true;
                break;
            }

            if (playerCellPos.x >= nowCellPos.x && playerCellPos.x <= nowCellPos.x + skillRange)
            {
                attackFlag = true;
                Left = false;
                _targetId = cc.PlayerId;
                ret = true;
                skillFlag = true;
                break;
            }
        }

        if (attackFlag)
        {
            SM.SetSpeed(0);

            if (swordFlag)
            {
                SM.SetDamage(_attack);
            }
            else if (skillFlag)
            {
                SM.SetSkill(_skillDamage1);
            }
        }
        return ret;
    }

    public override void UpdateSkill()
    {
        if (_coSkill1 == false)
        {
            StartCoroutine(CoSkill1());
        }
    }

    IEnumerator CoSkill1()
    {
        _coSkill1 = true;
        yield return new WaitForSeconds(0.6f);
        Shot();
        SM.SetSkill(0);
        MonsterPattern = Define.MonsterPattern.Idel;
        _coSkill1 = false;
    }

    public override void UpdateAttack1()
    {
        if (_coAttack == false)
            StartCoroutine(CoKnightAttack1());
    }

    public override void UpdateAttacked()
    {
        if (_coAttacked == false)
            StartCoroutine(CoAttacekd());
    }

    public override void UpdateLeft()
    {
        base.UpdateLeft();
    }

    public override void UpdateAI()
    {
        UpdateAIStart();

        UpdateAIState();

        UpdateAIEnd();
    }

    public override void Attacked(int damage)
    {
        _hp -= damage;
        SM.SetDamaged(damage);
        HpC.Attack(damage);

        if (_hp <= 0)
        {
            _hp = 0;
            SM.SetDie(true);
        }
    }

    public void AttackedSync(int damage)
    {
        _hp -= damage;
        SM.SetDamaged(damage);
        HpC.Attack(damage);

        if (_hp <= 0)
        {
            _hp = 0;
            SM.SetDie(true);
        }
    }

    public override void Attack()
    {
        bool targetFind = false;

        // 제대로 조회
        foreach (var playerPair in Managers.Instance.DataManager.GameMap.PlayerDict)
        {
            CreatureController cc = playerPair.Value;
   
            Vector3Int playerCellPos = Managers.Instance.MapManager.CovnertCellPos(cc.gameObject.transform.position);

            if (playerCellPos.y != CellPos.y)
                continue;

            if (Left == true && playerCellPos.x <= CellPos.x && playerCellPos.x >= CellPos.x - 2)
            {
                targetFind = true;
                _targetId = cc.PlayerId;
                SM.SetSpeed(0);
                SM.SetDamage(0);
                MonsterPattern = Define.MonsterPattern.Patrol;
                break;
            }

            if (Left == false && playerCellPos.x >= CellPos.x && playerCellPos.x <= CellPos.x + 2)
            {
                targetFind = true;
                _targetId = cc.PlayerId;
                SM.SetSpeed(0);
                SM.SetDamage(0);
                MonsterPattern = Define.MonsterPattern.Patrol;
                break;
            }
        }

        if (targetFind == false) return;


        if (Managers.Instance.DataManager.GameMap.IsAdmin == false) return;

        // 어드민임 
        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);
        BinaryWriter bw = new BinaryWriter(ms);

        Int16 pktHeader = (Int16)(8 + 4);

        bw.Write((Int16)Define.PacketProtocol.UDP_PLAYERATTACKED);
        bw.Write((Int16)pktHeader);

        Managers.Instance.DataManager.GameMap.PlayerDict.TryGetValue(_targetId, out var pc);

        if (pc == null) return;

        pc.Attacked(_attack);

        // PlayerId
        bw.Write((Int32)_targetId);
        // Damage 
        bw.Write((Int32)_attack);

        _net.UDPBrodCast(bytes, pktHeader);
    }

    public override void UpdateDie()
    {
        if (_coDied == false)
        {
            StartCoroutine(CoDied());
        }
    }

    private void UpdateAIStart()
    {
        switch (MonsterPattern)
        {
            case Define.MonsterPattern.Idel:
                break;

            case Define.MonsterPattern.Patrol:
                UpdatePatrol();
                break;

            case Define.MonsterPattern.Attack:
                break;

            case Define.MonsterPattern.Skill1:
                break;
        }
    }

    private void UpdateAIState()
    {
        switch (MonsterPattern)
        {
            case Define.MonsterPattern.Idel:
                SM.SetSpeed(0);
                break;

            case Define.MonsterPattern.Patrol:
                SM.SetSpeed(_speed);
                break;

            case Define.MonsterPattern.Attack:
                SM.SetSpeed(_speed);
                break;
        }
    }

    private void UpdateAIEnd()
    {
        switch (MonsterPattern)
        {
            case Define.MonsterPattern.Idel:
                MonsterPattern = Define.MonsterPattern.Patrol;
                break;

            case Define.MonsterPattern.Patrol:
                MonsterPattern = Define.MonsterPattern.Idel;
                break;
        }
    }

    private void UpdatePatrol()
    {
        Vector3Int nowCellPos = CellPos;

        int randomPosX = Random.Range(-4, 3);
        int randomPosY = Random.Range(-4, 3);

        DestPos = new Vector3Int(nowCellPos.x + randomPosX, nowCellPos.y + randomPosY);

        int swordAirRange = 5;

        foreach (var playerPair in Managers.Instance.DataManager.GameMap.PlayerDict)
        {
            CreatureController pc = playerPair.Value;

            Vector3Int playerCellPos = Managers.Instance.MapManager.CovnertCellPos(pc.gameObject.transform.position);

            if (playerCellPos.y != nowCellPos.y)
                continue;

            int swordAirRight = nowCellPos.x + swordAirRange;
            int swordAirLeft = nowCellPos.x - swordAirRange;

            if (playerCellPos.x <= nowCellPos.x && playerCellPos.x >= swordAirLeft)
            {
                MonsterPattern = Define.MonsterPattern.Attack; // 검풍
                break;
            }

            if (playerCellPos.x >= nowCellPos.x && playerCellPos.x <= swordAirRight)
            {
                MonsterPattern = Define.MonsterPattern.Attack;
                break;
            }
        }
    }

    IEnumerator CoKnightAttack1()
    {
        _coAttack = true;
        yield return new WaitForSeconds(0.2f);
        // 만약 플레이어가 방장일때만 패킷 전송
        Attack();
        yield return new WaitForSeconds(0.3f);
        SM.SetDamage(0);
        _coAttack = false;
    }

    IEnumerator CoAttacekd()
    {
        _coAttacked = true;
        CSpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        CSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        SM.SetDamaged(0);
        _coAttacked = false;
    }

    IEnumerator CoDied()
    {
        _coDied = true;

        yield return new WaitForSeconds(0.8f);

        _coDied = false;

        Managers.Instance.DataManager.GameMap.MonsterDict.Remove(MonsterID);
        Destroy(gameObject);
    }

    void Shot()
    {
        GameObject effect = Managers.Instance.ResourceManager.Instantiate("Game/Object/KnightEffect");
        effect.GetComponent<KEController>().Init(_net, _skillDamage1, Left, CellPos);
    }
}
