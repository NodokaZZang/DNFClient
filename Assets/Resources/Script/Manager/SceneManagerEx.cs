using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void LoadScene(Define.SceneType sceneType)
    {
        CurrentScene.Clear();
        SceneManager.LoadScene(GetSceneName(sceneType));
    }

    string GetSceneName(Define.SceneType sceneType)
    {
        string name = System.Enum.GetName(typeof(Define.SceneType), sceneType);
        return name;
    }
}
