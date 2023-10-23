using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScene : MonoBehaviour
{
    protected Define.SceneType _type = Define.SceneType.BaseScene;

    void Awake()
    {
        Init();
    }

    public virtual void Init() 
    {
        GameObject net = GameObject.Find("@Network");
        GameObject logger = GameObject.Find("@Logger");

        if (net == null) 
        {
            net = new GameObject("@Network");
            net.tag = "net";
            net.AddComponent<Network>();
            DontDestroyOnLoad(net);
        }
    }

    public virtual void Clear() { }

    public void FullScreen() 
    {
        int setWidth = 1920;
        int setHeight = 1080;

        Screen.SetResolution(setWidth, setHeight, true);
    }

    public void GameScreen() 
    {
        int setWidth = 800;
        int setHeight = 600;

        Screen.SetResolution(setWidth, setHeight, false);
    }
}
