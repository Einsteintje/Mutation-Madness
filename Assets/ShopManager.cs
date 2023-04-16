using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance { get; private set; }
    public float currency;

    [SerializeField]
    GameObject health,
        shield,
        speed,
        reload;

    public GameObject canvas;

    public TMP_Text currencyText;

    public Dictionary<string, Upgrade> upgrades = new Dictionary<string, Upgrade>();

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(canvas);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        upgrades["Health"] = new Upgrade(
            1f,
            100,
            health.GetComponentInChildren<TMP_Text>(),
            health
        );
        upgrades["Shield"] = new Upgrade(
            1f,
            100,
            shield.GetComponentInChildren<TMP_Text>(),
            shield
        );
        upgrades["Speed"] = new Upgrade(1f, 100, speed.GetComponentInChildren<TMP_Text>(), speed);
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
        currencyText.text = $"Currency = {currency.ToString()}";
    }

    // Update is called once per frame
    void Update() { }

    public void ButtonClick(GameObject obj)
    {
        if (currency > upgrades[obj.name].cost)
        {
            currency -= upgrades[obj.name].cost;
            upgrades[obj.name].cost *= 2;
            upgrades[obj.name].boost += 0.1f;
            upgrades[obj.name].text.text = $"{obj.name} - {upgrades[obj.name].cost.ToString()}";
            currencyText.text = $"Currency = {currency.ToString()}";
        }
    }

    public void PlayGame()
    {
        canvas.SetActive(false);
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
