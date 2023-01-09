using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxScript : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Hit()
    {
        Death();
    }

    void Death()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<ParticleSystem>().Play();
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject, 1f);
    }
}
