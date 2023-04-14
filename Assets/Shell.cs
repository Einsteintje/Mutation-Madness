using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Shell : MonoBehaviour
{
    public float explosionRange;

    [SerializeField]
    float speed;
    public Color color;

    [HideInInspector]
    public Vector3 destination;

    float travelTime;
    float timeTraveled;
    Vector3 originalSize;
    Vector3 movement;

    [SerializeField]
    GameObject circle;
    SpriteRenderer circleRenderer;
    ParticleSystem[] psList;

    [HideInInspector]
    public string mutation;

    void Start()
    {
        circle = Instantiate(circle);
        circle.transform.position = destination;
        originalSize = transform.localScale;
        circleRenderer = circle.GetComponent<SpriteRenderer>();
        GetComponent<SpriteRenderer>().color = color;
        travelTime = Vector3.Distance(transform.position, destination) / speed;
        psList = GetComponentsInChildren<ParticleSystem>();
        movement = (destination - transform.position).normalized;
    }

    void FixedUpdate()
    {
        if (timeTraveled >= travelTime)
        {
            if (!psList[0].isPlaying)
            {
                ScreenShake.instance.Shake();
                GetComponent<SpriteRenderer>().enabled = false;
                circleRenderer.enabled = false;
                Destroy(circle, 1f);
                Destroy(gameObject, 1f);
                foreach (ParticleSystem ps in psList)
                    ps.Play();
                Collider2D[] colliders = Physics2D.OverlapCircleAll(
                    transform.position,
                    explosionRange
                );
                foreach (Collider2D collider in colliders)
                    collider.gameObject.SendMessage(
                        "Hit",
                        (
                            (Player.instance.transform.position * 1.0001f - destination).normalized
                                * explosionRange
                                / 2,
                            mutation
                        )
                    );
            }
        }
        else
        {
            timeTraveled += Time.fixedDeltaTime;
            transform.localScale =
                originalSize * (1 + Mathf.Sin(timeTraveled * Mathf.PI / travelTime) * 3);
            color.a = timeTraveled / travelTime;
            circleRenderer.color = color;
            transform.position += movement * speed * Time.fixedDeltaTime;
        }
    }
}
