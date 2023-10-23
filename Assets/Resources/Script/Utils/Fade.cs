using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    private Image image;
    private bool _coFlag = false;
    private Define.SceneType _type;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (_coFlag)
            StartCoroutine(FadeOutCoroutineNoScene());
    }

    public void FadeOut(Define.SceneType type) 
    {
        _type = type;
        _coFlag = true;
    }

    IEnumerator FadeOutCoroutineNoScene()
    {
        _coFlag = false;

        float fadeCount = 0;
        while (fadeCount < 1.0f)
        {
            fadeCount += 0.02f;
            yield return new WaitForSeconds(0.01f);
            image.color = new Color(0, 0, 0, fadeCount);
        }

        Managers.Instance.SceneManagerEx.LoadScene(_type);
    }
}
