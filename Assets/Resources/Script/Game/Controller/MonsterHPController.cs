using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHPController : HPController
{
    private Vector3 originalV3;

    void Start()
    {

    }

    public void Init(int Hp)
    {
        originalV3 = transform.localScale;
        HP = Hp;
        HP_MAX = Hp;
        float xScale = ((float)HP / (float)HP_MAX) / 10;
        transform.localScale = new Vector3(xScale, originalV3.y, originalV3.z);
    }

    public void Attack(int damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
        }

        float xScale = ((float)HP / (float)HP_MAX) / 10;

        transform.localScale = new Vector3(xScale, originalV3.y, originalV3.z);
    }
}
