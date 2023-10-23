using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene1 : BaseScene
{
    GameObject EventSystem;
    public override void Init()
    {
        base.Init();
        _type = Define.SceneType.GameScene1;
        EventSystem = Managers.Instance.ResourceManager.Instantiate("UI/EventSystem");
        Managers.Instance.MapManager.LoadMap(1);
        GameScreen();
    }

    public override void Clear()
    {
        base.Clear();
        Managers.Instance.MapManager.DestroyMap();
        Managers.Instance.ResourceManager.Destory(EventSystem);
    }
}
