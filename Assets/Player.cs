using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public float moveSpeed;
    public weaponScript weapon;
    public ParticleSystem hitPS;
    public CircleCollider2D col;
    Vector3 knockback = new Vector3();
    Vector3 input = new Vector3();
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
    public string mutation;

    [HideInInspector]
    public Dictionary<string, float> mutationEffects = new Dictionary<string, float>
    {
        { "Ice", 0.0f },
        { "Fire", 0.0f },
        { "Electric", 0.0f }
    };
    Dictionary<string, ParticleSystem> mutationPS = new Dictionary<string, ParticleSystem>();

    [HideInInspector]
    public float slipperyness = 1f;

    [HideInInspector]
    public float fireSpeed = 1f;

    [HideInInspector]
    public float fireTimer = 0f;

    [HideInInspector]
    public float charged = 1f;

    public int hP;
    public int maxHP;
    float healthTimer;
    float maxHealthTimer;
    public GameObject healthBar;

    [HideInInspector]
    public healthBarScript healthBarScript;

    public int score;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SetMutation", 0.1f);
        weapon = GetComponentInChildren<weaponScript>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        volume.profile.TryGet(out vignette);
        color = renderers[0].color;
        healthBar = Instantiate(healthBar);
        healthBarScript = healthBar.GetComponent<healthBarScript>();

        if (ShopManager.instance != null)
            maxHP = (int)(maxHP * ShopManager.instance.upgrades["Health"].boost);

        hP = maxHP;
        healthBar.transform.position = transform.position + Vector3.up * 3;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MutationHandler();
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
            input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            input = Vector3.ClampMagnitude(input, 1);
            input -= weapon.recoil;
            if (ShopManager.instance != null)
                movement = Vector3.Lerp(
                    movement,
                    input
                        * moveSpeed
                        * ShopManager.instance.upgrades["Speed"].boost
                        / Mathf.Pow(slipperyness, 1f / 10f)
                        * fireSpeed
                        * Time.fixedDeltaTime
                        + knockback,
                    slipperyness
                );
            else
                movement = Vector3.Lerp(
                    movement,
                    input
                        * moveSpeed
                        / Mathf.Pow(slipperyness, 1f / 10f)
                        * fireSpeed
                        * Time.fixedDeltaTime
                        + knockback,
                    slipperyness
                );
            transform.Translate(movement);

            //stay inside the screen
            Vector3 clampedPos = Manager.instance.Clamp(transform.position);
            if (clampedPos.x != transform.position.x || clampedPos.y != transform.position.y)
                transform.position = new Vector3(clampedPos.x, clampedPos.y, transform.position.z);
            healthBar.transform.position = transform.position + Vector3.up * 3;
            if (hP != healthBarScript.hP)
            {
                healthBarScript.FadeIn();
                healthBarScript.UpdateSlider(hP, maxHP);
                healthTimer = maxHealthTimer;
            }
            else if (healthTimer > 0)
                healthTimer -= Time.fixedDeltaTime;
        }
        else
        {
            healthBar.transform.position = transform.position + Vector3.up * 3;
            foreach (string mutation in MutationManager.instance.mutations.Keys)
                mutationEffects[mutation] = 0f;

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
        hP--;
        if (hP > 0)
        {
            mutationEffects[tuple.Item2] = 1.0f;
            knockback = tuple.Item1;
            HitPS(tuple.Item1);
            Flash();
        }
        else
        {
            if (ShopManager.instance != null)
            {
                ShopManager.instance.Invoke("Start", 0.1f);
                //foreach (GameObject barrel in ShopManager.instance.barrels)
                //barrel.SetActive(true);
                //ShopManager.instance.canvas.SetActive(true);
                //ShopManager.instance.volume.SetActive(true);
                ShopManager.instance.currency += score;
                //ShopManager.instance.currencyText.text =
                //$"Currency = {ShopManager.instance.currency.ToString()}";
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            }
        }
    }

    void SetMutation()
    {
        if (ShopManager.instance.randomMutation)
        {
            ShopManager.instance.randomMutation = false;
            mutation = MutationManager.instance.mutations.Keys.ElementAt(
                Random.Range(0, MutationManager.instance.mutations.Keys.Count)
            );
        }
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

    void MutationHandler()
    {
        foreach (string mutation in MutationManager.instance.mutations.Keys)
        {
            if (mutationEffects[mutation] > 0)
            {
                if (!mutationPS.Keys.Contains(mutation))
                    mutationPS[mutation] = Instantiate(
                        MutationManager.instance.mutations[mutation].ps,
                        transform
                    );
                if (!mutationPS[mutation].isPlaying)
                    mutationPS[mutation].Play();
                MutationManager.instance.mutations[mutation].action(this.gameObject);
            }
            else if (mutationPS.Keys.Contains(mutation))
            {
                if (mutationPS[mutation].isPlaying)
                    mutationPS[mutation].Stop();
                if (mutation == "Fire")
                {
                    fireSpeed = 1f;
                    fireTimer = 0f;
                }
                else if (mutation == "Ice")
                {
                    slipperyness = 1f;
                }
                else
                    charged = 1.0f;
            }
        }
    }
}
