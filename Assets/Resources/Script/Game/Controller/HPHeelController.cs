using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HPHeelController : MonoBehaviour
{
    private Network _net;

    void Start()
    {
        _net = GameObject.FindWithTag("net").GetComponent<Network>();
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            bool heelFlag = false;

            Vector3Int nowPos = Managers.Instance.MapManager.CovnertCellPos(transform.position);

            if (Managers.Instance.DataManager.GameMap.IsAdmin == false) return;

            foreach (var playerGo in Managers.Instance.DataManager.GameMap.PlayerDict.Values)
            {
                CreatureController cc = playerGo.GetComponent<CreatureController>();

                Vector3Int playerCellPos = cc.CellPos;

                if (nowPos.y != playerCellPos.y) continue;

                if (playerCellPos.x <= nowPos.x + 1 && playerCellPos.x >= nowPos.x - 1)
                {
                    // 몬스터 피격
                    byte[] bytes = new byte[1024];
                    MemoryStream ms = new MemoryStream(bytes);
                    BinaryWriter bw = new BinaryWriter(ms);

                    Int16 pktHeader = (Int16)(4);

                    bw.Write((Int16)Define.PacketProtocol.UDP_HPHEEL);
                    bw.Write((Int16)pktHeader);
                    _net.UDPBrodCast(bytes, pktHeader);

                    heelFlag = true;
                    break;
                }
            }

            if (heelFlag)
            {
                foreach (var playerGo in Managers.Instance.DataManager.GameMap.PlayerDict.Values)
                {
                    CreatureController cc = playerGo.GetComponent<CreatureController>();

                    if (cc.SM.State == Define.CreatureState.Die)
                        continue;

                    cc.HpUP(250);
                }
                Destroy(gameObject);
            }
        }
        catch (Exception e) 
        {
            Debug.LogException(e);  
        }
    }
}
