using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;


public class UI_LoginButton : MonoBehaviour
{
    private Network _net;

    private void Start()
    {
        _net = GameObject.FindWithTag("net").GetComponent<Network>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return)) 
        {
            DoLogin();
        }   
    }


    public void OnButtonClicked() 
    {
        DoLogin();
    }

    private void DoLogin() 
    {
        TMP_Text text = Utils.FindChild<TMP_Text>(gameObject, "UsernameText", true);
        TMP_Text errorText = Utils.FindChild<TMP_Text>(gameObject, "ErrorLog", true);
        string username = text.text.Replace("\u200B", string.Empty);
        int usernameSize = username.Length;

        if (usernameSize <= 1)
        {
            errorText.enabled = true;
            errorText.text = "유저 이름은 1자리 이상입니다";
            return;
        }

        if (usernameSize > 10)
        {
            errorText.enabled = true;
            errorText.text = "유저 이름은 10자리 이내입니다";
            return;
        }

        for (int i = 0; i < usernameSize; i++)
        {
            if (username[i] == ' ')
            {
                errorText.enabled = true;
                errorText.text = "유저 이름에 공백이 들어갈수 없습니다";
                return;
            }
        }

        errorText.enabled = false;
        Define.Header header = new Define.Header();
        byte[] usernmaeByte = Encoding.Unicode.GetBytes(username.Trim());
        header._pktId = (int) Define.PacketProtocol.C_T_S_LOGIN;
        header._pktSize = (ushort)(usernmaeByte.Length + 4);

        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write(header._pktId);
        bw.Write(header._pktSize);
        bw.Write(usernmaeByte);


        _net.SendPacket(bytes, header._pktSize);
    }
}
