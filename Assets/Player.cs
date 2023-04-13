using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public float moveSpeed;
    public weaponScript weapon;
    public ParticleSystem hitPS;
    public CircleCollider2D col;
    Vector3 knockback = new Vector3();
    float flashTime = 0.05f;

    [HideInInspector]
    public Vector3 movement;

    [HideInInspector]
    public SpriteRenderer[] renderers;

    [HideInInspector]
    public Color color;

    [SerializeField]
    Volume volume;
    Vignette vignette;

    [HideInInspector]
    public Dictionary<string, float> mutationEffects = new Dictionary<string, float>
    {
        { "Ice", 0.0f },
        { "Fire", 0.0f },
        { "Electric", 0.0f }
    };

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
        weapon = GetComponentInChildren<weaponScript>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        volume.profile.TryGet(out vignette);
        color = renderers[0].color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        vignette.center.value = new Vector2(
            (transform.position.x / 2 + Manager.instance.screenSize.x / 2)
                / (Manager.instance.screenSize.x),
            (transform.position.y / 2 + Manager.instance.screenSize.y / 2)
                / (Manager.instance.screenSize.y)
        );
        knockback = new Vector3(
            Mathf.Lerp(knockback.x, 0, 0.3f),
            Mathf.Lerp(knockback.y, 0, 0.3f),
            0
        );
        if (Manager.instance.state == "Idle")
        {
            col.enabled = true;
            weapon.enabled = true;
            //movement
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            input = Vector3.ClampMagnitude(input, 1);
            input -= weapon.recoil;
            movement = input * moveSpeed * Time.fixedDeltaTime + knockback;
            transform.Translate(movement);
            //stay inside the screen
            Vector3 clampedPos = Manager.instance.Clamp(transform.position);
            if (clampedPos.x != transform.position.x || clampedPos.y != transform.position.y)
                transform.position = new Vector3(clampedPos.x, clampedPos.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, 0, 0.04f),
                Mathf.Lerp(transform.position.y, 0, 0.04f),
                transform.position.z
            );
            col.enabled = false;
            weapon.enabled = false;
            weapon.recoil = new Vector3(0, 0, 0);
        }
    }

    public void Hit((Vector3, string) tuple)
    {
        mutationEffects[tuple.Item2] = 1.0f;
        knockback = tuple.Item1;
        HitPS(tuple.Item1);
        Flash();
    }

    void Flash()
    {
        foreach (SpriteRenderer renderer in renderers)
            renderer.color = Color.white;
        Invoke("ResetColor", flashTime);
    }

    void ResetColor()
    {
        foreach (SpriteRenderer renderer in renderers)
            renderer.color = color;
    }

    public void HitPS(Vector3 kb)
    {
        ParticleSystem ps = Instantiate(hitPS, Manager.instance.transform);
        var subEmitter = ps.subEmitters.GetSubEmitterSystem(0);
        var main = ps.main;
        main.startColor = color;
        main = subEmitter.main;
        main.startColor = color;
        ps.transform.rotation = Quaternion.Euler(kb);
        ps.transform.position = transform.position;
        ps.Play();
    }
}
