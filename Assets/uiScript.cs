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
            tmpList[0].text = Manager.instance.waveTimer.ToString("F1");
        else
            tmpList[0].text = "0,0";

        tmpList[1].text = Manager.instance.currentWave.ToString();
    }
}
