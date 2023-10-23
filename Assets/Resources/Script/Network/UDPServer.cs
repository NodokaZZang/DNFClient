using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using UnityEngine;

public class UDPServer 
{
    private Socket _socket;
    public IPEndPoint _ipep;
    private List<Define.PlayerUDPInfo> _ipList = new List<Define.PlayerUDPInfo>();
    IPEndPoint _cppUdpServer = new IPEndPoint(IPAddress.Parse("58.236.86.23"), 30003);

    public Socket UDPSocket { get { return _socket; } }

    public UDPServer() 
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Interval = 1500;
        timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        timer.Start();
    }

    void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
       byte[] data = new byte[100];

        MemoryStream ms = new MemoryStream(data);
        BinaryWriter bw = new BinaryWriter(ms);

        Int16 pktHeader = (Int16)(4);

        bw.Write((Int16)Define.PacketProtocol.C_T_S_UDPPING);
        bw.Write((Int16)pktHeader);

        _socket.SendTo(data, pktHeader, SocketFlags.None, _cppUdpServer);
    }

    public void InitIPList(List<Define.PlayerUDPInfo> ipList) 
    {
        _ipList.Clear();

        foreach (Define.PlayerUDPInfo udpInfo in ipList)
            _ipList.Add(udpInfo);
    }

    public void BroadCast(byte[] data, int size) 
    {
        foreach (Define.PlayerUDPInfo udpInfo in _ipList)
            _socket.SendTo(data, size, SocketFlags.None, (EndPoint)udpInfo.ipep);
    }

    internal void UDPPlayerSend(int playerId, byte[] bytes, short pktHeader)
    {
        foreach (Define.PlayerUDPInfo udpInfo in _ipList) 
        {
            if (udpInfo.playerId == playerId)
            { 
                _socket.SendTo(bytes, pktHeader, SocketFlags.None, (EndPoint)udpInfo.ipep);
                break;
            }
        }
    }

    public void SendToServer(byte[] data, int size) 
    {
        _socket.SendTo(data, size, SocketFlags.None, _cppUdpServer);
    }

    //public int GetAvailablePort(int startPort, int endPort)
    //{
    //    IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

    //    IPEndPoint[] tcpEndPointArray = ipGlobalProperties.GetActiveTcpListeners();

    //    List<int> tcpPortList = tcpEndPointArray.Select(p => p.Port).ToList<int>();

    //    IPEndPoint[] udpEndPointArray = ipGlobalProperties.GetActiveUdpListeners();

    //    List<int> udpPortList = udpEndPointArray.Select(p => p.Port).ToList<int>();

    //    TcpConnectionInformation[] tcpConnectionInformationArray = ipGlobalProperties.GetActiveTcpConnections();

    //    List<int> usedPortList = tcpConnectionInformationArray.Where(p => p.State != TcpState.Closed).Select(p => p.LocalEndPoint.Port).ToList<int>();

    //    usedPortList.AddRange(tcpPortList.ToArray());
    //    usedPortList.AddRange(udpPortList.ToArray());

    //    int unusedPort = 0;

    //    for (int port = startPort; port < endPort; port++)
    //    {
    //        if (!usedPortList.Contains(port))
    //        {
    //            unusedPort = port;

    //            break;
    //        }
    //    }

    //    return unusedPort;
    //}
}
