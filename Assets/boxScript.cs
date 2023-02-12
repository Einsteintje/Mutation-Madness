using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxScript : MonoBehaviour
{
    UnityEngine.Rendering.Universal.Light2D light2d;

    void Hit()
    {
        Death();
    }

    void Death()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<ParticleSystem>().Play();
        GetComponent<SpriteRenderer>().enabled = false;
        InvokeRepeating("Lights", 0.0f, 0.1f);
        light2d = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        Destroy(gameObject, 1f);
    }

    void Lights()
    {
        light2d.intensity = Mathf.Lerp(light2d.intensity, 0, 0.2f);
    }
}
