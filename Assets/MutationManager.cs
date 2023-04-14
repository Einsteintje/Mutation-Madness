using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MutationManager : MonoBehaviour
{
    public static MutationManager instance { get; private set; }

    [HideInInspector]
    public Dictionary<string, Mutation> mutations = new Dictionary<string, Mutation>();

    public ParticleSystem firePS,
        icePS,
        electricPS;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        mutations["Fire"] = new Mutation(Color.red, firePS, Fire);
        mutations["Ice"] = new Mutation(Color.cyan, icePS, Ice);
        mutations["Electric"] = new Mutation(Color.yellow, electricPS, Electric);
    }

    // Update is called once per frame
    void Update() { }

    Action<GameObject> Fire = (obj) =>
    {
        if (obj.tag == "Player")
        {
            Player.instance.mutationEffects["Fire"] -= Time.fixedDeltaTime / 5;
            Player.instance.fireSpeed = 1.5f;
            Player.instance.fireTimer += Time.fixedDeltaTime;
            if (Player.instance.fireTimer > 2)
            {
                Player.instance.fireTimer = 0;
                Player.instance.hP -= 1;
            }
        }
    };

    Action<GameObject> Ice = (obj) =>
    {
        if (obj.tag == "Player")
        {
            Player.instance.mutationEffects["Ice"] -= Time.fixedDeltaTime / 5;
            Player.instance.slipperyness = 0.03f;
        }
    };

    Action<GameObject> Electric = (obj) =>
    {
        if (obj.tag == "Player")
        {
            Player.instance.mutationEffects["Electric"] -= Time.fixedDeltaTime / 5;
            Player.instance.charged = 1.5f;
        }
    };
}

public class Mutation
{
    public Color color;
    public ParticleSystem ps;
    public Action<GameObject> action;

    public Mutation(Color Color, ParticleSystem PS, Action<GameObject> Action)
    {
        color = Color;
        ps = PS;
        action = Action;
    }
}
