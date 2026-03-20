using System;
using UnityEngine;

/// <summary>
/// Key→object pair for the activateObject dictionary in NarratorManager.
/// Assigned in Inspector — works even with initially inactive objects.
/// </summary>
[Serializable]
public class SceneObjectEntry
{
    [Tooltip("Key name — must match DialogueLine.activateObject")]
    public string key;

    [Tooltip("Scene object to be activated")]
    public GameObject gameObject;
}
