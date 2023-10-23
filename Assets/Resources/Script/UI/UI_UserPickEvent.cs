using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_UserPickEvent : MonoBehaviour   
    , IPointerClickHandler
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{

    public int pick;
    private Network _net;
    void Start()
    {
        _net = GameObject.Find("@Network").GetComponent<Network>(); 
    }

    void Update()
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);

        int pktSize = 4 + 4;

        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write((Int16)Define.PacketProtocol.C_T_S_USERPICK);
        bw.Write((Int16)pktSize);
        bw.Write(pick);
        _net.SendPacket(bytes, pktSize);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {
    
    }
}
