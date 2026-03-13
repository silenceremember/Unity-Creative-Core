using System;
using UnityEngine;

/// <summary>
/// Пара ключ→объект для словаря activateObject в NarratorManager.
/// Назначается в Inspector — работает даже с изначально неактивными объектами.
/// </summary>
[Serializable]
public class SceneObjectEntry
{
    [Tooltip("Имя ключа — должно совпадать с DialogueLine.activateObject")]
    public string key;

    [Tooltip("Объект сцены, который будет активирован")]
    public GameObject gameObject;
}
