using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance { get { Init(); return _instance; } }
    public SceneManagerEx SceneManagerEx { get { return _sceneManager; } }
    public ResourceManager ResourceManager { get { return _resourceManager; } }
    public DataManager DataManager { get { return _dataManager; } }
    public MapManager MapManager { get { return _mapManager; } }

    MapManager _mapManager = new MapManager();
    DataManager _dataManager = new DataManager();
    SceneManagerEx _sceneManager = new SceneManagerEx();
    ResourceManager _resourceManager = new ResourceManager();

    private static void Init() 
    {
        GameObject go = GameObject.Find("@Managers");

        if (go == null) 
        {
            go = new GameObject("@Managers");
            go.AddComponent<Managers>();    
        }

        DontDestroyOnLoad(go);
        _instance = go.GetComponent<Managers>();
    }
}
