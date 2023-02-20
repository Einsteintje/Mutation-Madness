using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class managerScript : MonoBehaviour
{
    public GameObject box;
    public GameObject barrel;
    public GameObject turret;
    public GameObject enemy;
    public Vector3 screenSize;
    public Vector3 objectSize;
    public GameObject navMesh;

    private float scale = 0.2f;
    private float noise = 0.5f;
    private int turretAmount = 1;
    private int barrelAmount = 1;
    private int enemyAmount = 1;
    private float currentOffset;
    public string state = "Idle";

    public int maxTimer = 5;
    public float waveTimer;
    public int currentWave = 0;
    public int currentPower = 0;

    private Dictionary<string, int> objectDict = new Dictionary<string, int>
    {
        { "Turret", 0 },
        { "Barrel", 1 },
        { "Enemy", 2 },
        { "Box", 3 }
    };

    private Dictionary<string, int> powerDict = new Dictionary<string, int>{
        {"Turret", 100},
        {"Enemy", 50}
    };

    private List<string> enemies = new List<string>{"Turret", "Enemy"};

    private List<Vector2Int> objectSpots = new List<Vector2Int>();
    private List<Vector2Int> enemySpots = new List<Vector2Int>();

    private List<List<GameObject>> objectLists = new List<List<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        objectSize = barrel.transform.localScale;

        for (int i = 0; i < objectDict.Count; i++)
            objectLists.Add(new List<GameObject>());
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

    List<string> GetEnemies(){
        List<string> spawns = new List<string>();
        while (currentPower > 0){
            int randomIndex = Random.Range(0, enemies.Count);
            string randomKey = enemies[randomIndex];
            if (currentPower >= powerDict[randomKey]){
                currentPower -= powerDict[randomKey];
                spawns.Add(randomKey);
                }
            }
        return spawns;
    }

    void ClearMap()
    {
        for (int i = 0; i < objectLists.Count; i++)
            objectLists[i].RemoveAll(s => s == null);
        currentWave++;
        currentPower+= currentWave * 150;
        List<string> spawns = GetEnemies();
        this.Log(spawns);

        turretAmount = 2 + objectLists[0].Count;
        barrelAmount = 3;
        enemyAmount = 2 + objectLists[1].Count;
        StartCoroutine(DestroyObjects());
    }

    IEnumerator DestroyObjects()
    {
        WaitForSeconds wait = new WaitForSeconds(0.005f);

        foreach (List<GameObject> objectList in objectLists)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                if (objectList[i] == null)
                    continue;
                objectList[i].SendMessage("Death");
                yield return wait;
            }
        }
        yield return wait;
        state = "Generating";
        for (int i = 0; i < objectLists.Count; i++)
            objectLists[i].Clear();
        objectSpots.Clear();
        NewMap();
    }

    void NewMap()
    {
        currentOffset = Random.Range(100, 10000);
        List<List<float>> list = GenerateMap();
        StartCoroutine(LoadMap(list));
    }

    List<List<float>> GenerateMap()
    {
        List<List<float>> list = new List<List<float>>();
        for (int y = 0; y < 1080 / 60; y++)
        {
            list.Add(new List<float>());
            for (int x = 0; x < 1920 / 60; x++)
            {
                list[y].Add(Mathf.PerlinNoise(x * scale + currentOffset, y * scale + 30));
            }
        }
        return list;
    }

    IEnumerator LoadMap(List<List<float>> list)
    {
        WaitForSeconds wait = new WaitForSeconds(0.005f);

        //boxes
        Vector2Int middle = new Vector2Int(list[0].Count / 2, list.Count / 2);
        for (int y = 0; y < list.Count; y++)
        {
            for (int x = 0; x < list[y].Count; x++)
            {
                Vector3 pos = new Vector3(
                    (x + 0.5f) * objectSize.x - screenSize.x,
                    (y + 0.5f) * objectSize.y - screenSize.y,
                    0
                );
                if (list[y][x] > noise)
                {
                    if (Mathf.Abs(middle.x - x) > 1 && Mathf.Abs(middle.y - y) > 1)
                    {
                        GameObject spawned = Instantiate(box, pos, box.transform.rotation);
                        objectLists[objectDict[spawned.tag]].Add(spawned);
                        spawned.transform.parent = navMesh.transform;
                        yield return wait;
                    }
                }
                else if (!Neighbours(list, x, y))
                    objectSpots.Add(new Vector2Int(x, y));
                else
                    enemySpots.Add(new Vector2Int(x, y));
            }
        }

        //enemies
        enemySpots.Shuffle();
        for (int i = 0; i < enemyAmount; i++)
        {
            Vector3 pos = new Vector3(
                (enemySpots[i].x + 0.5f) * objectSize.x - screenSize.x,
                (enemySpots[i].y + 0.5f) * objectSize.y - screenSize.y,
                0
            );
            GameObject spawned = Instantiate(enemy, pos, enemy.transform.rotation);
            objectLists[objectDict[spawned.tag]].Add(spawned);
            yield return wait;
        }

        List<Vector2Int> spots = new List<Vector2Int>();
        List<int> randomNums = GenerateRandom(turretAmount + barrelAmount, 0, objectSpots.Count);
        foreach (int num in randomNums)
        {
            int crashFix = 0;
            int test = num;
            while (Neighbours2(spots, objectSpots[test]))
            {
                test = Random.Range(0, objectSpots.Count);
                crashFix += 1;
                if (crashFix > 100)
                {
                    this.Log("Crash fix1!");
                    break;
                }
            }
            spots.Add(objectSpots[test]);
        }

        //objects
        for (int i = 0; i < spots.Count; i++)
        {
            GameObject spawned;
            Vector3 pos = new Vector3(
                (spots[i].x + 0.5f) * objectSize.x - screenSize.x,
                (spots[i].y + 0.5f) * objectSize.y - screenSize.y,
                0
            );
            if (i < turretAmount)
            {
                spawned = Instantiate(turret, pos, turret.transform.rotation);
                spawned.transform.parent = navMesh.transform;
            }
            else
                spawned = Instantiate(barrel, pos, barrel.transform.rotation);
            spawned.transform.parent = navMesh.transform;
            objectLists[objectDict[spawned.tag]].Add(spawned);
            yield return wait;
        }

        yield return wait;
        state = "Idle";
    }

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

    bool Neighbours(List<List<float>> list, int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (
                    y + i >= 0
                    && y + i < list.Count
                    && x + j >= 0
                    && x + j < list[y].Count
                    && list[y + i][x + j] > noise
                )
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool Neighbours2(List<Vector2Int> list, Vector2Int pos)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (list.Contains(new Vector2Int(pos.x + i, pos.y + j)))
                    return true;
            }
        }
        return false;
    }
}
