using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;

/// <summary>
/// Python Script Inspector
/// </summary>
[Serializable, CustomEditor(typeof(PythonScript))]
public class PythonInspector : Editor
{
   PythonScript _target;

   [SerializeField]
   private PythonScript Target
   {
      get
      {
         if (_target == null)
         {
            _target = (PythonScript)target;
         }
         return _target;
      }
   }

   private Interpreter python;
   private GUIStyle fontDrag;
   private Rect dropArea;

   /// <summary>
   /// Sets the styles of the inspector
   /// </summary>
   private void SetStyles()
   {
      //Style of text
      fontDrag = new GUIStyle(GUI.skin.box);
      fontDrag.fontSize = 16;
      fontDrag.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
      fontDrag.alignment = TextAnchor.MiddleCenter;
      fontDrag.hover.background = TextureColor(Color.yellow);
   }

   /// <summary>
   /// Creates a new python file.
   /// </summary>
   private void CreateFile()
   {
      string filepath = EditorUtility.SaveFilePanel("Create Python Script", "Assets", this.name, "py");

      if (filepath != string.Empty)
      {
         File.WriteAllText(filepath, Target.DefaultCode);
         Target.FilePath = filepath;
         Target.FileName = Path.GetFileName(filepath);
         Target.FileCreated = true;
      }
   }

   /// <summary>
   /// Show a box dialog
   /// </summary>
   /// <param name="error">String of error</param>
   public static void DialogError(string error)
   {
      EditorUtility.DisplayDialog("Error Occurred", error, "OK");
   }

   /// <summary>
   /// Loads the editor prefs.
   /// </summary>
   private void LoadEditorPrefs()
   {

      string Paths = EditorPrefs.HasKey("SysPath") ?
          EditorPrefs.GetString("SysPath") : "\\";

      PythonScript.SysPath = Paths.Split('\n').ToList();
   }

   /// <summary>
   /// Raises the inspector GUI event.
   /// </summary>
   public override void OnInspectorGUI()
   {
      SetStyles();

      EditorGUILayout.Space();

      dropArea = GUILayoutUtility.GetRect(Screen.width, 35, GUILayout.ExpandWidth(true));

      if (string.IsNullOrEmpty(Target.FileName))
         GUI.Box(dropArea, "Drag Python Script", fontDrag);
      else
         GUI.Box(dropArea, Target.FileName, fontDrag);

      DragAndDropFile(dropArea);

      EditorGUILayout.Space();

      FileEditButtons();
   }

   /// <summary>
   /// Compiles the current python script. Or creates a new one if it does not exist.
   /// </summary>
   public void Compile()
   {
      if (!Target.FileCreated)
         CreateFile();

      python = new Interpreter();
      string Response = python.Compile(Target.FilePath, Microsoft.Scripting.SourceCodeKind.Statements);

      //Display if returned something, error, print, etc.
      if (!String.IsNullOrEmpty(Response))
         Debug.Log(Response);
   }

   /// <summary>
   /// Drags and drop the file.
   /// </summary>
   /// <param name="DropArea">Drop area.</param>
   private void DragAndDropFile(Rect DropArea)
   {
      Event current = Event.current;

      switch (current.type)
      {
         case EventType.DragUpdated:
         case EventType.DragPerform:

            if (DropArea.Contains(current.mousePosition))
            {
               DragAndDrop.visualMode = DragAndDrop.paths.Length == 0 ? DragAndDropVisualMode.Rejected
                   : DragAndDrop.paths[0].EndsWith(".py") ? DragAndDropVisualMode.Copy
                   : DragAndDrop.paths[0].EndsWith(".txt") ? DragAndDropVisualMode.Copy
                   : DragAndDropVisualMode.Rejected;

               if (current.type == EventType.DragPerform)
               {

                  Target.FilePath = DragAndDrop.paths[0];
                  Target.FileName = Path.GetFileName(Target.FilePath);

                  Target.FileCreated = true;

                  DragAndDrop.AcceptDrag();

                  current.Use();
               }
            }
            break;
      }
   }

   /// <summary>
   /// File edit buttons
   /// </summary>
   private void FileEditButtons()
   {
      GUILayout.BeginVertical();

      if (GUILayout.Button("Compile"))
      {
         Compile();
      }

      if (GUILayout.Button("Edit"))
      {
         if (string.IsNullOrEmpty(Target.FilePath))
            CreateFile();
         else
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(Target.FilePath, 1);
      }

      GUILayout.EndVertical();
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