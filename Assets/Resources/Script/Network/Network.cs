using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Network : MonoBehaviour
{
    private TCPConnector _connector = new TCPConnector();
    private UDPServer _udpServer = new UDPServer();
    private Thread _tcpThread;
    private Thread _udpThread;

    private PacketHandler packetHandler = new PacketHandler();
    private const int _recvBufferSize = 4096 * 5;
    private byte[] _recvBuffer = new byte[_recvBufferSize];
    public string LocalIp 
    {
        get
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    void Awake()
    {
        Init();
    }

    void Start()
    {   
        
    }

    void Init() 
    {
        StartCoroutine(CoPublicIpSend());

        if (_connector.ConnectTo("127.0.0.1", 30002))
        {
            _tcpThread = new Thread(new ThreadStart(TCPRecvProc));
            _tcpThread.Start();
        }

        _udpThread = new Thread(new ThreadStart(UDPRecvPorc));
        _udpThread.Start();
    }

    public void SendPacket(byte[] buffer, int sendSize) 
    {
        _connector.ConnectSocket.Send(buffer, sendSize, SocketFlags.None);
    }

    public void UDPInit(List<Define.PlayerUDPInfo> ipList) 
    {
        _udpServer.InitIPList(ipList); 
    }

    void Update()
    {
        ArraySegment<byte> packet = PacketQueue.Instance.Pop();

        if (packet != null) 
        {
            packetHandler.Handler(packet);
        }
    }

    private void TCPRecvProc() 
    {
        int recvSize = 0;
        int readPos = 0;
        int writePos = 0;
        try
        {
            while (true)
            {
                recvSize = _connector.ConnectSocket.Receive(_recvBuffer, writePos, _recvBuffer.Length - writePos, SocketFlags.None);

                if (recvSize < 1)
                {
                    _connector.ConnectSocket.Close();
                    break;
                }

                writePos += recvSize;
                // [200][100][200][100]
                while (true)
                {
                    int dataSize = Math.Abs(writePos - readPos);

                    if (dataSize < 4) break;

                    ArraySegment<byte> pktCodeByte = new ArraySegment<byte>(_recvBuffer, readPos, readPos + sizeof(UInt16));
                    ArraySegment<byte> pktSizeByte = new ArraySegment<byte>(_recvBuffer, readPos + sizeof(UInt16), readPos + sizeof(UInt16));

                    Int16 pktCode = BitConverter.ToInt16(pktCodeByte);
                    Int16 pktSize = BitConverter.ToInt16(pktSizeByte);

                    if (pktSize > dataSize)
                        break;

                    ArraySegment<byte> segment = new ArraySegment<byte>(_recvBuffer, readPos, pktSize);
                    byte[] data = new byte[pktSize];

                    Array.Copy(segment.ToArray(), data, pktSize);

                    PacketQueue.Instance.Push(data);

                    // TODO 데이터 처리
                    readPos += pktSize;

                    if (readPos == writePos)
                    {
                        readPos = 0;
                        writePos = 0;
                    }
                    else if (writePos >= 4096 * 4)
                    {
                        Buffer.BlockCopy(_recvBuffer, readPos, _recvBuffer, 0, dataSize);
                        writePos = dataSize;
                    }

                } 
            }
        }
        catch (Exception e) 
        {
            Debug.LogException(e);
        }
    }

    private void UDPRecvPorc() 
    {
        try
        {
            while (true) 
            {
                IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)client;

                byte[] recvBuffer = new byte[1024];
                
                int numOfBytes = _udpServer.UDPSocket.ReceiveFrom(recvBuffer, ref remote);

                ArraySegment<byte> segment = new ArraySegment<byte>(recvBuffer, 0, numOfBytes);

                PacketQueue.Instance.Push(segment);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    internal void UDPPlayerSend(int playerId, byte[] bytes, short pktHeader) 
    {
        _udpServer.UDPPlayerSend(playerId, bytes, pktHeader);
    }

    internal void UDPBrodCast(byte[] bytes, short pktHeader)
    {
        _udpServer.BroadCast(bytes, pktHeader);
    }

    public void SendUDPINFO(string ip, int port) 
    {
        byte[] bytes = new byte[1024];
        MemoryStream ms = new MemoryStream(bytes);
        BinaryWriter bw = new BinaryWriter(ms);
        ms.Position = 0;

        Managers.Instance.DataManager.PublicIP = ip;

        byte[] ipBytes = System.Text.Encoding.Unicode.GetBytes(ip);
        byte[] localipBytes = System.Text.Encoding.Unicode.GetBytes(LocalIp); // 이부분이 지금 수상한다 여기서 LocalIp가 이상하게 뽑혔다 추정


        int pktSize = 4 + 4 + ipBytes.Length + 4 + localipBytes.Length + 4;

        bw.Write((Int16)Define.PacketProtocol.C_T_S_UDPIPPORTSEND);
        bw.Write((Int16)pktSize);
        bw.Write((Int32)ipBytes.Length);
        bw.Write(ipBytes);
        bw.Write((Int32)localipBytes.Length);
        bw.Write(localipBytes);
        bw.Write((Int32)port);
        SendPacket(bytes, pktSize);
    }

    IEnumerator CoPublicIpSend() 
    {
        while (true) 
        {
            byte[] data = new byte[100];

            // TOOD 콜백을 받자

            MemoryStream ms = new MemoryStream(data);
            BinaryWriter bw = new BinaryWriter(ms);

            Int16 pktHeader = (Int16)(4);

            bw.Write((Int16)Define.PacketProtocol.C_T_S_IPPORTINFO);
            bw.Write((Int16)pktHeader);

            if (Managers.Instance.DataManager.PublicIP != null)
                yield break;

            _udpServer.SendToServer(data, pktHeader);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
