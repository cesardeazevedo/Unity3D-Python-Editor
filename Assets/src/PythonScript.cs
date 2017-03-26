using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PythonScript : MonoBehaviour
{
   /// <summary>
   /// The path of file
   /// </summary>  
   public string FilePath;

   /// <summary>
   /// The name of the file.
   /// </summary>
   public string FileName;

   /// <summary>
   /// The file created.
   /// </summary>
   public bool FileCreated;

   /// <summary>
   /// The lib path.
   /// </summary>
   public static List<string> SysPath = new List<string>();

   /// <summary>
   /// The default code.
   /// </summary>
   public string DefaultCode = "import UnityEngine as unity\n\n" +
                                      "class Untitled(): \n\n" +
                                      "\tdef Start(self, this):\n" +
                                      "\t\tpass\n\n" +
                                      "\tdef Update(self, this):\n" +
                                      "\t\tpass";

   private Interpreter interpreter = new Interpreter();
   private object classReference;

   void Awake()
   {
      if(string.IsNullOrEmpty(FilePath))
         return;
         
      interpreter.Compile(FilePath, Microsoft.Scripting.SourceCodeKind.Statements);
      classReference = interpreter.GetVariable(Path.GetFileNameWithoutExtension(FilePath));

      InvokeMethod("Awake");
   }

   void Start()
   {
      InvokeMethod("Start");
   }

   void LateUpdate()
   {
      InvokeMethod("LateUpdate");
   }

   void Update()
   {
      InvokeMethod("Update");
   }

   void OnEnable()
   {
      InvokeMethod("OnEnable");
   }

   void OnDisable()
   {
      InvokeMethod("OnDisable");
   }

   void OnDestroy()
   {
      InvokeMethod("OnDestroy");
   }

   void InvokeMethod(string Method)
   {
      interpreter.InvokeMethod(classReference, Method, this);
   }

   /// <summary>
   /// Reset this instance.
   /// </summary>
   public void Reset()
   {
      FilePath = string.Empty;
      FileCreated = false;
      FileName = string.Empty;
   }

   //TODO:
   [ContextMenu("Cursor Block")]
   private void Menu()
   {
      Debug.Log("CursorBlock");
   }
   //TODO:
   [ContextMenu("Vertical Bar")]
   private void Menu2()
   {
      Debug.Log("CursorBlock");
   }
}
