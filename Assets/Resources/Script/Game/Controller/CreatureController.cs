using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public GameObject Username { get; set; } = null;
    public Vector3Int CellPos { get; set; } = Vector3Int.zero;
    protected bool Left { get; set; } = false;
    protected Define.MoveDir Dir { get; set; } = Define.MoveDir.None;
    protected SpriteRenderer CSpriteRenderer { get; set; }
    public StateMachine SM { get; set; } = new StateMachine();
    protected Animator CAnimator { get; set; }
    protected Vector3 JumpStartPos { get; set; } = Vector3.zero;

    protected float _gravity = 2.7f;

    protected bool _attackFlag = false;

    public int PlayerId { get; set; } = 0;

    public int HP { get { return _hp; } set { _hp = value; } }

    protected int _hp = 0;
    protected int _hpMax = 0;

    void Start()
    {
        CSpriteRenderer = GetComponent<SpriteRenderer>();
        CAnimator = GetComponent<Animator>();
        Init();
    }

    void Update()
    {
        if ( (Managers.Instance.DataManager.GameMap && Managers.Instance.DataManager.GameMap.GMS != Define.GameMapState.Play) && (Managers.Instance.DataManager.GameMap && Managers.Instance.DataManager.GameMap.GMS != Define.GameMapState.NextStage) )
            return;

        Define.CreatureState prevSM = SM.State;
        Define.MoveDir prevDir = Dir;
        bool prevLeft = Left;
        bool prevAttackFlag = _attackFlag;

        UpdateInupt();

        UpdateUsername();

        SM.UpdateState();

        switch (SM.State)
        {
            case Define.CreatureState.Idel:
                UpdateIdle();
                break;

            case Define.CreatureState.Move:
                UpdateMove();
                break;

            case Define.CreatureState.Jump:
                UpdateJump();
                break;

            case Define.CreatureState.Attack1:
                UpdateAttack1();
                break;

            case Define.CreatureState.Attack2:
                UpdateAttack2();
                break;

            case Define.CreatureState.Attack3:
                UpdateAttack3();
                break;

            case Define.CreatureState.Attacked:
                UpdateAttacked();
                break;

            case Define.CreatureState.Die:
                UpdateDie();
                break;

            case Define.CreatureState.Skill:
                UpdateSkill();
                break;
        }

        UpdateLeft();
        UpdateAnimation();

        if (prevSM != SM.State || prevDir != Dir || prevLeft != Left || prevAttackFlag != _attackFlag)
            UpdateDetect();

        _attackFlag = false;
    }

    internal void HpUP(int hp)
    {
        int nextHp = _hp + hp;
        if (_hpMax < nextHp)
        {
            _hp = _hpMax;
        }
        else
        {
            _hp = nextHp;
        }

        Managers.Instance.DataManager.GameMap.PlayerHpcDict.TryGetValue(PlayerId, out var hpc);

        if (hpc != null)
        {
            hpc.HpUP(hp);
        }
    }

    public virtual void UpdateDie()
    {

    }

    public virtual void UpdateSkill()
    {

    }

    public virtual void Init()
    {

    }

    public virtual void UpdateAnimation()
    {

    }

    public virtual void UpdateIdle()
    {

    }

    public virtual void UpdateMove()
    {

    }

    public virtual void UpdateJump()
    {

    }

    public virtual void UpdateAttack1()
    {

    }

    public virtual void UpdateAttack2()
    {

    }
    public virtual void UpdateAttack3()
    {

    }
    public virtual void UpdateAttacked()
    {

    }

    public virtual void UpdateInupt()
    {

    }

    public virtual void UpdateAI()
    {

    }

    public virtual void UpdateUsername() { }

    public virtual void UpdateLeft()
    {
        if (Left)
            CSpriteRenderer.flipX = true;

        else
            CSpriteRenderer.flipX = false;
    }

    public virtual void UpdateDetect()
    {

    }

    public virtual void Attacked(int damage) { }

    public virtual void Attack() { }
}
