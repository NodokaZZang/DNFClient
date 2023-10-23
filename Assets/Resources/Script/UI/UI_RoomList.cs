using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class UI_RoomList : MonoBehaviour
{
    private int _page = 0;
    private int _pageCount = 0;
    private bool _roomListCorutin = false;
    private Network _net;
    private GameObject[] _items = new GameObject[6 + 1];
    private TMP_Text _pageText;
    void Start()
    {
        for (int i = 1; i <= 6; i++)
            _items[i] = GameObject.Find($"RoomItem{i}");

        _net = GameObject.FindWithTag("net").GetComponent<Network>();
        _pageText = GameObject.Find("RoomListPageText").GetComponent<TMP_Text>();
        GetRoomList();
    }


    void Update()
    {
        if (_roomListCorutin == false) 
        {
            _roomListCorutin = true;
            StartCoroutine(CorutinRoomListSync());
        }
    }

    void GetRoomList() 
    {
        byte[] sendBuffer = new byte[1024];
        MemoryStream ms = new MemoryStream(sendBuffer);
        ms.Position = 0;

        Define.Header header = new Define.Header();
        header._pktId = (int)Define.PacketProtocol.C_T_S_ROOMLIST;
        header._pktSize = (ushort)(4 + sizeof(int));

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write(header._pktId);
        bw.Write(header._pktSize);
        bw.Write(_page);
        _net.SendPacket(sendBuffer, (int)ms.Position);
    }

    IEnumerator CorutinRoomListSync()
    {
        yield return new WaitForSeconds(1.5f);
        GetRoomList();
        _roomListCorutin = false;
    }

    public void NextPage()
    {
        if (_page + 1 <= _pageCount)
        {
            _page++;
            GetRoomList();
        }
    }

    public void PrevPage()
    {
        if (_page - 1 >= 0)
        {
            _page--;
            GetRoomList();
        }
    }

    internal void RoomListSync(List<Room> roomList, int pageCount, int pageNum) 
    {
        _page = pageNum;
        _pageCount = pageCount;

        const int pageViewCount = 6;

        for (int i = 1; i <= pageViewCount; i++)
        {
            GameObject roomItem = _items[i];
            TMP_Text Number = roomItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
            TMP_Text Status = roomItem.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
            TMP_Text Title = roomItem.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
            TMP_Text JoinCnt = roomItem.transform.GetChild(3).gameObject.GetComponent<TMP_Text>();
            TMP_Text RoomSQ = roomItem.transform.GetChild(4).gameObject.GetComponent<TMP_Text>();

            Number.text = "";
            Status.text = "";
            Title .text = "";
            JoinCnt.text = "";
            RoomSQ.text = "";
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            Room room = roomList[i];

            GameObject roomItem = _items[i + 1];
            TMP_Text Number = roomItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
            TMP_Text Status = roomItem.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
            TMP_Text Title = roomItem.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
            TMP_Text JoinCnt = roomItem.transform.GetChild(3).gameObject.GetComponent<TMP_Text>();
            TMP_Text RoomSQ = roomItem.transform.GetChild(4).gameObject.GetComponent<TMP_Text>();

            Number.text = ((pageNum * 6) + (i+1)).ToString();
            Status.text = room.Status == 0 ? "대기" : "게임";
            Title.text = room.Title;
            JoinCnt.text = room.JoinCnt.ToString();
            RoomSQ.text = room.RoomSQ.ToString();
        }
        _pageText.text = $"{pageNum + 1}/{pageCount + 1}";
    }
}
