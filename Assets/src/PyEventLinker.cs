using System.IO;
using UnityEngine;

public class PyEventLinker : MonoBehaviour
{
    private PythonBase py;

    private Interpreter machine = new Interpreter();

    private object ClassReference;

    private void Awake(){
        py = GetComponent<PythonBase>();
        machine.Compile(py.FilePath, Microsoft.Scripting.SourceCodeKind.Statements);
        ClassReference = machine.GetVariable(Path.GetFileNameWithoutExtension(py.FilePath));

        InvokeMethod("Awake");
    }

    private void Start()
    {
        InvokeMethod("Start");
    }

    private void Update()
    {
        InvokeMethod("Update");
    }
    private void InvokeMethod(string Method)
    {
        machine.InvokeMethod(ClassReference, Method,this);
    }
}
