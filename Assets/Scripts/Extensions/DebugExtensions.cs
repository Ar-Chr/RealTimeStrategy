using System;
using System.Linq.Expressions;
using UnityEngine;

public static class DebugExtensions
{
    public static string WithClassName<T>(this T scriptInstance, string message)
    {
        return $"[{typeof(T).Name}] {message}";
    }
}
