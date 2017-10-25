using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace getReal3D
{
	public class SceneChecker
		: EditorWindow
	{
		static bool needsUpdate = true;
		static bool foundCameraUpdater = false;
		static bool foundNavigationScript = false;

        static bool s_hideDisabledWarnings = true;
        static List<ErrorInfo> errors = new List<ErrorInfo>();
        static Regex filenameFilter = new Regex(@".*\.(cs|js)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        Vector2 m_scrollPosition;

        public class ErrorInfo {
            public int lineNum;
            public string line;
            public string path;
            public string filename;

            public ErrorInfo(string path, string filename, int lineNum, string line)
            {
                this.path = path;
                this.filename = filename;
                this.lineNum = lineNum;
                this.line = line;
            }
        }

        static bool foundEventSystem = false;
        static bool foundWandEventModule = false;
        static bool foundVRMenu = false;
        static bool eventCameraMissing = false;
        static bool eventCameraNotOnWand = false;

        static bool ArrayEmpty(object[] array)
		{
			return array == null || array.Length == 0;
		}
	
		[MenuItem("getReal3D/Scene Checker", false, 50)]
		static void CreateChecker()
		{
			UpdateSceneStatus();
			EditorWindow.GetWindow(typeof(SceneChecker), false, "Scene Checker");
		}

        static bool isInScene(MonoBehaviour monoBehaviour)
        {
            if(monoBehaviour.hideFlags == HideFlags.NotEditable || monoBehaviour.hideFlags == HideFlags.HideAndDontSave)
                return false;
            else if(!EditorUtility.IsPersistent(monoBehaviour.transform.root.gameObject))
                return false;
            else
                return true;
        }

        static T[] FindObjectsOfTypeInScene<T>() where T: Behaviour
        {
            List<T> res = new List<T>();
            T[] allComponents = Resources.FindObjectsOfTypeAll<T>();
            foreach(var monoBehaviour in allComponents) {
                if(monoBehaviour.hideFlags != HideFlags.None) {
                    continue;
                }
                res.Add(monoBehaviour);
            }
            return res.ToArray();
        }

        static void UpdateSceneStatus()
		{
			getRealCameraUpdater[] cu = Resources.FindObjectsOfTypeAll(typeof(getRealCameraUpdater)) as getRealCameraUpdater[];
			getRealAimAndGoController[] ag = Resources.FindObjectsOfTypeAll(typeof(getRealAimAndGoController)) as getRealAimAndGoController[];
			getRealWalkthruController[] wt = Resources.FindObjectsOfTypeAll(typeof(getRealWalkthruController)) as getRealWalkthruController[];
			getRealWandLook[] wl = Resources.FindObjectsOfTypeAll(typeof(getRealWandLook)) as getRealWandLook[];
			getRealJoyLook[] jl = Resources.FindObjectsOfTypeAll(typeof(getRealJoyLook)) as getRealJoyLook[];
			
			
			foundCameraUpdater = !ArrayEmpty(cu);
			foundNavigationScript = !(ArrayEmpty(ag) && ArrayEmpty(wt) && ArrayEmpty(wl) && ArrayEmpty(jl));

            errors = new List<ErrorInfo>();

            string[] sourceFiles = AssetDatabase.FindAssets("t:Script");
            for(int i = 0; i < sourceFiles.Length; ++i) {
                string path = AssetDatabase.GUIDToAssetPath(sourceFiles[i]);
                string filename = Path.GetFileNameWithoutExtension(path);
                EditorUtility.DisplayProgressBar("Checking scripts...", path,
                                                 i / (float) sourceFiles.Length);
                if(filenameFilter.IsMatch(path)) {
                    try {
                        StreamReader file = new StreamReader(path);
                        string line;
                        int lineNum = 1;
                        while((line = file.ReadLine()) != null) {
                            if(!checkLine(line)) {
                                errors.Add(new ErrorInfo(path, filename, lineNum, line.Trim()));
                            }
                            ++lineNum;
                        }
                        file.Close();
                    }
                    catch(Exception e) {
                        Debug.LogWarning("Failed to parse script " + path + ": " + e.ToString());
                    }
                }
                // UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(@"C:\1.txt", 1);
            }
            EditorUtility.ClearProgressBar();

            foundEventSystem = false;
            foundWandEventModule = false;
            foundVRMenu = false;
            eventCameraMissing = false;
            eventCameraNotOnWand = false;

            UnityEngine.EventSystems.EventSystem[] eventSystems = FindObjectsOfTypeInScene<UnityEngine.EventSystems.EventSystem>();
            foundEventSystem = eventSystems.Length != 0;
            foreach(var eventSystem in eventSystems) {
                WandEventModule wandEventModule = eventSystem.GetComponent<WandEventModule>();
                foundWandEventModule |= wandEventModule != null;
            }

            UnityEngine.Canvas[] canvases = FindObjectsOfTypeInScene<UnityEngine.Canvas>();
            foreach(var canvas in canvases) {
                if(canvas.renderMode == RenderMode.WorldSpace){
                    foundVRMenu = true;
                    Camera eventCamera = canvas.worldCamera;
                    if(eventCamera) {
                        eventCameraNotOnWand |= eventCamera.GetComponentInParent<getRealWandUpdater>() != null;
                    }
                    else {
                        eventCameraMissing = true;
                    }
                }
            }

            needsUpdate = false;
		}

        static private bool checkLine(string str)
        {
            bool betweenQuotes = false;
            bool betweenComments = false;
            char lastChar = '\0';
            int len = str.Length;
            for(int i = 0; i < len; ) {
                char c = str[i];
                bool lastCharWasIdentifierChar =
                    (lastChar >= 'a' && lastChar <= 'z') ||
                    (lastChar >= 'A' && lastChar <= 'Z') ||
                    (lastChar >= '0' && lastChar <= '9') ||
                    (lastChar == '_');
                if(lastChar == '/' && c == '/') {
                    return true;
                }
                else if(lastChar != '\\' && c == '"') {
                    betweenQuotes = !betweenQuotes;
                }
                else if(lastChar != '/' && c == '*') {
                    betweenComments = true;
                }
                else if(lastChar != '*' && c == '/') {
                    betweenComments = true;
                }
                else if(!betweenComments && !betweenQuotes) {
                    if(0 == string.Compare(str, i, "UnityEngine.Input.", 0, 18, StringComparison.Ordinal)) {
                        return false;
                    }
                    else if(0 == string.Compare(str, i, "getReal3D.Input.", 0, 16, StringComparison.Ordinal)) {
                        lastChar = '.';
                        i += 16;
                        continue;
                    }
                    else if(!lastCharWasIdentifierChar && 0 == string.Compare(str, i, "Input.", 0, 6, StringComparison.Ordinal)) {
                        return false;
                    }
                }
                lastChar = c;
                ++i;
            }
            return true;
        }

        void OnGUI()
		{
			if (needsUpdate) UpdateSceneStatus();
			if (!foundCameraUpdater)
				EditorGUILayout.HelpBox("No getRealCameraUpdater script found. You probably want a getRealCameraUpdater attached to a GameObject with a Camera.", MessageType.Warning, true);
			else
				EditorGUILayout.HelpBox("Found getRealCameraUpdater.", MessageType.Info, true);
	
			if (!foundNavigationScript)
				EditorGUILayout.HelpBox("No getReal3D navigation scripts found. You probably want a navigation script (getRealAimAndGoController, getRealWalkthruController, getRealWandLook, getRealJoyLook) attached to a GameObject.", MessageType.None, true);
			else
				EditorGUILayout.HelpBox("Found getReal3D navigation scripts.", MessageType.Info, true);

            if(foundVRMenu) {
                if(!foundEventSystem) {
                    EditorGUILayout.HelpBox("EventSystem script not found. VR menu won't receive any input.", MessageType.Warning, true);
                }
                else if(!foundWandEventModule) {
                    EditorGUILayout.HelpBox("EventSystem found, but no WandEventModule associated. VR menu won't receive any input.", MessageType.Warning, true);
                }
                else if(eventCameraMissing) {
                    EditorGUILayout.HelpBox("World space rendering canvas found, but no camera is associated to it. VR menu won't receive any input.", MessageType.Warning, true);
                }
                else if(!eventCameraNotOnWand) {
                    EditorGUILayout.HelpBox("World space rendering canvas found, but attached camera is not a child of the wand. VR menu won't receive any input.", MessageType.Warning, true);
                }
            }

            EditorGUILayout.HelpBox("Prefer getReal3D.Input over UnityEngine.Input. or Input.", errors.Count != 0 ? MessageType.Warning : MessageType.Info, true);

            if(errors.Count != 0) {
                bool hideDisabledWarnings = GUILayout.Toggle(s_hideDisabledWarnings, "Don't show warnings for lines ending by '// no gr3d warning'.");
                if(hideDisabledWarnings != s_hideDisabledWarnings) {
                    s_hideDisabledWarnings = hideDisabledWarnings;
                }

                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                foreach(var errorInfo in errors) {
                    if(!s_hideDisabledWarnings || !errorInfo.line.EndsWith("// no gr3d warning")) {
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button("Select", GUILayout.Width(50))) {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(errorInfo.path);
                        }
                        if(GUILayout.Button("Open", GUILayout.Width(50))) {
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(errorInfo.path, errorInfo.lineNum);
                        }
                        GUILayout.Label(errorInfo.filename + ":" + errorInfo.lineNum + " " + errorInfo.line);
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Update"))
			{
				UpdateSceneStatus();
			}
		}
	}
}
