using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WatingRoom : MonoBehaviour
{
    private Network _net;
    private GameObject[] _items = new GameObject[4 + 1];
    private Sprite _swordManPick;
    private Sprite _gunnerPick;
    private Sprite _original;
    private int _playerId;
    private int _playerOrder;
    private bool _ready = false;
    private TMP_Text _buttonText;
    private bool _isAdmin = false;
    private Color _originalColor;
    void Start()
    {
        for (int i = 1; i <= 4; i++)
            _items[i] = GameObject.Find($"User{i}");

        _swordManPick = Resources.Load<Sprite>("UI/Img/SwordManPick"); 
        _gunnerPick = Resources.Load<Sprite>("UI/Img/GunnerPick");
        _original = GameObject.Find("User1").transform.GetChild(0).GetComponent<Image>().sprite;
        _originalColor = GameObject.Find("User1").transform.GetChild(0).GetComponent<Image>().color;
        _buttonText = GameObject.Find("ReadyText").GetComponent<TMP_Text>();

        _net = GameObject.FindWithTag("net").GetComponent<Network>();
        Init();
    }

    void Update()
    {
        
    }

    private void Init()
    {
        byte[] sendBuffer = new byte[1024];
        MemoryStream ms = new MemoryStream(sendBuffer);
        ms.Position = 0;

        Define.Header header = new Define.Header();
        header._pktId = (int)Define.PacketProtocol.C_T_S_WATINGROOMINIT;
        header._pktSize = (ushort)(4 + sizeof(int));

        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(header._pktId);
        bw.Write(header._pktSize);
        bw.Write(Managers.Instance.DataManager.RoomSQ);
        _net.SendPacket(sendBuffer, header._pktSize);
    }

    internal void Sync1(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        
        Int32 myPlayerId = br.ReadInt32();
        _playerId = myPlayerId;
        Int32 myPlayerOrder = br.ReadInt32();
        _playerOrder = myPlayerOrder;
        Int32 joinCnt = br.ReadInt32();

        for (int i = 1; i <= 4; i++) 
        {
            GameObject go = _items[i];
            Image PickSprite = Utils.FindChild(go, "Pick").GetComponent<Image>();
            PickSprite.sprite = _original;
            Image UsernameImage = Utils.FindChild(go, "Username").GetComponent<Image>();
            UsernameImage.color = _originalColor;
            TMP_Text Username = Utils.FindChild(go, "UsernameTxt", true).GetComponent<TMP_Text>();
            Username.text = "";
        }


        for (int i = 0; i < joinCnt; i++) 
        {
            Int32 playerOrder = br.ReadInt32();
            Int32 usernameSize = br.ReadInt32();
            byte[] usernameByte = br.ReadBytes(usernameSize);
            string username = Encoding.Unicode.GetString(usernameByte);
            Int32 playerPick = br.ReadInt32();
            bool isReady = br.ReadBoolean();

            GameObject go = _items[playerOrder + 1];
            Image PickSprite = Utils.FindChild(go, "Pick").GetComponent<Image>();
            TMP_Text Username = Utils.FindChild(go, "UsernameTxt", true).GetComponent<TMP_Text>();
            Image UsernameImage = Utils.FindChild(go, "Username").GetComponent<Image>();
            
            if (playerPick == 0)
            {
                PickSprite.sprite = _original;
            }
            else if (playerPick == 1)
            {
                PickSprite.sprite = _swordManPick;
            }
            else 
            {
                PickSprite.sprite = _gunnerPick;
            }


            if (_isAdmin == false && myPlayerOrder == i) 
            {
                _ready = isReady;
                if (_ready)
                    _buttonText.text = "취 소";
                else
                    _buttonText.text = "준 비";
            }

            if (isReady)
            {
                UsernameImage.color = Color.green;
            }
            else
            {
                UsernameImage.color = Color.red;
            }

            Username.text = username;
        }
    }

    internal void ISAdmin(int adminSQ)
    {
        _isAdmin = true;
        _buttonText.text = "게임시작";
    }

    public void OutRoom() 
    {
        byte[] sendBuffer = new byte[1024];
        MemoryStream ms = new MemoryStream(sendBuffer);
        ms.Position = 0;

        Define.Header header = new Define.Header();
        header._pktId = (int)Define.PacketProtocol.C_T_S_WATINGROOMOUTUSER;
        header._pktSize = (ushort)(4 + sizeof(int));

        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(header._pktId);
        bw.Write(header._pktSize);
        _net.SendPacket(sendBuffer, header._pktSize);
    }

    public void Ready() 
    {
        bool myReady = !_ready;

        byte[] sendBuffer = new byte[1024];
        MemoryStream ms = new MemoryStream(sendBuffer);
        ms.Position = 0;

        Define.Header header = new Define.Header();
        header._pktId = (int)Define.PacketProtocol.C_T_S_USERREADY;
        header._pktSize = (ushort)(sizeof(bool) + sizeof(int) + sizeof(bool));

        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(header._pktId);
        bw.Write(header._pktSize);
        bw.Write(myReady);
        bw.Write(_isAdmin);

        _net.SendPacket(sendBuffer, header._pktSize);
    }
}
