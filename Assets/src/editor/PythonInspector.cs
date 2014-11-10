using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using System.Collections;

/// <summary>
/// Call python.
/// </summary>
[Serializable, CustomEditor(typeof(PythonBase))]
public class PythonInspector : Editor
{

    [SerializeField]
    private PythonBase Target;

    private Interpreter python;

    public EditorView editor;

    private GUIStyle FontDrag, ButtonTabs;

    private int ColorSkinPro = 0;

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        Target = (PythonBase)target;

        if(editor == null)
            editor = new EditorView();

        editor.OnEnable(Target);

        //Repaint Action Delegate
        editor.RepaintAction += this.Repaint;

        //Set view back 
        SwitchView(Target.CurrentView);

        LoadEditorPrefs();

        ColorSkinPro = !EditorGUIUtility.isProSkin ? 0 : 255;

    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    private void OnDisable()
    {
        //Disable delegate
        editor.RepaintAction -= this.Repaint;
        //Ask for Save file
        DialogFileSystem();
    }

    /// <summary>
    /// Sets the styles on inspector (Tabs, Font)
    /// </summary>
    private void SetStyles()
    {
        //Style of text
        FontDrag = new GUIStyle(GUI.skin.box);
        FontDrag.fontSize  = 16;
        FontDrag.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        FontDrag.alignment = TextAnchor.MiddleCenter;
        FontDrag.hover.background = TextureColor(Color.yellow); 
        //Style of Tabs
        ButtonTabs = new GUIStyle(GUI.skin.box);
        ButtonTabs.fontSize = 16;
        ButtonTabs.normal.textColor = Color.white;
        ButtonTabs.alignment = TextAnchor.MiddleCenter;

        Color ColorTabs = new Color(ColorSkinPro,ColorSkinPro,ColorSkinPro,0.9f);
        ButtonTabs.normal.background = TextureColor(ColorTabs);
    }

    /// <summary>
    /// Dialogs the file system.
    /// </summary>
    private void DialogFileSystem()
    {
        if(!Target.Saved && Target.HasChanges) {

            switch(DialogSave()) {
                //Save
                case 0:
                    if(!Target.FileCreated)
                        SaveFileLocation();
                    else {
                        SaveCodeToFile();
                        Target.InMemory = false;
                    }
                    break;
                    //Cancel
                case 1:
                    if(Target.FileCreated) {
                        Target.Saved = true;
                        Target.HasChanges = false;
                    }
                    break;
                    //Keep in Memory
                case 2:
                    EditorDataBase.Instance.AddInstance(Target.GetInstanceID(), editor.Buffer);
                    Target.InMemory = true;
                    break;
            }
        }
    }
    /// <summary>
    /// Saves the file system.
    /// </summary>
    /// <returns>The file system.</returns>
    private void SaveFileLocation()
    {
        string Path = EditorUtility.SaveFilePanel("Save Python Script","Assets",this.name,"py");

        CreateFile(Path);
    }

    /// <summary>
    /// Dialog this instance.
    /// </summary>
    private int DialogSave()
    {
        return EditorUtility.DisplayDialogComplex("Save Python File ", "Save File?", "Save", "Cancel", "Keep in Memory");
    }

    /// <summary>
    /// Creates the file.
    /// </summary>
    /// <param name="FilePath">File path.</param>
    /// <param name="FileName">File name.</param>
    private void CreateFile(string FilePath)
    {
        if(FilePath != string.Empty) {
            File.WriteAllText(FilePath,editor.Buffer.CodeBuffer);
            Target.FilePath = FilePath;
            Target.FileName = Path.GetFileName(FilePath);
            Target.FileCreated = true;
            Target.Saved = true;
            Target.HasChanges = false;
        }
    }

    /// <summary>
    /// Saves the code to file.
    /// </summary>
    private void SaveCodeToFile()
    {
        File.WriteAllText(Target.FilePath,editor.Buffer.CodeBuffer, Encoding.UTF8);
        Target.Saved      = true;
        Target.InMemory   = false;
        Target.HasChanges = false;
        //Remove from memory
        EditorDataBase.Instance.RemoveInstance(Target.GetInstanceID());
    }

    /// <summary>
    // "Compile" on save file
    /// </summary>
    private void CompileOnSave()
    {
        python = new Interpreter();
        string Response = python.Compile(Target.FilePath, Microsoft.Scripting.SourceCodeKind.Statements);

        //Display if returned something, error, print, etc.
        if(!String.IsNullOrEmpty(Response))
            Debug.Log(Response);
    }

    /// <summary>
    /// Show a box dialog
    /// </summary>
    /// <param name="error">String of error</param>
    public static void DialogError(string error)
    {
        EditorUtility.DisplayDialog("Error Occurred",error,"OK");
    }

