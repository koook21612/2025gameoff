using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 从队列随机抽取
/// </summary>
public static class ListExtensions
{ 
    public static T Draw<T>(this List<T> list)
    {
        if(list.Count == 0)
        {
            return default;
        }
        int r = Random.Range(0, list.Count);
        T t = list[r];
        list.Remove(t);
        return t;
    }
}

