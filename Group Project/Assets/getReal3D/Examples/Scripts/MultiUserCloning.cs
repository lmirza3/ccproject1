using UnityEngine;
using System.Collections;
using System;

public class MultiUserCloning : MonoBehaviour {

    public Transform[] m_cloningPositions = null;
    public getRealUser m_userToClone = null;
    public string m_avatarLayerPrefix = "Avatar_";

    void Start()
    {
        uint userCount = getReal3D.Plugin.getUserCount();
        uint cloningPositionsCount = (uint) m_cloningPositions.Length;
        if(userCount > cloningPositionsCount) {
            getReal3D.Plugin.error("Not enough cloning position defined in MultiUserCloning instance.");
        }
        for(uint i = 0; i < userCount - 1 && i < cloningPositionsCount; ++i) {
            doClone(i+1, m_cloningPositions[i].position, m_cloningPositions[i].rotation);
        }
    }

    protected virtual getRealUser doClone(uint userId, Vector3 position, Quaternion rotation)
    {
        getRealUser clone = UnityEngine.Object.Instantiate(m_userToClone, position, rotation) as getRealUser;
        clone.transform.parent = m_userToClone.transform.parent;
        clone.userId = userId;

        string postfix = getPostfix(userId);

        clone.gameObject.name = m_userToClone.name + postfix;

        var walkthruController = clone.gameObject.GetComponent<getRealWalkthruController>();
        if(walkthruController != null) {
            walkthruController.forwardAxis += postfix;
            walkthruController.strafeAxis += postfix;
            walkthruController.navSpeedButton += postfix;
            walkthruController.resetButton += postfix;
            walkthruController.jumpButton += postfix;
        }

        var joyLook = clone.gameObject.GetComponent<getRealJoyLook>();
        if(joyLook != null) {
            joyLook.yawAxis += postfix;
            joyLook.pitchAxis += postfix;
            joyLook.resetButton += postfix;
        }

        var wandLook = clone.gameObject.GetComponent<getRealWandLook>();
        if(wandLook != null) {
            wandLook.activationButton += postfix;
            wandLook.resetButton += postfix;
        }

        var aimAndGoController = clone.gameObject.GetComponent<getRealAimAndGoController>();
        if(aimAndGoController != null) {
            aimAndGoController.forwardAxis += postfix;
            aimAndGoController.strafeAxis += postfix;
            aimAndGoController.navSpeedButton += postfix;
            aimAndGoController.resetButton += postfix;
        }

        var wandDriveController = clone.gameObject.GetComponent<getRealWandDriveController>();
        if(wandDriveController != null) {
            wandDriveController.activationButton += postfix;
            wandDriveController.resetButton += postfix;
            wandDriveController.jumpButton += postfix;
        }

        var grabbingWand = clone.gameObject.GetComponentInChildren<GrabbingWand>(true);
        if(grabbingWand != null) {
            grabbingWand.button += postfix;
        }

        var wandManager = clone.gameObject.GetComponentInChildren<WandManager>(true);
        if(wandManager != null) {
            wandManager.changeWandButton += postfix;
        }

        var menuDrag = clone.gameObject.GetComponentInChildren<MenuDrag>(true);
        if(menuDrag != null) {
            menuDrag.m_wandButton += postfix;
        }

        var showMenu = clone.gameObject.GetComponentInChildren<ShowMenu>(true);
        if(showMenu != null) {
            showMenu.m_wandButton += postfix;
        }

        var shadowUI = clone.gameObject.GetComponentInChildren<ShadowUI>(true);
        if(shadowUI != null) {
            shadowUI.enabled = false;
        }

        var vRSettingsUI = clone.gameObject.GetComponentInChildren<getRealVRSettingsUI>(true);
        if(vRSettingsUI != null) {
            vRSettingsUI.enabled = false;
        }

        var cameraUpdater = clone.gameObject.GetComponentInChildren<getRealCameraUpdater>(true);
        if(cameraUpdater != null) {
            cameraUpdater.enabled = true;
        }

        string firstAvatarLayer = m_avatarLayerPrefix + "1";
        int firstAvatarLayerId = LayerMask.NameToLayer(firstAvatarLayer);

        string avatarLayer = m_avatarLayerPrefix + (userId + 1).ToString();
        int avatarLayerId = LayerMask.NameToLayer(avatarLayer);
        Camera [] cameras = clone.gameObject.GetComponentsInChildren<Camera>(true);
        foreach(var camera in cameras) {
            if(firstAvatarLayerId >= 0) {
                camera.cullingMask |= 1 << firstAvatarLayerId;
            }
            if(avatarLayerId >= 0) {
                camera.cullingMask &= ~(1 << avatarLayerId);
            }
        }

        if(firstAvatarLayerId >= 0 && avatarLayerId >= 0) {
            replaceLayer(clone.gameObject, firstAvatarLayerId, avatarLayerId);
        }

        getRealSensorUpdater[] sensorUpdaters = clone.gameObject.GetComponentsInChildren<getRealSensorUpdater>(true);

        foreach(getRealSensorUpdater sensorUpdater in sensorUpdaters)
        {
            sensorUpdater.sensorName += postfix;
        }
        
        return clone;
    }

    protected string getPostfix(uint userId)
    {
        return "_" + (userId + 1).ToString();
    }

    private void replaceLayer(GameObject go, int layerFrom, int layerTo)
    {
        if(go.layer == layerFrom) {
            go.layer = layerTo;
        }
        foreach(Transform child in go.transform) {
            replaceLayer(child.gameObject, layerFrom, layerTo);
        }
    }
}
