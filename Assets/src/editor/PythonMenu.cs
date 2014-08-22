using UnityEngine;
using UnityEditor;
using System.Collections;

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
        throw new System.NotImplementedException();
    }

    [MenuItem("Python/External Library",false, 1)]
    private static void SetPythonLibs()
    {
        Title = "Locate Library";

        CreateWindow(new Rect(Screen.width, Screen.height/2,600,200));

        if(PythonBase.SysPath.Count == 0)
            PythonBase.SysPath.Add("\\");
    }

    [MenuItem("Python/About")]
    private static void About()
    {
        //TODO
        Title = "About";
        CreateWindow();
    }

    private void OnEnable()
    {
        title = Title;
    }

    private void OnGUI()
    {

        SysPathStyle = new GUIStyle(GUI.skin.textField);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        int top = 5;
        for (int i = 0; i < PythonBase.SysPath.Count; i++) {

            GUI.Label(new Rect(5,top,Screen.width-180,20),PythonBase.SysPath[i], SysPathStyle);

            if(GUI.Button(new Rect(Screen.width-170,top,80,20), "Open")) {
                PythonBase.SysPath[i] = DialogLocation(PythonBase.SysPath[i]);

                string paths = string.Join("\n", PythonBase.SysPath.ToArray());

                EditorPrefs.SetString("SysPath",paths);

            }
            if(GUI.Button(new Rect(Screen.width-85,top,80,20), "Delete")) 
                PythonBase.SysPath.RemoveAt(i);

            top += 25;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        if(GUI.Button(new Rect(0,Screen.height-45,Screen.width,20), "Add Path")) {
            PythonBase.SysPath.Add("\\");
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
