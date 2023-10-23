using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CreatureController
{
    public Define.PlayerType _playType = Define.PlayerType.SwordMan;

    private float _speed = 3.0f;
    private int _attack = 15;
    private int _gunnerSkillDamage = 15;
    private int _swordManSkillDamage = 30;
    private float _jumpPower = 4.0f;
    private Network _net;
    private bool _coAttack = false;
    private bool _continueAttack = false;
    private GameObject _shadow;
    private Vector3 _shadowPos;
    private int _swordManAttackRange = 3;
    private bool _coAttacked = false;
    private bool _coDied = false;
    private bool _coSkill = false;
    private bool _skillCollTime = false;

    public AudioSource _walkSound;
    public AudioSource _swordSound;
    public AudioSource _swordSkill;

    public AudioSource _bulletSound;
    public AudioSource _gunnerSkill;
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
                AudioSource [] soundEffect = GetComponents<AudioSource>();
                _swordSound = soundEffect[0];
                _walkSound = soundEffect[1];
                _swordSkill = soundEffect[2];
                break;

            case Define.PlayerType.Gunner:
                _hpMax = 300;
                _hp = 300;
                _attack = 5;
                _speed = 3.5f;
                AudioSource[] soundEffect2 = GetComponents<AudioSource>();
                _walkSound = soundEffect2[0];
                _bulletSound = soundEffect2[1];
                _gunnerSkill= soundEffect2[2];
                break;
        }
    }

    public override void UpdateInupt()
    {
        if (SM.State == Define.CreatureState.Die)
            return;

        //if (SM.State != Define.CreatureState.Jump && Input.GetKeyDown(KeyCode.C))
        //{
        //    _shadowPos = _shadow.transform.position;
        //    JumpStartPos = transform.position;
        //    SM.SetJumpPower(4.0f);
        //}
        if (SM.State != Define.CreatureState.Jump && Input.GetKeyDown(KeyCode.X))
        {
            _attackFlag = true;
            _continueAttack = true;
            SM.SetDamage(_attack);
        }
        if (_skillCollTime == false && SM.State != Define.CreatureState.Jump && Input.GetKeyDown(KeyCode.Z))
        {
            _skillCollTime = true;
            SM.SetSkill(_attack);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Dir = Define.MoveDir.Up;
            SM.SetSpeed(_speed);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Dir = Define.MoveDir.Right;
            SM.SetSpeed(_speed);

            if (SM.State == Define.CreatureState.Idel || SM.State == Define.CreatureState.Move || SM.State == Define.CreatureState.Jump)
                Left = false;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            Dir = Define.MoveDir.Left;
            SM.SetSpeed(_speed);

            if (SM.State == Define.CreatureState.Idel || SM.State == Define.CreatureState.Move || SM.State == Define.CreatureState.Jump)
                Left = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Dir = Define.MoveDir.Down;
            SM.SetSpeed(_speed);
        }
        else
        {
            Dir = Define.MoveDir.None;
            SM.SetSpeed(0);
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
            Debug.Log(CellPos);
            transform.position = new Vector3(movePos.x, movePos.y, movePos.y);
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

    public override void UpdateDie()
    {
        if (_coDied == false)
        {
            StartCoroutine(CoDied());
        }
    }

    public override void UpdateLeft()
    {
        base.UpdateLeft();
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
        _swordSkill.Play();
        _coSkill = true;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(CoSkillCool());
        SwordSkillAttack();
        yield return new WaitForSeconds(0.2f);
        SM.SetSkill(0);
        _coSkill = false;
    }

    public override void UpdateDetect()
    {
        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);
        BinaryWriter bw = new BinaryWriter(ms);
        Int16 pktHeader = 66;

        bw.Write((Int16)Define.PacketProtocol.UDP_PLAYERSYNC);
        bw.Write((Int16)pktHeader);
        // PlayerID
        bw.Write((Int32)Managers.Instance.DataManager.GameMap.PlayerId);
        // trasform.x y z
        bw.Write((float)transform.position.x);
        bw.Write((float)transform.position.y);
        bw.Write((float)transform.position.z);
        // CreatureState
        bw.Write((Int32)SM.State);
        // MoveDir
        bw.Write((Int32)Dir);
        // Left
        bw.Write((bool)Left);
        // CellPos x y z
        bw.Write((Int32)CellPos.x);
        bw.Write((Int32)CellPos.y);
        bw.Write((Int32)CellPos.z);
        // JumpStartPos 
        bw.Write((float)JumpStartPos.x);
        bw.Write((float)JumpStartPos.y);
        bw.Write((float)JumpStartPos.z);
        // attackFlag
        bw.Write((bool)_attackFlag);
        // shadowPos
        bw.Write((float)_shadow.transform.position.x);
        bw.Write((float)_shadow.transform.position.y);
        bw.Write((float)_shadow.transform.position.z);

        _net.UDPBrodCast(bytes, pktHeader);
    }

    private void UpdateSwordManAnimation()
    {
        switch (SM.State)
        {
            case Define.CreatureState.None:
            case Define.CreatureState.Idel:
                CAnimator.Play("SwordIdle");
                _walkSound.Stop();
                _swordSound.Stop();
                break;

            case Define.CreatureState.Move:
                CAnimator.Play("SwordRun");
                if (!_walkSound.isPlaying)
                    _walkSound.Play();
                break;

            case Define.CreatureState.Jump:
                CAnimator.Play("SwordJump");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attack1:
                CAnimator.Play("SwordAttack1");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attack2:
                CAnimator.Play("SwordAttack2");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attack3:
                CAnimator.Play("SwordAttack3");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attacked:
                CAnimator.Play("SwordAttacked");
                _walkSound.Stop();
                _swordSound.Stop();
                break;

            case Define.CreatureState.Die:
                CAnimator.Play("SwordDie");
                _swordSound.Stop();
                _walkSound.Stop();
                break;

            case Define.CreatureState.Skill:
                CAnimator.Play("SwordManSkill");
                _swordSound.Stop();
                _walkSound.Stop();
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
                _walkSound.Stop();
                break;

            case Define.CreatureState.Move:
                CAnimator.Play("GunnerRun");
                if (!_walkSound.isPlaying)
                    _walkSound.Play();
                break;

            case Define.CreatureState.Jump:
                CAnimator.Play("GunnerJump");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attack1:
                CAnimator.Play("GunnerAttack1");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attack2:
                CAnimator.Play("GunnerAttack2");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Attacked:
                CAnimator.Play("GunnerAttaced");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Die:
                CAnimator.Play("GunnerDie");
                _walkSound.Stop();
                break;

            case Define.CreatureState.Skill:
                CAnimator.Play("GunnerSkill");
                _walkSound.Stop();
                break;
        }
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

    IEnumerator SwordManCoAttack1()
    {
        _continueAttack = false;
        _coAttack = true;
        _swordSound.Play();
        yield return new WaitForSeconds(0.3f);
        Attack();
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
        _swordSound.Play();
        yield return new WaitForSeconds(0.4f);
        Attack();
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
        _swordSound.Play();
        yield return new WaitForSeconds(0.3f);
        Attack();
        yield return new WaitForSeconds(0.6f);
        SM.SetDamage(0);
        _coAttack = false;
    }

    IEnumerator GunnerAttack1()
    {
        _continueAttack = false;
        _coAttack = true;
        yield return new WaitForSeconds(0.2f);
        _bulletSound.Play();
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
        _bulletSound.Play();
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
        StartCoroutine(CoSkillCool());
        _gunnerSkill.Play();
        yield return new WaitForSeconds(0.4f);
        GunnerSkillAttack();
        yield return new WaitForSeconds(0.8f);
        GunnerSkillAttack();
        SM.SetSkill(0);
        _coSkill = false;
    }

    public override void Attack()
    {
        try
        {
            Vector3Int nowPos = CellPos;
            Vector3Int range = Vector3Int.zero;
            List<MonsterController> attackedMonster = new List<MonsterController>();

            if (Left == false)
                range = new Vector3Int(CellPos.x + _swordManAttackRange, CellPos.y);
            else
                range = new Vector3Int(CellPos.x - _swordManAttackRange, CellPos.y);


            foreach (var monsterPair in Managers.Instance.DataManager.GameMap.MonsterDict)
            {
                //int monsterId = monsterPair.Key;
                MonsterController mc = monsterPair.Value.GetComponent<MonsterController>();

                if (mc.CellPos.y != nowPos.y)
                    continue;

                if (Left && mc.CellPos.x <= nowPos.x && mc.CellPos.x >= range.x)
                    attackedMonster.Add(mc);

                if (Left == false && mc.CellPos.x >= nowPos.x && mc.CellPos.x <= range.x)
                    attackedMonster.Add(mc);
            }

            // 피격당한 몬스터들이 들어 있음 ...
            byte[] bytes = new byte[1024];
            MemoryStream ms = new MemoryStream(bytes);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)((attackedMonster.Count * 8) + 4 + 2);

            bw.Write((Int16)Define.PacketProtocol.UDP_MONSTERATTACKED);
            bw.Write((Int16)pktHeader);
            bw.Write((Int16)attackedMonster.Count);

            foreach (MonsterController mc in attackedMonster)
            {
                mc.Attacked(_attack);
                // MonsterID
                bw.Write((Int32)mc.MonsterID);
                // Damage 
                bw.Write((Int32)_attack);
            }
            _net.UDPBrodCast(bytes, pktHeader);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            SM.SetDamage(0);
            _coAttack = false;
        }
    }

    public void GunnerSkillAttack()
    {
        try
        {
            Vector3Int nowPos = CellPos;

            List<MonsterController> attackedMonster = new List<MonsterController>();

            foreach (var monsterPair in Managers.Instance.DataManager.GameMap.MonsterDict)
            {
                //int monsterId = monsterPair.Key;
                MonsterController mc = monsterPair.Value.GetComponent<MonsterController>();

                if (mc.CellPos.y != nowPos.y)
                    continue;

                if (mc.CellPos.x <= nowPos.x && mc.CellPos.x >= nowPos.x - 2)
                    attackedMonster.Add(mc);

                if (mc.CellPos.x >= nowPos.x && mc.CellPos.x <= nowPos.x + 2)
                    attackedMonster.Add(mc);
            }

            // 피격당한 몬스터들이 들어 있음 ...
            byte[] bytes = new byte[1024];
            MemoryStream ms = new MemoryStream(bytes);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)((attackedMonster.Count * 8) + 4 + 2);

            bw.Write((Int16)Define.PacketProtocol.UDP_MONSTERATTACKED);
            bw.Write((Int16)pktHeader);
            bw.Write((Int16)attackedMonster.Count);

            foreach (MonsterController mc in attackedMonster)
            {
                mc.Attacked(_gunnerSkillDamage);
                // MonsterID
                bw.Write((Int32)mc.MonsterID);
                // Damage 
                bw.Write((Int32)_gunnerSkillDamage);
            }
            _net.UDPBrodCast(bytes, pktHeader);
        }
        catch (Exception e)
        {
            _coSkill = false;
            SM.SetSkill(0);
            Debug.LogError(e);
        }
    }

    public void SwordSkillAttack()
    {
        try
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

            Vector3Int nowPos = CellPos;

            List<MonsterController> attackedMonster = new List<MonsterController>();

            foreach (var monsterPair in Managers.Instance.DataManager.GameMap.MonsterDict)
            {
                //int monsterId = monsterPair.Key;
                MonsterController mc = monsterPair.Value.GetComponent<MonsterController>();

                if (mc.CellPos.y != nowPos.y)
                    continue;

                if (Left == true && mc.CellPos.x <= nowPos.x && mc.CellPos.x >= nowPos.x - 5)
                {
                    attackedMonster.Add(mc);
                }
                else if (Left == false && mc.CellPos.x >= nowPos.x && mc.CellPos.x <= nowPos.x + 5)
                {
                    attackedMonster.Add(mc);
                }
            }

            // 피격당한 몬스터들이 들어 있음 ...
            byte[] bytes = new byte[1024];
            MemoryStream ms = new MemoryStream(bytes);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)((attackedMonster.Count * 8) + 4 + 2);

            bw.Write((Int16)Define.PacketProtocol.UDP_MONSTERATTACKED);
            bw.Write((Int16)pktHeader);
            bw.Write((Int16)attackedMonster.Count);

            foreach (MonsterController mc in attackedMonster)
            {
                mc.Attacked(_swordManSkillDamage);
                // MonsterID
                bw.Write((Int32)mc.MonsterID);
                // Damage 
                bw.Write((Int32)_swordManSkillDamage);
            }
            _net.UDPBrodCast(bytes, pktHeader);
        }
        catch (Exception e)
        {
            _coSkill = false;
            SM.SetSkill(0);
            Debug.LogError(e);
        }
    }


    public void Shoot()
    {
        GameObject bullet = Managers.Instance.ResourceManager.Instantiate("Game/Object/Bullet");
        bullet.GetComponent<BulletController>().Init(_net, Define.CreatureType.Player, _attack, Left, CellPos);
    }

    IEnumerator CoSkillCool()
    {
        Color original = Managers.Instance.DataManager.GameMap._SkillIcon.GetComponent<Image>().color;
        Color originalTextColor = Managers.Instance.DataManager.GameMap._SkillIcon.transform.GetChild(0).GetComponent<TMP_Text>().color;
        Color skillCoolTimeColor = original;
        Color skillCollTimeTextColor = originalTextColor;
        skillCoolTimeColor.a = 0.3f;
        skillCollTimeTextColor.a = 0.3f;
        Managers.Instance.DataManager.GameMap._SkillIcon.GetComponent<Image>().color = skillCoolTimeColor;
        Managers.Instance.DataManager.GameMap._SkillIcon.transform.GetChild(0).GetComponent<TMP_Text>().color = skillCollTimeTextColor;
        yield return new WaitForSeconds(5.0f);
        _skillCollTime = false;
        Managers.Instance.DataManager.GameMap._SkillIcon.GetComponent<Image>().color = original;
        Managers.Instance.DataManager.GameMap._SkillIcon.transform.GetChild(0).GetComponent<TMP_Text>().color = originalTextColor;
    }
}
