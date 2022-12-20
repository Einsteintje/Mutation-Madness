using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponScript : MonoBehaviour
{
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 spawnPos = transform.position + Quaternion.Euler(0,0,90) * transform.up * 5;
        GameObject spawned = Instantiate(bullet, spawnPos, transform.rotation);
    }
}
