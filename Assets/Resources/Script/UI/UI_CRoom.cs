using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class UI_CRoom : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void CRoomOpen() 
    {
        gameObject.SetActive(true);
    }

    public void CRoomClose() 
    {
        gameObject.SetActive(false);
    }

    public void CreateRoom() 
    {
        TMP_Text text = Utils.FindChild<TMP_Text>(gameObject, "CRoomText", true);
        string title = text.text.Replace("\u200B", string.Empty);

        if (title.Length <= 0)
            return;

        byte[] titleBytes = Encoding.Unicode.GetBytes(title.Trim());

        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);

        int pktSize = 4 + 4 +  titleBytes.Length;

        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write((Int16)Define.PacketProtocol.C_T_S_CREATEROOM);
        bw.Write((Int16)pktSize);
        bw.Write(titleBytes.Length);
        bw.Write(titleBytes);

        Network net = GameObject.Find("@Network").GetComponent<Network>();
        net.SendPacket(bytes, pktSize);
    }
}
