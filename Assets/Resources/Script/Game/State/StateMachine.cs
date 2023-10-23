using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public Define.CreatureState State { get; set; } = Define.CreatureState.Idel;
    
    private float _speed = 0;
    private float _jumpPower = 0;
    private int _damage = 0;
    private int _damaged = 0;
    private int _skillDamage = 0;
    private bool _continuousAttack = false;
    public float gravity = 2.7f;
    private bool _die = false;

    public void SetSpeed(float speed) { _speed = speed; }
    public void SetJumpPower(float jumpPower) { _jumpPower = jumpPower; }
    public void SetDamage(int damage) { _damage = damage; }
    public void SetContinueAttack(bool continueAttack) { _continuousAttack = continueAttack; }
    public void SetDamaged(int damage) { _damaged = damage; }

    public void SetSkill(int skill) { _skillDamage = skill; }
    public void UpdateState() 
    {
        switch (State) 
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
    }

    private void UpdateSkill()
    {
        if (_skillDamage <= 0)
        {
            State = Define.CreatureState.Idel;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }

    private void UpdateIdle() 
    {
        if (_speed > 0)
        {
            State = Define.CreatureState.Move;
        }

        if (_damage > 0) 
        {
            State = Define.CreatureState.Attack1;
        }

        if (_skillDamage > 0) 
        {
            State = Define.CreatureState.Skill;
        }

        if (_jumpPower > 0) 
        {
            State = Define.CreatureState.Jump;
        }

        if (_damaged > 0) 
        {
            State = Define.CreatureState.Attacked;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }

    private void UpdateMove() 
    {
        if (_speed <= 0)
        {
            State = Define.CreatureState.Idel;
        }

        if (_damage > 0)
        {
            State = Define.CreatureState.Attack1;
        }

        if (_skillDamage > 0)
        {
            State = Define.CreatureState.Skill;
        }

        if (_jumpPower > 0)
        {
            State = Define.CreatureState.Jump;
        }

        if (_damaged > 0)
        {
            State = Define.CreatureState.Attacked;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }
    private void UpdateJump() 
    {
        if (_jumpPower <= 0)
            State = Define.CreatureState.Idel;
    }

    private void UpdateAttack1() 
    {
        if (_damaged > 0)
        {
            State = Define.CreatureState.Attacked;
            return;
        }

        if (_continuousAttack) 
        {
            State = Define.CreatureState.Attack2;
            _continuousAttack = false;
        }

        if (_damage <= 0)
        {
            State = Define.CreatureState.Idel;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }
    private void UpdateAttack2() 
    {
        if (_damaged > 0)
        {
            State = Define.CreatureState.Attacked;
            return;
        }

        if (_continuousAttack) 
        {
            State = Define.CreatureState.Attack3;
            _continuousAttack = false;
        }

        if (_damage <= 0)
        {
            State = Define.CreatureState.Idel;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }
    private void UpdateAttack3() 
    {
        if (_damaged > 0)
        {
            State = Define.CreatureState.Attacked;
            return;
        }

        if (_damage <= 0) 
        {
            State = Define.CreatureState.Idel;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }
    private void UpdateAttacked() 
    {
        if (_damaged <= 0) 
        {
            State = Define.CreatureState.Idel;
        }

        if (_die)
        {
            State = Define.CreatureState.Die;
        }
    }

    private void UpdateDie() 
    {
       
    }

    internal void SetDie(bool v)
    {
        _die = v;
    }
}
