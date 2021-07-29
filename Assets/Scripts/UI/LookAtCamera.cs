using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(
            transform.position + mainCamera.transform.forward,
            mainCamera.transform.up);
    }
}
