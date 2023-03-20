using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dasher : Enemy
{
    public float maxCD;
    public float currentCD;

    RaycastHit2D hit;

    // Start is called before the first frame update
    void Start()
    {
        SharedStart();
        prediction = 10;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Manager.instance.state != "Clearing" && hp > 0)
        {
            if (state == "Idle")
            {
                SharedIdle();
            }
            else
                target = AIManager.instance.GetPos(pos, out pos);
            SharedUpdate();
        }
    }

    public override void DisableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = false;
    }
}
