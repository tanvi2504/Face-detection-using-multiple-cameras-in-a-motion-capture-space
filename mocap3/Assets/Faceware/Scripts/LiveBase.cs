using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using SimpleJSON;

public class LiveBase : MonoBehaviour
{
    public string LiveServerHostIP = "localhost";
    public int LiveServerHostPort = 802;
    public bool ConnectOnPlay = true;
    public bool ReconnectOnLostConnection = true;
    public bool DropPackets = true;
    public LiveCharacterSetupFile ExpressionSetFile;
    //Connection to Live Server
    public LiveConnection live;
    //Character Setup that Plugin is Driving
    public LiveCharacterSetup character = new LiveCharacterSetup();
    // A handle to the current DataSet driving the rig
    public SimpleJSON.JSONNode currentDatSet;
    // Threshold of how many calls to get data resulting in no data before we consider connection to Live Server lost.
    private int TimeoutThreshold = 90; // Roughly 1 and 1/2 seconds
    private int m_EmptyPacketsCounter = 0;

    void Start()
    {
        //Load the Character 
        if (ExpressionSetFile != null)
        {
            character.Init(ExpressionSetFile.Expressions, ExpressionSetFile.Controls);
            List<string> missingCtrlList = LiveUnityInterface.ValidateControls(this.gameObject, character.GetControlList());
            if (missingCtrlList.Count > 0)
            {
                string msg = "[Faceware Live] These controls are not in your scene:\n";
                foreach (string ctrl in missingCtrlList)
                {
                    msg += ctrl;
                    msg += "\n";
                }
                Debug.LogWarning(msg);
            }

            //Setup Connection to Live
            live = new LiveConnection(LiveServerHostIP, LiveServerHostPort);
            live.m_Reconnect = ReconnectOnLostConnection;
            // Connect on Play if toggled true
            if (ConnectOnPlay)
                Connect();
            live.m_DropPackets = DropPackets;
        }
    }

    public void BaseUpdate()
    {
        if (live != null && live.IsConnected() && ExpressionSetFile)
        {
            if (currentDatSet != null && currentDatSet.Count > 0)
            {
                m_EmptyPacketsCounter = 0;
            }
            else if (m_EmptyPacketsCounter >= TimeoutThreshold)
            {
                // Force reconnect as threshold of no data recieved has been met.
                Disconnect();
                Connect();
                m_EmptyPacketsCounter = 0;
            }
            else
            {
                m_EmptyPacketsCounter++;
            }
        }
    }

    // API to call a specific dataset from the current json object driving the rig
    public SimpleJSON.JSONNode GetControl(string controlName)
    {
        SimpleJSON.JSONNode control = null;
        try
        {
            control = currentDatSet[controlName];
        }
        catch
        {
            Debug.Log("[Faceware Live] Requested object is not within that data object. Ensure Live is streaming the requested data...");
        }
        return control;
    }

    public void Connect()
    {
        if (live != null)
        {
            if (!live.IsConnected())
            {
                live.m_HostIP = LiveServerHostIP;
                live.m_HostPort = LiveServerHostPort;
                live.Connect();
            }
            else
            {
                Debug.LogWarning("[Faceware Live] Live Client is already connected!");
            }
        }
        else
            Debug.LogWarning("[Faceware Live] Unable to Connect to Live Server Due to bad configurations or scene is not playing...");
    }

    public void Disconnect()
    {
        if (live != null)
            live.Disconnect();
    }

    public void OnSettingsChange()
    {
        if (live != null)
        {
            live.m_Reconnect = ReconnectOnLostConnection;
            live.m_DropPackets = DropPackets;
        }
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    // Note: Calibration can only be triggered if LiveServer and Unity are both on the same machine.
    // Calibration Logic utilizes Windows APIs to send a message directly to Live Server.
    public void CalibrateLiveServer()
    {
        IEnumerable<IntPtr> windows = LiveHelpers.FindWindowsWithText("Faceware Live");

        LiveHelpers.COPYDATASTRUCT cds = new LiveHelpers.COPYDATASTRUCT(); // Live only utilizes the dwData at this time
        cds.dwData = (IntPtr)1;

        int liveServerCount = 0;
        foreach (var window in windows)
        {
            LiveHelpers.SendMessage(window, 0X004A, IntPtr.Zero, ref cds);
            liveServerCount++;
        }

        if (liveServerCount > 0)
        {
            Debug.Log("[Faceware Live] Calibration message sent to " + liveServerCount + " instance(s) of Live!");
        }
        else
        {
            Debug.Log("[Faceware Live] No local instance of Live Server found! Please start Live Server!");
        }
    }
#endif

    void OnApplicationQuit()
    {
        Disconnect();
    }
}