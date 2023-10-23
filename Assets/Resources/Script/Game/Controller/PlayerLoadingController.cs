using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLoadingController : MonoBehaviour
{
    public int TASK { get; set; } = 0;
    public int TASK_MAX { get; set; } = 1;
    protected GameObject _taskBar;
    private TMP_Text _username;
    private GameObject _userPick;
    private Sprite _swordManPick;
    private Sprite _gunnerPick;


    void Awake()
    {
        _swordManPick = Resources.Load<Sprite>("UI/Img/SwordManPick");
        _gunnerPick = Resources.Load<Sprite>("UI/Img/GunnerPick");
        _userPick = transform.GetChild(0).gameObject;
        _username = Utils.FindChild<TMP_Text>(gameObject, "UserNameText", true);
        _taskBar = transform.GetChild(2).gameObject;
    }

    public void Init(string username, Define.PlayerType userPick, bool atcive, int userCnt)
    {
        _username.text = username;
        TASK_MAX = userCnt;
        TASK = 0;
        _userPick.GetComponent<Image>().sprite = userPick == Define.PlayerType.SwordMan ? _swordManPick : _gunnerPick;
        gameObject.SetActive(atcive);

        if (TASK_MAX == 0)
            TASK_MAX = 1;
    }

    public void TaskComplete() 
    {
        int nextTaskGage = TASK + 1;
        
        if (TASK_MAX < nextTaskGage)
        {
            TASK = TASK_MAX;
        }
        else
        {
            TASK = nextTaskGage;
        }

        Image taskImage = _taskBar.GetComponent<Image>();
        taskImage.fillAmount = (float)((float)TASK / (float)TASK_MAX);
    }

    public bool ISTaskComplete() 
    {
        return TASK == TASK_MAX;
    }
}
