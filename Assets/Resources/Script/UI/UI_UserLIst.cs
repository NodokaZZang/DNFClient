using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class UI_UserLIst : MonoBehaviour
{
    private int _page = 0;
    private int _pageCount = 0;
    private bool _userListCorutin = false;
    private Network _net;
    private GameObject[] _items = new GameObject[6 + 1];
    private TMP_Text _pageText;

    void Start()
    {
        for (int i = 1; i <= 6; i++)
            _items[i] = GameObject.Find($"UserItem{i}");

        _net = GameObject.FindWithTag("net").GetComponent<Network>();
        _pageText = GameObject.Find("PageText").GetComponent<TMP_Text>();
        GetUserList();
    }

    void Update()
    {
        if (_userListCorutin == false) 
        {
            _userListCorutin = true;
            StartCoroutine(CorutinUserListSync());
        }
    }

    void GetUserList()
    {
        byte[] sendBuffer = new byte[1024];
        MemoryStream ms = new MemoryStream(sendBuffer);
        ms.Position = 0;

        Define.Header header = new Define.Header();
        header._pktId = (int)Define.PacketProtocol.C_T_S_USERLIST;
        header._pktSize = (ushort)(4 + sizeof(int));

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write(header._pktId);
        bw.Write(header._pktSize);
        bw.Write(_page);
        _net.SendPacket(sendBuffer, (int)ms.Position);
    }

    internal void UserListSync(List<string> usernameList, int pageCount, int pageNum)
    {
        _page = pageNum;
        _pageCount = pageCount;

        const int pageViewCount = 6;

        for (int i = 1; i <= pageViewCount; i++) 
        {
            GameObject userItem = _items[i];
            GameObject textGo = userItem.transform.GetChild(0).gameObject;
            TMP_Text text = textGo.GetComponent<TMP_Text>();
            text.text = "";
        }


        for (int i = 0; i < usernameList.Count; i++) 
        {
            GameObject userItem = _items[i + 1];
            GameObject textGo = userItem.transform.GetChild(0).gameObject;
            TMP_Text text = textGo.GetComponent<TMP_Text>();
            text.text = usernameList[i];
        }

        _pageText.text = $"{pageNum+1}/{pageCount+1}";
    }

    IEnumerator CorutinUserListSync()
    {
        yield return new WaitForSeconds(1.5f);
        GetUserList();
        _userListCorutin = false;
    }

    public void NextPage() 
    {
        if (_page + 1 <= _pageCount)
        {
            _page++;
            GetUserList();
        }
    }

    public void PrevPage() 
    {
        if (_page - 1 >= 0)
        {
            _page--;
            GetUserList();
        }
    }
}
