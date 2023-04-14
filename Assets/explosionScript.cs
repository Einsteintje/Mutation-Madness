using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class explosionScript : MonoBehaviour
{
    public ParticleSystem[] psList;
    public float explosionRange;
    string mutation;

    void Start()
    {
        psList = GetComponentsInChildren<ParticleSystem>();
        mutation = MutationManager.instance.mutations.Keys.ElementAt(
            Random.Range(0, MutationManager.instance.mutations.Keys.Count)
        );
        GetComponent<SpriteRenderer>().color = MutationManager.instance.mutations[mutation].color;
    }

    void Hit()
    {
        Death();
    }

    void Death()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRange);
        foreach (Collider2D collider in colliders)
            collider.gameObject.SendMessage(
                "Hit",
                (
                    (
                        collider.gameObject.transform.position * 1.0001f - transform.position
                    ).normalized
                        * explosionRange
                        / 2,
                    mutation
                )
            );
        ScreenShake.instance.Shake();
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        foreach (ParticleSystem ps in psList)
        {
            ps.Play();
        }
        Destroy(gameObject, 1f);
    }
}
