using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class explosionScript : MonoBehaviour
{
    public ParticleSystem[] psList;
    public float explosionRange;
    string mutation;
    public GameObject[] gameObjects;

    public Dictionary<string, GameObject> mutationPS = new Dictionary<string, GameObject>();

    void Start()
    {
        mutationPS["Fire"] = gameObjects[0];
        mutationPS["Ice"] = gameObjects[1];
        mutationPS["Electric"] = gameObjects[2];

        mutation = MutationManager.instance.mutations.Keys.ElementAt(
            Random.Range(0, MutationManager.instance.mutations.Keys.Count)
        );
        GameObject subGameObject = Instantiate(mutationPS[mutation], transform);
        GetComponent<SpriteRenderer>().color = MutationManager.instance.mutations[mutation].color;
        psList = mutationPS[mutation].GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in psList)
        {
            Instantiate(ps, transform);
        }
        Destroy(subGameObject);
        psList = GetComponentsInChildren<ParticleSystem>();
    }

    void Hit()
    {
        Death();
    }

    void Death()
    {
        if (GetComponent<SpriteRenderer>() != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRange);
            foreach (Collider2D collider in colliders)
                if (collider.gameObject.tag != "Barrel")
                    collider.gameObject.SendMessage(
                        "Hit",
                        (
                            (
                                collider.gameObject.transform.position * 1.0001f
                                - transform.position
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
                if (ps != null)
                    ps.Play();
            }
            Destroy(gameObject, 1f);
        }
    }
}
