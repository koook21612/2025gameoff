using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InteracteSO_", menuName = "Scriptable Objects/可交互物品")]
public class Item : ScriptableObject
{
    public Vector3 position;
    public Quaternion rotation;
    public string Function;

}
