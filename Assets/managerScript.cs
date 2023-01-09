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
    private int turretAmount = 6;
    private int barrelAmount = 8;
    private int enemyAmount = 4;
    private float currentOffset;
    public string state = "Idle";
    private Dictionary<string, int> objectDict = new Dictionary<string, int>
    {
        { "Turret", 0 },
        { "Barrel", 1 },
        { "Enemy", 2 },
        { "Box", 3 }
    };

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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ClearMap();
            state = "Clearing";
        }
        if (state == "Generating")
        {
            for (int i = 0; i < objectLists.Count; i++)
                objectLists[i].Clear();
            objectSpots.Clear();
            NewMap();
        }
    }

    void ClearMap()
    {
        for (int i = 0; i < objectLists.Count; i++)
            objectLists[i].RemoveAll(s => s == null);

        turretAmount = 0 + objectLists[0].Count;
        barrelAmount = 0 + objectLists[1].Count;
        StartCoroutine(DestroyObjects());
    }

    IEnumerator DestroyObjects()
    {
        WaitForSeconds wait = new WaitForSeconds(0.01f);

        foreach (List<GameObject> objectList in objectLists)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                if (objectList[i] == null)
                    continue;

                objectList[i].SendMessage("Death");
                //ParticleSystem[] psList = objectList[i].GetComponentsInChildren<ParticleSystem>();
                //foreach (ParticleSystem ps in psList)
                //    ps.Play();
                // objectList[i]..spriteRenderer.enabled = false;
                //Destroy(objectList[i], 2f);
                yield return wait;
            }

            //   this.Log(objectList.Count);
        }
        yield return wait;
        state = "Generating";
    }

    void NewMap()
    {
        currentOffset = Random.Range(100, 10000);
        List<List<float>> list = GenerateMap();
        LoadMap(list);
        Objects();
        Enemies();
        state = "Idle";
        //objectLists[2].Shuffle();
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

    void LoadMap(List<List<float>> list)
    {
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
                    }
                }
                else if (!Neighbours(list, x, y))
                    objectSpots.Add(new Vector2Int(x, y));
                else
                    enemySpots.Add(new Vector2Int(x, y));
            }
        }
    }

    void Enemies()
    {
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
        }
    }

    void Objects()
    {
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
            objectLists[objectDict[spawned.tag]].Add(spawned);
        }
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
