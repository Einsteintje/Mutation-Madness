using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxScript : MonoBehaviour
{
    UnityEngine.Rendering.Universal.Light2D light2d;

    [SerializeField]
    ParticleSystem ps;

    void Hit()
    {
        Death();
    }

    void Death()
    {
        if (GetComponent<BoxCollider2D>().enabled)
        {
            ps = Instantiate(ps, Manager.instance.transform);
            ps.transform.position = transform.position;
            ps.Play();
        }
        AudioManager.instance.breakSound.Play();
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        InvokeRepeating("Lights", 0.0f, 0.02f);
        light2d = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        light2d.intensity = 5.0f;
        Destroy(gameObject, 2f);
    }

    void Lights()
    {
        light2d.intensity = Mathf.Lerp(light2d.intensity, 0, 0.1f);
    }
}
