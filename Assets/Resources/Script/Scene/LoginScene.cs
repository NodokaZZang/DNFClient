using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    GameObject LoginUI;
    GameObject EventSystem;
    public override void Init() 
    {
        base.Init();
        _type = Define.SceneType.LoginScene;
        EventSystem = Managers.Instance.ResourceManager.Instantiate("UI/EventSystem");
        LoginUI = Managers.Instance.ResourceManager.Instantiate("UI/LoginUI");
        FullScreen();
    }

    public override void Clear()
    {
        base.Clear();
        Managers.Instance.ResourceManager.Destory(LoginUI);
        Managers.Instance.ResourceManager.Destory(EventSystem);
    }
}
