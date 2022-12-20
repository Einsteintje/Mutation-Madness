using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        transform.up = Quaternion.Euler(0,0,90) * transform.up;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.Log(transform.rotation);
        transform.position += transform.up;
    }
     void OnBecameInvisible(){
        Destroy(gameObject);
     }
}
