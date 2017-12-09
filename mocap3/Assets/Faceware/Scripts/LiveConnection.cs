﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SimpleJSON;

public class LiveConnection
{

    public string m_HostIP { get; set; }
    public int m_HostPort { get; set; }
    public bool m_Reconnect { get; set; }
    public bool m_DropPackets { get; set; }

    public List<SimpleJSON.JSONNode> m_LiveData;

    private TcpClient m_Tcp;

    public LiveConnection()
    {
        m_HostIP = "localhost";
        m_HostPort = 802;
        m_Reconnect = true;
        m_DropPackets = false;

        m_LiveData = new List<SimpleJSON.JSONNode>();
    }

    public LiveConnection(string ip, int port)
    {
        m_HostIP = ip;
        m_HostPort = port;
        m_Reconnect = true;
        m_DropPackets = false;

        m_LiveData = new List<SimpleJSON.JSONNode>();
    }

    public void Connect()
    {
        m_Tcp = new TcpClient();
        m_Tcp.BeginConnect(m_HostIP, m_HostPort, BeginConnectCallback, m_Tcp);
    }

    public void Disconnect()
    {
        if (m_Tcp != null)
        {
            PrintWarning("Disconnected from Live Server!");
            m_Tcp.Close();
        }
    }

    public bool IsConnected()
    {
        if(m_Tcp == null || !m_Tcp.Connected)
        {
            return false;
        }
        else if (m_Tcp.Client.Poll(0, SelectMode.SelectRead))
        {
            byte[] buff = new byte[1];
            if (m_Tcp.Client.Receive(buff, SocketFlags.Peek) == 0)
            {
                // Client disconnected
                return false;
            }
        }
        return true;
    }

    public void BeginConnectCallback(IAsyncResult asyncResult)
    {
        if (IsConnected())
        {
            PrintMessage("Connected to Live Server!");
            GetNextMessage();
        }
        else
        {
            PrintWarning("Failed to Connect to Live Server!");
            if(m_Reconnect)
                m_Tcp.BeginConnect(m_HostIP, m_HostPort, BeginConnectCallback, m_Tcp);
        }
    }

    private void GetNextMessage()
    {
        if (IsConnected() )
        {
            GetHeader();
        }
        else
        {
            PrintError("Live Server Connection Lost!");
            m_Tcp.Close();
            if (m_Reconnect) // If true, always reconnect upon receiving bad data
            {
                PrintMessage("Attempting to Reestablish Connection with Live Server!");
                Connect();
            }
        }
    }

    private void OnNewMessageComplete(string result)
    {
        // Broken JSON to test reconnection logic on bad packet
        //result = @"{""result"":{""success"":true,""value"":""8cb2237d0679ca88db6464eac60da96345513964}";
        if (result != null && result != "")
        {
            try
            {
                SimpleJSON.JSONNode json = JSON.Parse(result);
                if (json != null)
                {
                    // Valid Data, Add it to the data list
                    m_LiveData.Add(json);
                }
                else
                {
                    PrintWarning("Error: Deserialization of \n" + result + "\nfailed! Malformed JSON...");
                }
            }
            catch (Exception e)
            {
                PrintWarning("Error: Deserialization of \n" + result + "\n" + e.Message + "\nTrace: " + e.StackTrace);
            }
        }
        GetNextMessage();
    }

    public void GetHeader()
    {
        NetworkStream stream = m_Tcp.GetStream();
        int headerSize = 4;
        byte[] header = new byte[headerSize];

        AsyncCallback headerCB = headerRead =>
        {
            stream.EndRead(headerRead);
            int bodySize = BitConverter.ToInt32(header, 0);

            GetMessage(bodySize);
        };
        stream.BeginRead(header, 0, headerSize, headerCB, null);
    }

    public void GetMessage(int bodySize)
    {
        NetworkStream stream = m_Tcp.GetStream();
        byte[] body = new byte[bodySize];
        AsyncCallback bodyCB = bodyRead =>
        {
            stream.EndRead(bodyRead);
            string data = Encoding.ASCII.GetString(body);

            OnNewMessageComplete(data);
        };
        stream.BeginRead(body, 0, bodySize, bodyCB, null);
    }

    public SimpleJSON.JSONNode GetLiveData()
    {
        SimpleJSON.JSONNode data = null;
        if (m_LiveData.Count > 0)
        {
            data = m_LiveData[0];
            m_LiveData.RemoveAt(0);
            if((m_DropPackets) && m_LiveData.Count > 0)
            {
                PrintMessage("Dropping " + m_LiveData.Count + " Packets");
                m_LiveData.Clear();
            }
        }
        return data;
    }

    private void PrintMessage(string msg)
    {
        Debug.Log("[Faceware Live] " + msg);
    }

    private void PrintWarning(string msg)
    {
        Debug.LogWarning("[Faceware Live] " + msg);
    }

    private void PrintError(string msg)
    {
        Debug.LogError("[Faceware Live] " + msg);
    }
}