    /// <summary>
    /// Loads the editor prefs.
    /// </summary>
    private void LoadEditorPrefs()
    {

        string Paths = EditorPrefs.HasKey("SysPath") ?
            EditorPrefs.GetString("SysPath") : "\\";

        PythonBase.SysPath = Paths.Split('\n').ToList();
    }

    /// <summary>
    /// Raises the inspector GU event.
    /// </summary>
    public override void OnInspectorGUI()
    {

        SetStyles();

        DoSpace();

        Rect DropArea = GUILayoutUtility.GetRect(Screen.width,50,GUILayout.ExpandWidth(true));

        GUI.Box(DropArea,"Drag Python Script", FontDrag);

        DragAndDropFile(DropArea);

        DoSpace();

        ToogleButtons();

        editor.EditorViewGUI(Target.CurrentView == PythonBase.Views.Interpreter);

        if(GUILayout.Button("Save and Compile")) {

            Target.Saved = true;
            Target.HasChanges = false;

            if(!Target.FileCreated)
                SaveFileLocation();
            else
                SaveCodeToFile();

            CompileOnSave();
        }
    }

    /// <summary>
    /// Drags the and drop file.
    /// </summary>
    /// <param name="DropArea">Drop area.</param>
    private void DragAndDropFile(Rect DropArea)
    {
        Event current = Event.current;

        switch(current.type) {

            case EventType.DragUpdated:
            case EventType.DragPerform:

                if(DropArea.Contains(current.mousePosition)) {
                    DragAndDrop.visualMode = DragAndDrop.paths.Length == 0   ? DragAndDropVisualMode.Rejected 
                        :   DragAndDrop.paths[0].EndsWith(".py")             ? DragAndDropVisualMode.Copy
                        :   DragAndDrop.paths[0].EndsWith(".txt")            ? DragAndDropVisualMode.Copy
                        :   DragAndDropVisualMode.Rejected;

                    if(current.type == EventType.DragPerform) {

                        Target.FilePath = DragAndDrop.paths[0];

                        Target.FileName = Path.GetFileName(Target.FilePath);

                        StreamReader file = new StreamReader(Target.FilePath,Encoding.UTF8);

                        editor.Buffer.Initialize();

                        string LocalBuffer = file.ReadToEnd();
                        editor.Buffer.CodeBuffer = LocalBuffer == "" ? " " : LocalBuffer;

                        Target.FileCreated = true;

                        SwitchView(PythonBase.Views.Code);

                        DragAndDrop.AcceptDrag();

                        current.Use();
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Toogles the tabs
    /// </summary>
    private void ToogleButtons()
    {
        GUILayout.BeginHorizontal();

        bool Active = Target.CurrentView == PythonBase.Views.Code;

        SwitchColors(Target.CurrentView == PythonBase.Views.Code);

        //Check if file has changes and put "*"
        string FileName = Target.FileName + ((Target.HasChanges) ? "*" : "");

        //Limit the width of Tab
        int width = Math.Min(Screen.width-200, 100 + FileName.Length*9);
        if(GUILayout.Toggle(Active, FileName, ButtonTabs, GUILayout.Width(width),
                    GUILayout.Height(40)) != Active) {

            SwitchView(PythonBase.Views.Code);
        }

        SwitchColors(Target.CurrentView == PythonBase.Views.Interpreter);
        if(GUILayout.Toggle(Active, "Interpreter", ButtonTabs, GUILayout.Width(170),
                    GUILayout.Height(40)) != Active) {

            SwitchView(PythonBase.Views.Interpreter);
            editor.Buffer.CurrentLine = string.Empty;
            editor.InitializeInterpreter();
        }

        GUILayout.EndHorizontal();
    }

    private void SwitchView(PythonBase.Views view)
    {
        editor.Buffer.InterpreterView = editor.InterpreterView
                                      = (view == PythonBase.Views.Interpreter);
        Target.CurrentView = view;
    }

    private void SwitchColors(bool view)
    {
        Color ActiveColor = view ? new Color(ColorSkinPro,ColorSkinPro,ColorSkinPro,0.35f) :
                                   new Color(ColorSkinPro,ColorSkinPro,ColorSkinPro,0.50f);

        ButtonTabs.normal.background = TextureColor(ActiveColor);
    }

    /// <summary>
    /// Spacing of InspectorGUI
    /// </summary>
    private void DoSpace()
    {
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Applies a color to a texture.
    /// </summary>
    /// <returns>The color.</returns>
    /// <param name="color">Color.</param>
    private static Texture2D TextureColor(Color color)
    {
        Texture2D TextureColor = new Texture2D(1, 1);
        TextureColor.SetPixels(new Color[] { color });
        TextureColor.Apply();
        TextureColor.hideFlags = HideFlags.HideAndDontSave;
        return TextureColor;
    }

}
