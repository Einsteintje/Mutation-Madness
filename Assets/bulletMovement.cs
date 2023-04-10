using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{
    public float moveSpeed;
    public float size;
    public Color color;
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        ParticleSystem.MainModule main = ps.main;
        main.startColor = color;
        GetComponent<SpriteRenderer>().color = color;
        //GetComponent<TrailRenderer>().startColor = color;
        //GetComponent<TrailRenderer>().endColor = color;

        transform.up = Quaternion.Euler(0, 0, -90) * transform.up;
        transform.localScale = new Vector3(size, size, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.up * Time.fixedDeltaTime * moveSpeed;
        if (Manager.instance.state == "Clearing" && ps != null && moveSpeed != 0)
            Death();
        if (
            transform.position.y < -Manager.instance.screenSize.y
            || transform.position.y > Manager.instance.screenSize.y
            || transform.position.x < -Manager.instance.screenSize.x
            || transform.position.x > Manager.instance.screenSize.x
        )
            Destroy(gameObject, 1f);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag != "Bullet" && moveSpeed != 0)
        {
            if (color == Player.instance.color)
            {
                ScreenShake.instance.Shake();
                other.gameObject.SendMessage("Hit", transform.up);
            }
            else
            {
                if (!Manager.instance.enemies.Contains(other.gameObject.tag))
                    other.gameObject.SendMessage("Hit", transform.up);
            }
            Death();
        }
    }

    void Death()
    {
        moveSpeed = 0;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<TrailRenderer>().enabled = false;
        ps.Play();
        Destroy(gameObject, 2f);
    }
}
