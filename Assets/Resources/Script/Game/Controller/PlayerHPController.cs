using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPController : HPController
{
    private TMP_Text _username;
    private GameObject _userPick;
    private Sprite _swordManPick;
    private Sprite _gunnerPick;
    public Define.PlayerType playerType;
    public string playerUsername;
    void Awake()
    {
        _swordManPick = Resources.Load<Sprite>("UI/Img/SwordManPick");
        _gunnerPick = Resources.Load<Sprite>("UI/Img/GunnerPick");
        _userPick = transform.GetChild(0).gameObject;
        _username = Utils.FindChild<TMP_Text>(gameObject, "UserNameText", true);
        _hpBar = transform.GetChild(3).gameObject;
    }
    public void Init(string username, Define.PlayerType userPick, bool atcive)
    {
        playerUsername = username;
        _username.text = username;
        HP_MAX = userPick == Define.PlayerType.SwordMan ? 500 : 300;
        HP = userPick == Define.PlayerType.SwordMan ? 500 : 300;
        _userPick.GetComponent<Image>().sprite = userPick == Define.PlayerType.SwordMan ? _swordManPick : _gunnerPick;
        gameObject.SetActive(atcive);
        playerType = userPick;
    }

    public void Init(string username, Define.PlayerType userPick, bool atcive, int hp, int hpMax)
    {
        playerUsername = username;
        _username.text = username;
        HP_MAX = hpMax;
        HP = hp;
        _userPick.GetComponent<Image>().sprite = userPick == Define.PlayerType.SwordMan ? _swordManPick : _gunnerPick;
        gameObject.SetActive(atcive);
        playerType = userPick;
        Attack(0);
    }

    public void Attack(int damage)
    {
        HP -= damage;
        Image hpImage = _hpBar.GetComponent<Image>();
        hpImage.fillAmount = (float)((float)HP / (float)HP_MAX);
    }

    internal void HpUP(int hp)
    {
        int nextHp = HP + hp;
        if (HP_MAX < nextHp)
        {
            HP = HP_MAX;
        }
        else
        {
            HP = nextHp;
        }

        Image hpImage = _hpBar.GetComponent<Image>();
        hpImage.fillAmount = (float)((float)HP / (float)HP_MAX);
    }
}
