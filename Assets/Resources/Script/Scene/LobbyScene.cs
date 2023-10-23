using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    GameObject LobbyPlay;
    GameObject EventSystem;
    GameObject LobbyUI;
    public override void Init()
    {
        base.Init();
        _type = Define.SceneType.LobbyScene;
        LobbyPlay = Managers.Instance.ResourceManager.Instantiate("UI/LobbyPlay");
        LobbyUI = Managers.Instance.ResourceManager.Instantiate("UI/LobbyUI");
        EventSystem = Managers.Instance.ResourceManager.Instantiate("UI/EventSystem");
        FullScreen();
    }

    public override void Clear()
    {
        base.Clear();
        Managers.Instance.ResourceManager.Destory(LobbyUI);
        Managers.Instance.ResourceManager.Destory(LobbyPlay);
        Managers.Instance.ResourceManager.Destory(EventSystem);
    }
}
