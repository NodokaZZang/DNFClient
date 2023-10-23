using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RoomItemEvent : MonoBehaviour   
    , IPointerClickHandler
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{

    private Image _img;
    private Color _originalColor;
    private Network _net;
    void Start()
    {
        _img = gameObject.GetComponent<Image>();
        _originalColor = _img.color;
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
        GameObject go = Utils.FindChild(gameObject, "RoomSQ");
        TMP_Text sq = go.GetComponent<TMP_Text>();

        int roomSQ = int.Parse(sq.text);

        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);

        int pktSize = 4 + 4;

        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write((Int16)Define.PacketProtocol.C_T_S_ENTEROOM);
        bw.Write((Int16)pktSize);
        bw.Write(roomSQ);

        _net.SendPacket(bytes, pktSize);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _img.color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _img.color = _originalColor;
    }
}
