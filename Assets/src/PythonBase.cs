using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Model
/// </summary>
[System.Serializable]
public class PythonBase : MonoBehaviour
{

	/// <summary>
	/// The path of file
	/// </summary>	
	public string FilePath;
	
	public string FileName = "Untitled.py";

	/// <summary>
	/// The file created.
	/// </summary>
	public bool FileCreated, Saved, InMemory = false;
	public bool HasChanges = true;
	
	public enum Views {
		Code,
		Interpreter
	};
	
	/// <summary>
	/// The current view.
	/// </summary>
	public Views CurrentView;
	
	/// <summary>
	/// The lib path.
	/// </summary>
	public static List<string> SysPath = new List<string>();
	
	/// <summary>
	/// The default code.
	/// </summary>
	public static string DefaultCode = "import UnityEngine as unity\n\ndef Start():\n\tpass\n\ndef Update():\n\tpass";
	
		
	/// <summary>
	/// Reset this instance.
	/// </summary>
	public void Reset()
	{
		FilePath = string.Empty;
		FileCreated = false;
		Saved = false;
		HasChanges = true;
		FileName = "Untitled.py";
		
	}
	
	void Update() 
	{
		Debug.Log("Update");
	}
					
	#region Context Options TODO
	
	[ContextMenu("Cursor Block")]
	private void Menu()
	{
		Debug.Log("CursorBlock");
	}
	
	[ContextMenu("Vertical Bar")]
	private void Menu2()
	{
		Debug.Log("CursorBlock");
	}
	
	#endregion 
	
}