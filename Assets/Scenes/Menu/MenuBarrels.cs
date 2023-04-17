using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBarrels : MonoBehaviour
{
    Color color;
    public SpriteRenderer SpriteRenderer;
    ParticleSystem[] psList;
    ParticleSystem[] psList2;
    public GameObject obj;

    void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        color = SpriteRenderer.color;
        psList = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in psList)
        {
            Instantiate(ps, transform);
        }
        Destroy(obj);
        Invoke("PSList2", 0.1f);
    }

    void PSList2()
    {
        psList2 = GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (color.a < 1 && !psList2[0].isPlaying)
        {
            color.a = Mathf.Lerp(color.a, 1f, 0.1f);
        }
        if (color.a > 0.99)
            color.a = 1f;
        SpriteRenderer.color = color;
    }

    void OnMouseDown()
    {
        if (color.a > 0.8f)
        {
            color.a = 0f;
            SpriteRenderer.color = color;
            foreach (ParticleSystem ps in psList2)
            {
                ps.Play();
            }
        }
    }
}
