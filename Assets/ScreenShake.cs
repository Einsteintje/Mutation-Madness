using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake instance { get; private set; }
    Vector3 originalPos;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    void Start(){
        originalPos = Camera.main.transform.position;
    }

    void Update() { }

    public void Shake()
    {
        Camera.main.DOShakePosition(0.05f, 1, 20,50, true, ShakeRandomnessMode.Harmonic).OnComplete(() => transform.position = originalPos);
    }
}
