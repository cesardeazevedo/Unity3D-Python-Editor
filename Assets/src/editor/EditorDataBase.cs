using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Keep buffers in memory
/// </summary>
[Serializable]
public class EditorDataBase : ScriptableObject
{
    /// <summary>
    /// InstanceID and the instance of buffer
    /// </summary>
    [SerializeField]
    public Dictionary<int, Code> Buffers = new Dictionary<int, Code>();

    /// <summary>
    /// The _instance.
    /// </summary>
    private static EditorDataBase _instance;

    /// <summary>
    /// Singleton of instance
    /// </summary>
    /// <value>The instance.</value>
    public static EditorDataBase Instance {
        get {
            if(_instance == null) {

                //Create instance of ScriptableObject.
                _instance = ScriptableObject.CreateInstance<EditorDataBase>();

                //Remove existing instances
                _instance.RemoveAllInstances();

                //Leaks
                _instance.hideFlags = HideFlags.HideAndDontSave;
            }

            return _instance;
        }
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    public void OnEnable()
    {
        //Leaks
        hideFlags = HideFlags.HideAndDontSave;

        if (_instance == null)
            _instance = this;

    }

    /// <summary>
    /// Adds the instance.
    /// </summary>
    /// <param name="InstanceID">Instance I.</param>
    /// <param name="e">E.</param>
    public void AddInstance(int InstanceID, Code e)
    {
        if(!Instance.Buffers.ContainsKey(InstanceID))
            Instance.Buffers.Add(InstanceID,e); 
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <returns>The instance.</returns>
    /// <param name="InstanceID">Instance I.</param>
    public Code GetInstance(int InstanceID)
    {
        if(Instance.Buffers.ContainsKey(InstanceID))
            return Instance.Buffers[InstanceID];        

        //return a default buffer
        return CreateInstance<Code>().Initialize();
    }

    /// <summary>
    /// Removes the instance.
    /// </summary>
    /// <param name="InstanceID">Instance I.</param>
    public void RemoveInstance(int InstanceID)
    {
        Instance.Buffers.Remove(InstanceID);
    }

    /// <summary>
    /// Removes the instances.
    /// </summary>
    private void RemoveAllInstances()
    {
        Instance.Buffers.Clear();
    }
}
