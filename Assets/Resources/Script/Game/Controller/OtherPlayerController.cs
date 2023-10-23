using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OtherPlayerController : CreatureController
{
    private float _speed = 3.0f;
    private int _attack = 15;
    private float _jumpPower = 4.0f;
    public Define.PlayerType _playType = Define.PlayerType.SwordMan;
    private Network _net;
    private bool _coAttack = false;
    private bool _continueAttack = false;
    private GameObject _shadow;
    private Vector3 _shadowPos;
    private int _swordManAttackRange = 1;
    private bool _coAttacked = false;
    private bool _coDied = false;
    private bool _coSkill = false;

    public override void Init()
    {
        _jumpPower = 4.0f;
        CSpriteRenderer.sortingOrder = (int)Define.SortOrder.Player;
        _shadow = Utils.FindChild(gameObject, "Shadow");
        _shadowPos = _shadow.transform.position;
        _net = GameObject.FindWithTag("net").GetComponent<Network>();
    }

    public void Init(Vector3Int startPos, Define.PlayerType playerType)
    {
        CellPos = startPos;
        Vector3 pos = Managers.Instance.MapManager.CovnertWorldPos(CellPos);
        transform.position = new Vector3(pos.x, pos.y, pos.y);
        _playType = playerType;

        switch (_playType)
        {
            case Define.PlayerType.SwordMan:
                _hpMax = 500;
                _hp = 500;
                _attack = 15;
                _speed = 3.0f;
                break;

            case Define.PlayerType.Gunner:
                _hpMax = 300;
                _hp = 300;
                _attack = 5;
                _speed = 3.5f;
                break;
        }
    }

    public override void UpdateAnimation()
    {
        switch (_playType)
        {
            case Define.PlayerType.SwordMan:
                UpdateSwordManAnimation();
                break;

            case Define.PlayerType.Gunner:
                UpdateGunnerAnimation();
                break;
        }
    }

    public override void UpdateIdle()
    {

    }

    public override void UpdateMove()
    {
        Vector3 movePos = transform.position;

        switch (Dir)
        {
            case Define.MoveDir.Up:
                movePos += Vector3.up * Time.deltaTime * _speed;
                break;

            case Define.MoveDir.Down:
                movePos += Vector3.down * Time.deltaTime * _speed;
                break;

            case Define.MoveDir.Right:
                movePos += Vector3.right * Time.deltaTime * _speed;
                break;

            case Define.MoveDir.Left:
                movePos += Vector3.left * Time.deltaTime * _speed;
                break;
        }

        if (Managers.Instance.MapManager.CanGo(movePos))
        {
            CellPos = Managers.Instance.MapManager.CovnertCellPos(transform.position);
            transform.position = new Vector3(movePos.x, movePos.y, movePos.y);
        }
    }

    public override void UpdateUsername()
    {
        if (Username != null && _playType == Define.PlayerType.SwordMan)
        {
            Username.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.85f, 0));
        }

        else if (Username != null && _playType == Define.PlayerType.Gunner)
        {
            Username.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.2f, 0));
        }
    }

    public override void UpdateJump()
    {
        _shadow.transform.position = new Vector3(_shadow.transform.position.x, _shadowPos.y);

        transform.position += Vector3.up * _jumpPower * Time.deltaTime;
        _jumpPower -= _gravity * Time.deltaTime * 2;
        Vector3 movePos = transform.position;

        switch (Dir)
        {
            case Define.MoveDir.Right:
                movePos += Vector3.right * Time.deltaTime * _speed;
                break;

            case Define.MoveDir.Left:
                movePos += Vector3.left * Time.deltaTime * _speed;
                break;
        }

        if (Managers.Instance.MapManager.CanGo(new Vector3(movePos.x, JumpStartPos.y, JumpStartPos.z))) 
        {
            transform.position = new Vector3(movePos.x, movePos.y, -9);
            CellPos = Managers.Instance.MapManager.CovnertCellPos(transform.position);
        }

        if (transform.position.y <= JumpStartPos.y)
        {
            transform.position = new Vector3(transform.position.x, JumpStartPos.y, JumpStartPos.y);
            _shadow.transform.position = new Vector3(_shadow.transform.position.x, _shadowPos.y);
            SM.SetJumpPower(0);
            _jumpPower = 4.0f;
        }
    }

    public override void UpdateAttack1()
    {
        switch (_playType)
        {
            case Define.PlayerType.SwordMan:
                if (Input.GetKeyDown(KeyCode.X))
                    _continueAttack = true;

                if (_coAttack == false)
                    StartCoroutine(SwordManCoAttack1());
                break;

            case Define.PlayerType.Gunner:
                if (Input.GetKeyDown(KeyCode.X))
                    _continueAttack = true;

                if (_coAttack == false)
                    StartCoroutine(GunnerAttack1());
                break;
        }
    }

    internal void PlayerSync(Vector3 ts, Define.CreatureState sM, Define.MoveDir dIR, bool left, Vector3Int cell, Vector3 jumpStartPos, bool attackFlag, Vector3 shadowPos)
    {
        Dir = dIR;
        Left = left;
        CellPos = cell;
        JumpStartPos = jumpStartPos;
        transform.position = ts;
        _attackFlag = attackFlag;
        _shadow.transform.position = shadowPos;
        UpdateSM(sM);
    }

    public override void UpdateAttack2()
    {
        switch (_playType)
        {
            case Define.PlayerType.SwordMan:
                if (Input.GetKeyDown(KeyCode.X))
                    _continueAttack = true;

                if (_coAttack == false)
                    StartCoroutine(SwordManCoAttack2());
                break;

            case Define.PlayerType.Gunner:
                if (Input.GetKeyDown(KeyCode.X))
                    _continueAttack = true;

                if (_coAttack == false)
                    StartCoroutine(GunnerAttack2());
                break;
        }
    }
    public override void UpdateAttack3()
    {
        switch (_playType)
        {
            case Define.PlayerType.SwordMan:
                if (_coAttack == false)
                    StartCoroutine(SwordManCoAttack3());
                break;

            case Define.PlayerType.Gunner:
                break;
        }
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

    public override void UpdateDetect()
    {

    }

    public override void UpdateDie()
    {
        if (_coDied == false)
        {
            StartCoroutine(CoDied());
        }
    }

    public override void UpdateSkill()
    {
        switch (_playType)
        {
            case Define.PlayerType.SwordMan:
                if (_coSkill == false)
                    StartCoroutine(SwordManSkill());
                break;

            case Define.PlayerType.Gunner:
                if (_coSkill == false)
                    StartCoroutine(GunnerCoSkill());
                break;
        }
    }

    IEnumerator SwordManSkill()
    {
        _coSkill = true;
        yield return new WaitForSeconds(0.2f);
        SwordSkillAttack();
        yield return new WaitForSeconds(0.2f);
        SM.SetSkill(0);
        _coSkill = false;
    }

    private void SwordSkillAttack()
    {
        GameObject effect = Managers.Instance.ResourceManager.Instantiate("Game/Object/SwordManEffect");

        if (Left == true)
        {
            effect.transform.position = new Vector3(gameObject.transform.position.x - 3f, gameObject.transform.position.y, gameObject.transform.position.y);
            effect.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            effect.transform.position = new Vector3(gameObject.transform.position.x + 3f, gameObject.transform.position.y, gameObject.transform.position.y);
            effect.GetComponent<SpriteRenderer>().flipX = true;
        }

        EffectController ef = effect.AddComponent<EffectController>();
        ef.Init(0.7f);
    }

    public override void Attacked(int damage)
    {
        _hp -= damage;
        SM.SetDamaged(damage);

        Managers.Instance.DataManager.GameMap.PlayerHpcDict.TryGetValue(PlayerId, out var hpc);

        if (hpc != null)
            hpc.Attack(damage);

        if (_hp <= 0)
        {
            _hp = 0;
            SM.SetDie(true);
        }
    }

    private void UpdateSwordManAnimation()
    {
        switch (SM.State)
        {
            case Define.CreatureState.None:
            case Define.CreatureState.Idel:
                CAnimator.Play("SwordIdle");
                break;

            case Define.CreatureState.Move:
                CAnimator.Play("SwordRun");
                break;

            case Define.CreatureState.Jump:
                CAnimator.Play("SwordJump");
                break;

            case Define.CreatureState.Attack1:
                CAnimator.Play("SwordAttack1");
                break;

            case Define.CreatureState.Attack2:
                CAnimator.Play("SwordAttack2");
                break;

            case Define.CreatureState.Attack3:
                CAnimator.Play("SwordAttack3");
                break;

            case Define.CreatureState.Attacked:
                CAnimator.Play("SwordAttacked");
                break;

            case Define.CreatureState.Die:
                CAnimator.Play("SwordDie");
                break;

            case Define.CreatureState.Skill:
                CAnimator.Play("SwordManSkill");
                break;
        }
    }

    private void UpdateGunnerAnimation()
    {
        switch (SM.State)
        {
            case Define.CreatureState.None:
            case Define.CreatureState.Idel:
                CAnimator.Play("GunnerIdle");
                break;

            case Define.CreatureState.Move:
                CAnimator.Play("GunnerRun");
                break;

            case Define.CreatureState.Jump:
                CAnimator.Play("GunnerJump");
                break;

            case Define.CreatureState.Attack1:
                CAnimator.Play("GunnerAttack1");
                break;

            case Define.CreatureState.Attack2:
                CAnimator.Play("GunnerAttack2");
                break;

            case Define.CreatureState.Attacked:
                CAnimator.Play("GunnerAttaced");
                break;

            case Define.CreatureState.Die:
                CAnimator.Play("GunnerDie");
                break;

            case Define.CreatureState.Skill:
                CAnimator.Play("GunnerSkill");
                break;
        }
    }

    IEnumerator SwordManCoAttack1()
    {
        _continueAttack = false;
        _coAttack = true;
        yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.4f);
        if (_continueAttack)
        {
            _continueAttack = false;
            SM.SetContinueAttack(true);
        }
        else
        {
            SM.SetDamage(0);
        }
        _coAttack = false;
    }

    IEnumerator SwordManCoAttack2()
    {
        _continueAttack = false;
        _coAttack = true;
        yield return new WaitForSeconds(0.4f);

        yield return new WaitForSeconds(0.6f);
        if (_continueAttack)
        {
            _continueAttack = false;
            SM.SetContinueAttack(true);
        }
        else
        {
            SM.SetDamage(0);
        }
        _coAttack = false;
    }


    IEnumerator SwordManCoAttack3()
    {
        _continueAttack = false;
        _coAttack = true;
        yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.6f);
        SM.SetDamage(0);
        _coAttack = false;
    }

    IEnumerator GunnerAttack1()
    {
        _continueAttack = false;
        _coAttack = true;
        yield return new WaitForSeconds(0.2f);
        Shoot();
        yield return new WaitForSeconds(0.4f);
        if (_continueAttack)
        {
            _continueAttack = false;
            SM.SetContinueAttack(true);
        }
        else
        {
            SM.SetDamage(0);
        }
        _coAttack = false;
    }

    IEnumerator GunnerAttack2()
    {
        _continueAttack = false;
        _coAttack = true;
        yield return new WaitForSeconds(0.4f);
        Shoot();
        yield return new WaitForSeconds(0.4f);
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
        Managers.Instance.DataManager.GameMap.PlayerDict.Remove(PlayerId);
        Destroy(Username);
        Managers.Instance.DataManager.GameMap.CameraSwap(PlayerId);
        Destroy(gameObject);
    }

    IEnumerator GunnerCoSkill()
    {
        _coSkill = true;
        yield return new WaitForSeconds(0.4f);
        yield return new WaitForSeconds(0.8f);
        SM.SetSkill(0);
        _coSkill = false;
    }


    private void UpdateSM(Define.CreatureState sm)
    {
        if (SM.State != Define.CreatureState.Jump && sm == Define.CreatureState.Jump)
        {
            _shadowPos = _shadow.transform.position;
            JumpStartPos = transform.position;
            SM.SetJumpPower(4.0f);
        }
        if (sm == Define.CreatureState.Skill)
        {
            SM.SetSkill(_attack);
        }
        if (_attackFlag == true)
        {
            _continueAttack = true;
            SM.SetDamage(_attack);
            _attackFlag = false;
        }
        if (Dir == Define.MoveDir.Up)
        {
            SM.SetSpeed(_speed);
        }
        else if (Dir == Define.MoveDir.Right)
        {
            SM.SetSpeed(_speed);

            if (SM.State == Define.CreatureState.Idel || SM.State == Define.CreatureState.Move || SM.State == Define.CreatureState.Jump)
                Left = false;
        }
        else if (Dir == Define.MoveDir.Left)
        {
            SM.SetSpeed(_speed);

            if (SM.State == Define.CreatureState.Idel || SM.State == Define.CreatureState.Move || SM.State == Define.CreatureState.Jump)
                Left = true;
        }
        else if (Dir == Define.MoveDir.Down)
        {
            SM.SetSpeed(_speed);
        }
        else if (Dir == Define.MoveDir.None)
        {
            SM.SetSpeed(0);
        }
    }

    public void Shoot()
    {
        GameObject bullet = Managers.Instance.ResourceManager.Instantiate("Game/Object/Bullet");
        bullet.GetComponent<BulletController>().Init(_net, Define.CreatureType.OtherPlayer, _attack, Left, CellPos);
    }
}
