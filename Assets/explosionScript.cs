using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explosionScript : MonoBehaviour
{
    public ParticleSystem[] psList;

    void Start()
    {
        psList = GetComponentsInChildren<ParticleSystem>();
    }

    void Hit()
    {
        Death();
    }

    void Death()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        foreach (ParticleSystem ps in psList)
        {
            ps.Play();
        }
        Destroy(gameObject, 1f);
    }
}
