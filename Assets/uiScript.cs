using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class uiScript : MonoBehaviour
{
    TMP_Text[] tmpList;

    // Start is called before the first frame update
    void Start()
    {
        tmpList = GetComponentsInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager.instance.state == "Idle")
        {
            string time = Manager.instance.waveTimer.ToString("F1");
            tmpList[0].text = $"Time : {time}";
        }
        else
            tmpList[0].text = "Time : 0,0";

        tmpList[1].text = $"Wave : {Manager.instance.wave.ToString()}";
        tmpList[2].text = $"Score : {Player.instance.score}";
    }
}
