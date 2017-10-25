using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using getReal3D;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("getReal3D/Updater/Camera Updater")]

public class getRealCameraUpdater
    : getRealUserScript
{
    private static List<getRealUser> m_userCameraHandled = null;

    private Transform m_transform;
    private Camera m_camera;

    public int cameraIndex = 0;
    public bool applyHeadPosition = true;
    public bool applyHeadRotation = true;
    public bool applyCameraProjection = true;

    private float m_mainNearClip;
    private float m_mainFarClip;

    private bool m_pluginInitialized = false;

    private bool m_useRenderToTexture = false;

    public GameObject CameraPrefab = null;

    public enum ViewportType { Automatic, UserModulated, UserOverride };
    public ViewportType viewportType;
    public Rect userViewport = new Rect(0f, 0f, 1f, 1f);

    public bool updateOnPreCull = true;

    void Awake()
    {
        m_transform = transform;
        m_camera = GetComponent<Camera>();
        m_pluginInitialized = getReal3D.Input.Init();
    }

    void Start()
    {
        getReal3D.Plugin.debug("Camera updater start.");

        m_mainNearClip = m_camera.nearClipPlane;
        m_mainFarClip = m_camera.farClipPlane;

        List<int> neededCameras = computeNeededCameras();

        if(!m_pluginInitialized) {
            Debug.LogError("Failed to initialize GetReal3DPlugin");
            return;
        }

        /// Ensure we call CreateCameras only once per user
        if(m_userCameraHandled == null || m_userCameraHandled.IndexOf(user()) < 0) {
            CreateCameras();
        }
        else {
            getReal3D.Plugin.debug("Cameras have already been duplicated for that user.");
        }

        if(cameraIndex < 0 || cameraIndex >= getReal3D.Input.cameras.Count || neededCameras.Count == 0) {
            enabled = false;
            GetComponent<Camera>().enabled = false;
            return;
        }

        GetComponent<Camera>().enabled = true;

        Rect viewport = new Rect(0f, 0f, 1f, 1f);

        switch(viewportType) {
        case ViewportType.Automatic:
            getReal3D.Plugin.getCameraViewport((uint) cameraIndex, ref viewport);
            break;
        case ViewportType.UserModulated:
            getReal3D.Plugin.getCameraViewport((uint) cameraIndex, ref viewport);
            viewport.Set(userViewport.x * viewport.width + viewport.x, userViewport.y * viewport.height + viewport.y,
                          userViewport.width * viewport.width, userViewport.height * viewport.height);
            break;
        case ViewportType.UserOverride:
            viewport = userViewport;
            break;
        }

        GetComponent<Camera>().rect = viewport;

        GetComponent<Camera>().renderingPath = getReal3D.Config.renderingPath;

        m_useRenderToTexture = getReal3D.Plugin.getCameraUseRTT((uint)cameraIndex);
        if (m_useRenderToTexture){
             ensureRenderToTexture();
        }
    }

    private List<int> computeNeededCameras()
    {
        List<int> needCameras = new List<int>();
        for(int i = 0; i < getReal3D.Input.cameras.Count; ++i) {
            if(getReal3D.Plugin.getUserFromCamera((uint) i) == userId()) {
                needCameras.Add(i);
            }
        }
        return needCameras;
    }

    void CreateCameras()
    {
        List<int> needCameras = computeNeededCameras();
        if(m_userCameraHandled == null) {
            m_userCameraHandled = new List<getRealUser>();
        }
        m_userCameraHandled.Add(user());

        getReal3D.Plugin.debug("User requires cameras: {" + string.Join(", ", needCameras.ConvertAll(i => i.ToString()).ToArray()) + "}.");

        if (needCameras.Count == 0) {
            return;
        }

        cameraIndex = needCameras[0];
        needCameras.RemoveAt(0);

        // find cameras, see which we can remove from needCameras
        foreach(Camera cam in transform.root.GetComponentsInChildren<Camera>()) {
            if(cam.GetComponent<getRealCameraUpdater>() != null && (cam.name == name || cam.name == name + "(Clone)")) {
                int idx = cam.GetComponent<getRealCameraUpdater>().cameraIndex;
                if(idx >= 0) {
                    getReal3D.Plugin.debug("Camera " + idx.ToString() + " already exists.");
                    needCameras.Remove(idx);
                }
            }
        }

        // make missing cameras
        foreach(int idx in needCameras) {

            getReal3D.Plugin.debug("Creating camera " + idx.ToString() + ".");

            GameObject newCamObject = null;
            if(CameraPrefab == null) {
                newCamObject = Instantiate(gameObject) as GameObject;
            }
            else {
                newCamObject = Instantiate(CameraPrefab) as GameObject;
            }

            foreach(AudioListener listener in newCamObject.GetComponents<AudioListener>()) {
                Destroy(listener);
            }

            newCamObject.transform.parent = transform.parent;
            newCamObject.tag = gameObject.tag;
            newCamObject.layer = gameObject.layer;

            getRealCameraUpdater camUpdater = newCamObject.GetComponent<getRealCameraUpdater>();
            if(camUpdater == null) camUpdater = newCamObject.AddComponent<getRealCameraUpdater>();
            camUpdater.cameraIndex = idx;

            newCamObject.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
        }

        getReal3D.Plugin.debug("Camera creation done.");
    }

    void Update()
    {
        if(!updateOnPreCull)
            UpdateCamera();
    }

    void OnPreCull()
    {
        if(updateOnPreCull)
            UpdateCamera();
    }

    void UpdateCamera()
    {
        if(m_useRenderToTexture) {
            ensureRenderToTexture();
        }
        if(m_pluginInitialized) {
            if(applyHeadPosition) {
                m_transform.localPosition = getReal3D.Input.GetCameraSensor((uint) cameraIndex).position;
            }
            if(applyHeadRotation) {
                m_transform.localRotation = getReal3D.Input.GetCameraSensor((uint) cameraIndex).rotation;
            }
            if(applyCameraProjection) {
                m_camera.projectionMatrix = getReal3D.Input.GetCameraProjection((uint) cameraIndex, m_mainFarClip, m_mainNearClip);
            }
        }
    }

    void OnPreRender()
    {
#if     UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
        GL.IssuePluginEvent(Plugin.getCameraProfilingCallback(), cameraIndex);
#       endif
    }

    void OnPostRender()
    {
        if(m_camera.targetTexture) {
            Plugin.setTextureFromUnity(cameraIndex, m_camera.targetTexture.GetNativeTexturePtr());
            issueCameraEvent();
        }
        else {
            StartCoroutine(WaitAndIssuePluginEvent());
        }
    }

    private void issueCameraEvent()
    {
        /// Seems dumb, but UNITY_X_Y_OR_NEWER was introduced in Unity 5.3.4
#       if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
        GL.IssuePluginEvent(Plugin.getCameraEventCallback(), cameraIndex);
#       else
        GL.IssuePluginEvent(cameraIndex);
#       endif
    }

    private IEnumerator WaitAndIssuePluginEvent()
    {
        yield return new WaitForEndOfFrame();
        issueCameraEvent();
    }

    private void ensureRenderToTexture()
    {
        int w = getReal3D.Plugin.getCameraWidth((uint) cameraIndex);
        int h = getReal3D.Plugin.getCameraHeight((uint) cameraIndex);
        if(!m_camera.targetTexture ||
            m_camera.targetTexture.width != w ||
            m_camera.targetTexture.height != h) {
            getReal3D.Plugin.debug("Creating new RenderTexture of size " + w.ToString() + "x" + h.ToString() + ".");
            GetComponent<Camera>().targetTexture = new RenderTexture(w, h, 24);
        }
    }
}
