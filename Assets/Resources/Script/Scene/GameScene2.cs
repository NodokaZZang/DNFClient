using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene2 : BaseScene
{
    GameObject EventSystem;
    public override void Init()
    {
        base.Init();
        _type = Define.SceneType.GameScene2;
        EventSystem = Managers.Instance.ResourceManager.Instantiate("UI/EventSystem");
        Managers.Instance.MapManager.LoadMap(2);
        GameScreen();
    }

    public override void Clear()
    {
        base.Clear();
        Managers.Instance.MapManager.DestroyMap();
        Managers.Instance.ResourceManager.Destory(EventSystem);
    }
}
