using UnityEngine;

public class ToggleStereo : getReal3D.MonoBehaviourWithRpc {

    private bool m_stereoEnabled;
    private float m_eyeSeparationBackup;

    void Start()
    {
        m_stereoEnabled = true;
    }

    void OnGUI()
    {
        if(!getReal3D.GUI.BeginGUI()) return;
        GUILayout.BeginArea(new Rect(150, 0, 150, getReal3D.GUI.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical("Stereo", GUI.skin.window);
        bool res = GUILayout.Toggle(m_stereoEnabled, "Stereo");
        if(res != m_stereoEnabled) {
            getReal3D.RpcManager.call("enableStereo", res);
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
        getReal3D.GUI.EndGUI();
    }

    [getReal3D.RPC]
    private void enableStereo(bool enabled)
    {
        if(enabled) {
            getReal3D.Plugin.SetEyeSeparation(m_eyeSeparationBackup);
        }
        else {
            m_eyeSeparationBackup = getReal3D.Plugin.GetEyeSeparation();
            getReal3D.Plugin.SetEyeSeparation(0.0f);
        }
        m_stereoEnabled = enabled;
    }
}
