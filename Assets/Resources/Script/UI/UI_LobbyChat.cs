using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class UI_LobbyChat : MonoBehaviour
{
    private List<string> chat = new List<string>();
    public int msgMaxSize = 20;
    private GameObject _contents = null;
    private Network _net;
    void Start()
    {
        _contents  = Utils.FindChild(gameObject, "Content", true);
        _net = GameObject.FindWithTag("net").GetComponent<Network>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            SendChat();
        }

        for (int i = 0; i < _contents.transform.childCount; i++)
            Destroy(_contents.transform.GetChild(i).gameObject);

        foreach (string msg in chat) 
        {  
            GameObject msgGo = Managers.Instance.ResourceManager.Instantiate("UI/Msg", _contents.transform);
            TMP_Text msgText = msgGo.GetComponent<TMP_Text>();
            msgText.text = msg;
        }
    }

    void SendChat() 
    {
        GameObject chatInput = Utils.FindChild(gameObject, "ChatInput", true);
        string msg = chatInput.GetComponent<TMP_InputField>().text;

        if (msg.Trim().Equals("")) 
        {
            return;
        }

        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);
        BinaryWriter bw = new BinaryWriter(ms);
        ms.Position = 0;

        byte[] msgBytes = Encoding.Unicode.GetBytes(msg.Trim());
        Int16 pktSize = (Int16)(4 + msgBytes.Length);

        bw.Write((Int16)Define.PacketProtocol.C_T_S_CHATSEND_LOBBY);
        bw.Write(pktSize);
        bw.Write(msgBytes);

        _net.SendPacket(bytes, pktSize);

        TMP_InputField input = chatInput.GetComponent<TMP_InputField>();
        input.text = "";
        input.ActivateInputField();
        input.Select();
    }

    internal void Push(string msg)
    {
        if (chat.Count >= msgMaxSize)
        { 
            chat.RemoveAt(chat.Count - 1);           
        }
        chat.Insert(0,msg);
    }
}
