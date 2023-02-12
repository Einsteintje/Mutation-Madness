using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class uiScript : MonoBehaviour
{
    GameObject managerObject;
    managerScript manager;

    TMP_Text[] tmpList;

    // Start is called before the first frame update
    void Start()
    {
        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();

        tmpList = GetComponentsInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.state == "Idle")
            tmpList[0].text = manager.waveTimer.ToString("F1");
        else
            tmpList[0].text = "0,0";

        tmpList[1].text = manager.currentWave.ToString();
    }
}
