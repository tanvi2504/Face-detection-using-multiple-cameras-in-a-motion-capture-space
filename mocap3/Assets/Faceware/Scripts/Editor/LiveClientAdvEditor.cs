using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;

[CustomEditor(typeof(LiveClientAdv))]
public class LiveClientAdvEditor : Editor
{

	LiveClientAdv FwLive ;
	Texture titleIcon ;
	GUIStyle titleStyle ;

	void OnEnable ()
	{

		titleStyle = new GUIStyle () ;
		titleStyle.fontStyle = FontStyle.Bold ;
		titleStyle.fontSize = 12 ;
		titleStyle.margin = new RectOffset (5, 0, 6, 6);

		FwLive = (LiveClientAdv)target ;

		// Faceware Icon
		string iconPath = Utility.CombinePath( "Assets", "Faceware", "Scripts", "Editor", "Icons" );
		titleIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "LiveClient_DeviceHeader.png" ), typeof( Texture ) );
	}

	public override void OnInspectorGUI()
    {
		EditorGUILayout.BeginVertical();
		{
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(titleIcon, GUILayout.MinWidth(200));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Version " + LiveHelpers.CurrentVersion(), titleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label( "Faceware Live Client for Unity", titleStyle );

            GUILayout.BeginVertical();
            {
                //Server
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Live Server Hostname: ");
                    FwLive.LiveServerHostIP = EditorGUILayout.TextField("", FwLive.LiveServerHostIP, GUILayout.MinWidth(50));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                //Port
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Live Server Port: ");
                    FwLive.LiveServerHostPort = EditorGUILayout.IntField("", FwLive.LiveServerHostPort, GUILayout.MinWidth(50));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                // Connect on Start Toggle
                FwLive.ConnectOnPlay = GUILayout.Toggle(FwLive.ConnectOnPlay, "Connect on Play");

                // Reconnect on Server Lost
                FwLive.ReconnectOnLostConnection = GUILayout.Toggle(FwLive.ReconnectOnLostConnection, new GUIContent("Automatic Reconnect", "When connection to Live Server is lost, enabling this checkbox will allow the plugin to automatically attempt reconnecting"));

                // Drop Packets Flag
                FwLive.DropPackets = GUILayout.Toggle(FwLive.DropPackets, new GUIContent("Drop Packets on Update", "Drop packets when there is more than 1 packet from Live Server queued."));

                EditorGUILayout.BeginVertical("Box"); //Begin Live Server Interface
                {
                    EditorGUILayout.LabelField("Live Server", titleStyle);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Connect"))
                        {
                            FwLive.Connect();
                        }

                        if (GUILayout.Button("Disconnect"))
                        {
                            FwLive.Disconnect();
                        }
                    }
                    GUILayout.EndHorizontal();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    if (FwLive.LiveServerHostIP == "localhost" ||
                       FwLive.LiveServerHostIP == "127.0.0.1" ||
                       FwLive.LiveServerHostIP == Network.player.ipAddress)
                    {
                        if (GUILayout.Button("Calibrate"))
                        {
                            FwLive.CalibrateLiveServer();
                        }
                    }
#endif
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();

			EditorGUILayout.Space ();

            EditorGUILayout.BeginVertical("Box"); //Begin help buttons region
            {
                EditorGUILayout.LabelField("Need Help?", titleStyle);

                EditorGUILayout.BeginHorizontal(); //Live client user guide region
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Live Client for Unity - User Guide"))
                    {
                        System.Diagnostics.Process.Start("http://support.facewaretech.com/live-client-for-unity");
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal(); //End user guide region

                EditorGUILayout.BeginHorizontal(); //Visit website region
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Visit www.facewaretech.com"))
                    {
                        System.Diagnostics.Process.Start("http://www.facewaretech.com/");
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal(); //End website region

                EditorGUILayout.BeginHorizontal(); //30 day trial region
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("30-Day Free Trial", "Click here to get your 30-day free trial of Faceware Live Server.")))
                    {
                        System.Diagnostics.Process.Start("http://facewaretech.com/products/software/free-trial/");
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal(); //End 30 day trial region
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical ();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(FwLive);
            FwLive.OnSettingsChange();
        }
	}
}
