using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Manager : MonoBehaviour
{
    public static Manager instance { get; private set; }

    [SerializeField]
    GameObject box;

    [SerializeField]
    GameObject barrel;

    [SerializeField]
    GameObject turret;

    [SerializeField]
    GameObject shooter;

    [SerializeField]
    GameObject dasher;

    [SerializeField]
    GameObject thrower;
    public Vector3 screenSize;
    public Vector3 objectSize;
    public GameObject navMesh;

    private float scale = 0.2f;
    private float noise = 0.5f;

    private float offset;
    public string state = "Idle";

    public int maxTimer = 30;
    public float waveTimer;
    public int wave = 0;
    public int wavePower = 0;

    [HideInInspector]
    public Dictionary<string, MyValue> objectDict = new Dictionary<string, MyValue>();

    private List<Vector2Int> objectSpots = new List<Vector2Int>();
    private List<Vector2Int> enemySpots = new List<Vector2Int>();

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

        objectDict["Shooter"] = new MyValue(10, 0, "Enemy", shooter, new List<GameObject>());
        objectDict["Dasher"] = new MyValue(20, 0, "Enemy", dasher, new List<GameObject>());
        objectDict["Thrower"] = new MyValue(30, 0, "Enemy", thrower, new List<GameObject>());
        objectDict["Turret"] = new MyValue(100, 0, "Stationary", turret, new List<GameObject>());
        objectDict["Barrel"] = new MyValue(0, 3, "Stationary", barrel, new List<GameObject>());
        objectDict["Box"] = new MyValue(0, 0, "Map", box, new List<GameObject>());
    }

    void Start()
    {
        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        objectSize = turret.transform.localScale;

        NewMap();
        waveTimer = maxTimer;
        state = "Generating";
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == "Idle")
            waveTimer -= Time.fixedDeltaTime;

        if (Input.GetMouseButtonDown(1) || waveTimer < 0)
        {
            waveTimer = maxTimer;
            ClearMap();
            state = "Clearing";
        }
    }

    void NewMap()
    {
        offset = Random.Range(100, 10000);
        List<List<float>> list = GenerateMap();
        AddEnemies();
        StartCoroutine(LoadMap(list));
    }

    void ClearMap()
    {
        foreach (MyValue value in objectDict.Values)
            value.objects.RemoveAll(s => s == null);
        wave++;
        for (int i = 0; i < AIManager.instance.spots; i++)
            foreach (string distance in AIManager.instance.distances)
                AIManager.instance.dict[(distance, i)] = true;
        StartCoroutine(DestroyObjects());
    }

    void AddEnemies()
    {
        wavePower += 50 + wave * 50;

        //get strongest enemy spawn
        (string, int) strongestSpawn = ("_", 0);
        foreach (string key in objectDict.Keys)
            if (objectDict[key].power > strongestSpawn.Item2 && objectDict[key].power <= wavePower)
            {
                strongestSpawn.Item1 = key;
                strongestSpawn.Item2 = objectDict[key].power;
            }
        objectDict[strongestSpawn.Item1].amount++;
        wavePower -= strongestSpawn.Item2;

        //get random enemies
        while (wavePower > 0)
        {
            MyValue randomValue = objectDict.Values.ElementAt(Random.Range(0, objectDict.Count));
            if (wavePower >= randomValue.power)
            {
                wavePower -= randomValue.power;
                randomValue.amount++;
            }
        }

        //add any leftover enemies from last wave
        foreach (MyValue value in objectDict.Values)
        {
            value.amount += value.objects.Count;
            value.objects.Clear();
        }

        //respawn barrels
        objectDict["Barrel"].amount = Random.Range(3, 5);
    }

    IEnumerator DestroyObjects()
    {
        WaitForSeconds wait = new WaitForSeconds(0.003f);
        WaitForSeconds shorterWait = new WaitForSeconds(0.001f);
        foreach (MyValue value in objectDict.Values)
            for (int i = 0; i < value.objects.Count; i++)
            {
                if (value.objects[i] == null)
                    continue;
                value.objects[i].SendMessage("Death");
                yield return wait;
            }

        ParticleSystem[] psList = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < psList.Length; i++)
        {
            UnityEngine.ParticleSystem.Particle[] particles =
                new UnityEngine.ParticleSystem.Particle[psList[i].particleCount];
            int numParticles = psList[i].GetParticles(particles);

            for (int time = 0; time < numParticles; time++)
            {
                particles[time].remainingLifetime = 0.01f;
            }
            psList[i].SetParticles(particles, numParticles);
            Destroy(psList[i], 1);
            //yield return shorterWait;
        }
        state = "Generating";
        objectSpots.Clear();
        NewMap();
    }

    IEnumerator LoadMap(List<List<float>> list)
    {
        WaitForSeconds wait = new WaitForSeconds(0.003f);

        //boxes
        Vector2Int middle = new Vector2Int(list[0].Count / 2, list.Count / 2);
        for (int y = 0; y < list.Count; y++)
            for (int x = 0; x < list[y].Count; x++)
            {
                Vector3 pos = new Vector3(
                    (x + 0.5f) * objectSize.x - screenSize.x,
                    (y + 0.5f) * objectSize.y - screenSize.y,
                    0
                );
                if (Mathf.Abs(middle.x - x) > 4 || Mathf.Abs(middle.y - y) > 4)
                {
                    if (list[y][x] > noise)
                    {
                        this.Log(pos);
                        GameObject spawned = Instantiate(box, pos, box.transform.rotation);
                        spawned.transform.parent = navMesh.transform;
                        objectDict[spawned.tag].objects.Add(spawned);
                        yield return wait;
                    }
                    else if (!Neighbours(list, x, y))
                        objectSpots.Add(new Vector2Int(x, y));
                    else
                        enemySpots.Add(new Vector2Int(x, y));
                }
            }

        //enemies
        enemySpots.Shuffle();
        foreach (MyValue value in objectDict.Values)
            if (value.group == "Enemy")
                for (int i = value.amount; i > 0; i--)
                {
                    Vector3 pos = new Vector3(
                        (enemySpots[i].x + 0.5f) * objectSize.x - screenSize.x,
                        (enemySpots[i].y + 0.5f) * objectSize.y - screenSize.y,
                        0
                    );
                    GameObject spawned = Instantiate(value.obj, pos, value.obj.transform.rotation);
                    enemySpots.RemoveAt(i);
                    value.amount--;
                    value.objects.Add(spawned);
                    yield return wait;
                }

        //objects
        List<Vector2Int> spots = new List<Vector2Int>();
        int stationaryAmount = 0;
        foreach (MyValue value in objectDict.Values)
            if (value.group == "Stationary")
                stationaryAmount += value.amount;
        List<int> randomNums = GenerateRandom(stationaryAmount, 0, objectSpots.Count);
        foreach (int num in randomNums)
        {
            int crashFix = 0;
            int numToTest = num;
            while (Neighbours2(spots, objectSpots[numToTest]))
            {
                numToTest = Random.Range(0, objectSpots.Count);
                crashFix += 1;
                if (crashFix > 100)
                {
                    this.Log("Crash fix1!");
                    break;
                }
            }
            spots.Add(objectSpots[numToTest]);
        }

        int index = -1;
        foreach (MyValue value in objectDict.Values)
            if (value.group == "Stationary")
                while (value.amount > 0)
                {
                    index += 1;
                    Vector3 pos = new Vector3(
                        (spots[index].x + 0.5f) * objectSize.x - screenSize.x,
                        (spots[index].y + 0.5f) * objectSize.y - screenSize.y,
                        0
                    );
                    GameObject spawned = Instantiate(value.obj, pos, value.obj.transform.rotation);
                    spawned.transform.parent = navMesh.transform;
                    value.amount--;
                    value.objects.Add(spawned);
                    yield return wait;
                }
        state = "Idle";
    }

    //function to generate a perlin noise map
    List<List<float>> GenerateMap()
    {
        List<List<float>> list = new List<List<float>>();
        for (int y = 0; y < screenSize.y / 2; y++)
        {
            list.Add(new List<float>());
            for (int x = 0; x < screenSize.x / 2; x++)
            {
                list[y].Add(Mathf.PerlinNoise(x * scale + offset, y * scale + 30));
            }
        }
        return list;
    }

    // get an amount of random numbers between min and max
    List<int> GenerateRandom(int count, int min, int max)
    {
        List<int> list = new List<int>();
        int random;
        for (int i = 0; i < count; i++)
        {
            int crashFix = 0;
            random = Random.Range(min, max);
            while (list.Contains(random))
            {
                random = Random.Range(min, max);
                crashFix += 1;
                if (crashFix > 100)
                {
                    this.Log("Crash fix2!");
                    break;
                }
            }
            list.Add(random);
        }
        return list;
    }

    public bool InScreen(Vector3 pos)
    {
        if (Mathf.Abs(pos.x) < screenSize.x && Mathf.Abs(pos.y) < screenSize.y)
            return true;
        else
            return false;
    }

    public Vector3 Clamp(Vector3 pos)
    {
        float x = Mathf.Clamp(
            pos.x,
            -screenSize.x + objectSize.x / 2,
            screenSize.x - objectSize.x / 2
        );
        float y = Mathf.Clamp(
            pos.y,
            -screenSize.y + objectSize.y / 2,
            screenSize.y - objectSize.y / 2
        );
        return new Vector3(x, y, 0);
    }

    bool Neighbours(List<List<float>> list, int x, int y)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if (
                    y + i >= 0
                    && y + i < list.Count
                    && x + j >= 0
                    && x + j < list[y].Count
                    && list[y + i][x + j] > noise
                )
                    return true;
        return false;
    }

    bool Neighbours2(List<Vector2Int> list, Vector2Int pos)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if (list.Contains(new Vector2Int(pos.x + i, pos.y + j)))
                    return true;
        return false;
    }
}

public class MyValue
{
    public int power;
    public int amount;
    public string group;
    public GameObject obj;
    public List<GameObject> objects;

    public MyValue(int Power, int Amount, string Group, GameObject Obj, List<GameObject> Objects)
    {
        power = Power;
        amount = Amount;
        group = Group;
        obj = Obj;
        objects = Objects;
    }
}
