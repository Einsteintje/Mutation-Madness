using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance { get; private set; }
    public int currency;

    GameObject health,
        shield,
        speed,
        reload,
        play;

    public GameObject currencyObj;
    TMP_Text currencyText;

    public Dictionary<string, Upgrade> upgrades = new Dictionary<string, Upgrade>();

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
            //DontDestroyOnLoad(canvas);
            //DontDestroyOnLoad(volume);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        play = GameObject.FindWithTag("Play");
        health = GameObject.FindWithTag("Health");
        reload = GameObject.FindWithTag("Reload");
        speed = GameObject.FindWithTag("Speed");
        shield = GameObject.FindWithTag("Shield");
        currencyObj = GameObject.FindWithTag("Currency");
        currencyText = currencyObj.GetComponentInChildren<TMP_Text>();
        if (upgrades.Keys.Contains("Health"))
        {
            upgrades["Health"].obj = health;
            upgrades["Health"].text = health.GetComponentInChildren<TMP_Text>();
        }
        else
            upgrades["Health"] = new Upgrade(
                1f,
                100,
                health.GetComponentInChildren<TMP_Text>(),
                health
            );
        if (upgrades.Keys.Contains("Shield"))
        {
            upgrades["Shield"].obj = shield;
            upgrades["Shield"].text = shield.GetComponentInChildren<TMP_Text>();
        }
        else
            upgrades["Shield"] = new Upgrade(
                1f,
                100,
                shield.GetComponentInChildren<TMP_Text>(),
                shield
            );
        if (upgrades.Keys.Contains("Speed"))
        {
            upgrades["Speed"].obj = speed;
            upgrades["Speed"].text = speed.GetComponentInChildren<TMP_Text>();
        }
        else
            upgrades["Speed"] = new Upgrade(
                1f,
                100,
                speed.GetComponentInChildren<TMP_Text>(),
                speed
            );
        if (upgrades.Keys.Contains("Reload"))
        {
            upgrades["Reload"].obj = reload;
            upgrades["Reload"].text = reload.GetComponentInChildren<TMP_Text>();
        }
        else
            upgrades["Reload"] = new Upgrade(
                1f,
                100,
                reload.GetComponentInChildren<TMP_Text>(),
                reload
            );

        foreach (Upgrade upgrade in upgrades.Values)
        {
            upgrade.text.text = $"{upgrade.obj.name} - {upgrade.cost.ToString()}";
        }
        health.GetComponent<Button>().onClick.AddListener(() => ButtonClick(health));
        speed.GetComponent<Button>().onClick.AddListener(() => ButtonClick(speed));
        reload.GetComponent<Button>().onClick.AddListener(() => ButtonClick(reload));
        shield.GetComponent<Button>().onClick.AddListener(() => ButtonClick(shield));
        play.GetComponent<Button>().onClick.AddListener(() => PlayGame());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currencyText.text = $"Currency = {currency.ToString()}";
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void ButtonClick(GameObject obj)
    {
        if (currency > upgrades[obj.name].cost)
        {
            currency -= upgrades[obj.name].cost;
            upgrades[obj.name].cost *= 2;
            upgrades[obj.name].boost += 0.1f;
            upgrades[obj.name].text.text = $"{obj.name} - {upgrades[obj.name].cost.ToString()}";
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

public class Upgrade
{
    public float boost;
    public int cost;
    public TMP_Text text;
    public GameObject obj;

    public Upgrade(float Boost, int Cost, TMP_Text Text, GameObject Obj)
    {
        boost = Boost;
        cost = Cost;
        text = Text;
        obj = Obj;
    }
}
