using UnityEngine;
using UnityEditor;

/// <summary>
/// Python menu.
/// </summary>
public class PythonMenu : EditorWindow 
{

    private static string Title = "Window";

    private GUIStyle SysPathStyle;

    private static void CreateWindow()
    {
        EditorWindow.GetWindow(typeof(PythonMenu));
    }

    private static void CreateWindow(Rect rect)
    {
        EditorWindow.GetWindowWithRect(typeof(PythonMenu),rect);
    }

    [MenuItem("Python/New Python Script",false,0)]
    private static void AddComponent()
    {
		if(Selection.activeGameObject != null) {
            GameObject Selected = Selection.activeTransform.gameObject;
            Selected.AddComponent<PythonScript>();
		} else 
			EditorUtility.DisplayDialog("Missing GameObject", "Please, Select a GameObject", "OK");
    }

    [MenuItem("Python/External Library",false, 1)]
    private static void SetPythonLibs()
    {
        Title = "Locate Library";

        CreateWindow();

        if(PythonScript.SysPath.Count == 0)
            PythonScript.SysPath.Add("\\");
    }

    private void OnEnable()
    {
        titleContent.text = Title;
    }

    private void OnGUI()
    {
        SysPathStyle = new GUIStyle(GUI.skin.textField);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        int top = 5;
        for (int i = 0; i < PythonScript.SysPath.Count; i++) {

            GUI.Label(new Rect(5,top,Screen.width-180,20),PythonScript.SysPath[i], SysPathStyle);

            if(GUI.Button(new Rect(Screen.width-170,top,80,20), "Open")) {
                PythonScript.SysPath[i] = DialogLocation(PythonScript.SysPath[i]);

                string paths = string.Join("\n", PythonScript.SysPath.ToArray());

                EditorPrefs.SetString("SysPath",paths);

            }
            if(GUI.Button(new Rect(Screen.width-85,top,80,20), "Delete")) 
                PythonScript.SysPath.RemoveAt(i);

            top += 25;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        if(GUI.Button(new Rect(0,Screen.height-45,Screen.width,20), "Add Path")) {
            PythonScript.SysPath.Add("\\");
        }   
    }

    /// <summary>
    /// Dialogs the location.
    /// </summary>
    private string DialogLocation(string DefaultPath)
    {
        return EditorUtility.OpenFolderPanel("Python Library Path", DefaultPath,"");
    }

}
