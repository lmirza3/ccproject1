using UnityEngine;
using System.Collections;

public class getRealExit : MonoBehaviour {
    
    public getReal3D.VirtualKeyCode m_exitKey = getReal3D.VirtualKeyCode.VK_ESCAPE;

    void Update()
    {
        if(getReal3D.Input.GetKeyDown(m_exitKey)) {
#if         UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#           else
            getReal3D.Plugin.clusterShutdown();
#           endif
        }
    }
}
