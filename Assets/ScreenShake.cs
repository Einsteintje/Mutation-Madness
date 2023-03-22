using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake instance { get; private set; }
    public Camera camera;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    void Update() { }

    public void Shake()
    {
        camera.DOShakePosition(0.1f, 10, 10, 50, true);
    }
}
