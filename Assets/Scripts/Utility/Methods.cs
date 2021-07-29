using System;
using System.Collections;
using UnityEngine;

public static class Methods
{
    public static void DelayAction(this MonoBehaviour bhvr, Action action, float delay)
    {
        bhvr.StartCoroutine(DelayActionCoroutine(action, delay));
    }

    private static IEnumerator DelayActionCoroutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}

public static class VectorExtensions
{
    #region Vector3

    public static Vector3 WithX(this Vector3 vector, float newX) => new Vector3(newX, vector.y, vector.z);
    public static Vector3 WithY(this Vector3 vector, float newY) => new Vector3(vector.x, newY, vector.z);
    public static Vector3 WithZ(this Vector3 vector, float newZ) => new Vector3(vector.x, vector.y, newZ);

    public static Vector3 WithXPlus(this Vector3 vector, float value) => new Vector3(vector.x + value, vector.y, vector.z);
    public static Vector3 WithYPlus(this Vector3 vector, float value) => new Vector3(vector.x, vector.y + value, vector.z);
    public static Vector3 WithZPlus(this Vector3 vector, float value) => new Vector3(vector.x, vector.y, vector.z + value);

    public static Vector3 WithXMultBy(this Vector3 vector, float multiplier) => new Vector3(vector.x * multiplier, vector.y, vector.z);
    public static Vector3 WithYMultBy(this Vector3 vector, float multiplier) => new Vector3(vector.x, vector.y * multiplier, vector.z);
    public static Vector3 WithZMultBy(this Vector3 vector, float multiplier) => new Vector3(vector.x, vector.y, vector.z * multiplier);

    #endregion


    #region Vector2

    public static Vector2 WithX(this Vector2 vector, float newX) => new Vector2(newX, vector.y);
    public static Vector2 WithY(this Vector2 vector, float newY) => new Vector2(vector.x, newY);

    public static Vector2 WithXMultBy(this Vector2 vector, float multiplier) => new Vector2(vector.x * multiplier, vector.y);
    public static Vector2 WithYMultBy(this Vector2 vector, float multiplier) => new Vector2(vector.x, vector.y * multiplier);

    #endregion
}
