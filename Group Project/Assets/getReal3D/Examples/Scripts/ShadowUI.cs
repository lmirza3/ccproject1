using UnityEngine;
using System.Collections;

public class ShadowUI
    : getReal3D.MonoBehaviourWithRpc
{
	private Light[] m_lights;

    /// <summary>
    /// Constructor
    /// </summary>
    /// Make sure RPC are not automatically registered so that we can disable the 
    /// behavior before registration.
    public ShadowUI() : base(false)
    {
        
    }

    void Start()
    {
        registerRpc();
        m_lights = FindObjectsOfType(typeof(Light)) as Light[];
    }

	void OnGUI()
	{
		if (!getReal3D.GUI.BeginGUI()) return;
		GUILayout.BeginArea(new Rect(0,0,150,getReal3D.GUI.height));
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical( "Lights", GUI.skin.window );
        foreach(Light l in m_lights)
		{
			bool res = GUILayout.Toggle( l.enabled, l.name );
            if(res != l.enabled) {
                getReal3D.RpcManager.call("enableLight", l.name, res);
            }
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
		getReal3D.GUI.EndGUI();
	}

    [getReal3D.RPC]
    void enableLight(string name, bool enabled)
    {
        foreach(Light l in m_lights) {
            if(l.name == name) {
                l.enabled = enabled;
            }
        }
    }
	
	void LateUpdate()
	{
		if (getReal3D.Input.GetKeyDown(getReal3D.VirtualKeyCode.VK_ESCAPE))
			Application.Quit();
	}
}
