using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatingScene : BaseScene
{
    private int sq;
    GameObject LobbyPlay;
    GameObject EventSystem;
    GameObject WatingUI;
    public void SetRoomSQ(int _sq) { sq = _sq; }
    public int GetRoomSQ() { return sq; }   

    public override void Init()
    {
        base.Init();
        _type = Define.SceneType.WatingScene;
        LobbyPlay = Managers.Instance.ResourceManager.Instantiate("UI/LobbyPlay");
        EventSystem = Managers.Instance.ResourceManager.Instantiate("UI/EventSystem");
        WatingUI = Managers.Instance.ResourceManager.Instantiate("UI/WatingUI");
        FullScreen();
    }
    public override void Clear()
    {
        base.Clear();
        Managers.Instance.ResourceManager.Destory(LobbyPlay);
        Managers.Instance.ResourceManager.Destory(EventSystem);
        Managers.Instance.ResourceManager.Destory(WatingUI);
    }
}
