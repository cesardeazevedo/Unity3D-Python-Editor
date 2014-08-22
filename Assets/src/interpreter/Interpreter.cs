using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Collections;
using IronPython.Hosting;
using IronPython.Modules;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;

/// <summary>
/// Interpreter for IronPython.
/// </summary>
public class Interpreter 
{
	/// <summary>
	/// The scope.
	/// </summary>
	private ScriptScope  Scope;
	
	/// <summary>
	/// The engine.
	/// </summary>
	private ScriptEngine Engine;
	
	/// <summary>
	/// The source.
	/// </summary>
	private ScriptSource Source;
	
	/// <summary>
	/// The compiled.
	/// </summary>
	private CompiledCode Compiled;
	
	/// <summary>
	/// The operation.
	/// </summary>
	private ObjectOperations Operation;
		
	/// <summary>
	/// The python class.
	/// </summary>
	private object PythonClass;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Interpreter"/> class.
	/// </summary>
	public Interpreter()
	{
		Engine = Python.CreateEngine();  
		Scope  = Engine.CreateScope();
		SetLibrary();	
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Interpreter"/> class.
	/// </summary>
	/// <param name="source">Source.</param>
	public Interpreter(string src) : this() 
	{
		Compile(src);
	}

	/// <summary>
	/// Compile the specified src.
	/// </summary>
	/// <param name="src">Source.</param>
	public string Compile(string src)
	{
		if(src == string.Empty)
			return string.Empty;
		
		LoadRuntime();
		
		Source = Engine.CreateScriptSourceFromString(src, Microsoft.Scripting.SourceCodeKind.SingleStatement);  
		
		ErrorHandle errors = new ErrorHandle();
		
		MemoryStream stream = new MemoryStream();
		//Set IO Ouput of execution
		Engine.Runtime.IO.SetOutput(stream, new StreamWriter(stream));
		
		Compiled  = Source.Compile(errors);
		Operation = Engine.CreateOperations();
			
		try {
			Compiled.Execute(Scope);
			return FormatOutput(ReadFromStream(stream));
			
		} catch(Exception ex) {
			return Engine.GetService<ExceptionOperations>().FormatException(ex);
		}
	}
	
	/// <summary>
	/// Formats the output of execution
	/// </summary>
	/// <returns>The output.</returns>
	/// <param name="output">Output.</param>
	private string FormatOutput(string output)
	{
		return string.IsNullOrEmpty(output) ? string.Empty 
		:      string.Join("\n", output.Remove(output.Length-1)
                                               .Split('\n')
                                               .Reverse().ToArray());
	}
	
	/// <summary>
	/// Reads MemoryStream.
	/// </summary>
	/// <returns>The from stream.</returns>
	/// <param name="ms">Ms.</param>
	private string ReadFromStream(MemoryStream ms) {
		
		int length = (int)ms.Length;
		Byte[] bytes = new Byte[ms.Length];
		ms.Seek(0, SeekOrigin.Begin);
		ms.Read(bytes, 0, length);
		
		return Encoding.GetEncoding("utf-8").GetString(bytes, 0, length);
	}
	
	/// <summary>
	/// Set sys paths
	/// </summary>
	private void SetLibrary()
	{
		if(PythonBase.SysPath.Count > 0) {
		
			ICollection<string> SysPath = Engine.GetSearchPaths();
			
			foreach(string path in PythonBase.SysPath)
				SysPath.Add(path);
			
			Engine.SetSearchPaths(SysPath);
			
		}
	}
		
	/// <summary>
	/// Load runtime Assemblies of Unity3D
	/// </summary>
	private void LoadRuntime()
	{
		Engine.Runtime.LoadAssembly(typeof(GameObject).Assembly);  	
	}
	
	public void AddRuntime<T>()
	{
		Engine.Runtime.LoadAssembly(typeof(T).Assembly);
	}
	
	public void AddRuntime(Type type)
	{
		Engine.Runtime.LoadAssembly(type.Assembly);
	}
			
	/// <summary>
	/// Gets the variable.
	/// </summary>
	/// <returns>The variable.</returns>
	/// <param name="name">Name.</param>
	public object GetVariable(string name)
	{
		return Operation.Invoke(Scope.GetVariable(name));
	}
	
	/// <summary>
	/// Initializes the class.
	/// </summary>
	/// <returns>The class.</returns>
	/// <param name="nameClass">Name class.</param>
	public object InitializeClass(object nameClass)
	{
		return Operation.Invoke(nameClass);
	}
	
	/// <summary>
	/// Calls the method.
	/// </summary>
	/// <param name="name">Name.</param>
	public void InvokeMethod(object nameClass, string Method, params object[] parameters)
	{
		object Func = Operation.GetMember(nameClass, Method);
		Operation.Invoke(Func,parameters);
	}

}
