using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance { get; private set; }

    public Dictionary<(string, int), bool> dict = new Dictionary<(string, int), bool>();
    List<int> list = new List<int>();
    public List<string> distances = new List<string> { "Close", "Far" };
    public int spots = 10;
    Dictionary<string, int> radius = new Dictionary<string, int> { { "Close", 10 }, { "Far", 50 } };

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    void Start()
    {
        for (int i = 0; i < spots; i++)
        {
            list.Add(i);
            foreach (string distance in distances)
            {
                dict[(distance, i)] = true;
            }
        }
    }

    public Vector3 GetPos(int oldPos, out int newPos, string distance)
    {
        list.Shuffle();
        for (int j = -5; j <= 5; j += 5)
            foreach (int i in list)
                if (dict[(distance, i)])
                {
                    Vector3 pos = new Vector3(
                        Mathf.Cos(Mathf.Deg2Rad * 360 / spots * i) * radius[distance] + j,
                        Mathf.Sin(Mathf.Deg2Rad * 360 / spots * i) * radius[distance] + j,
                        0
                    );
                    RaycastHit2D hit = Physics2D.Linecast(
                        Player.instance.transform.position + pos,
                        Player.instance.transform.position
                    );

                    if (hit)
                        if (Manager.instance.InScreen(Player.instance.transform.position + pos))
                        {
                            if (
                                hit.collider.gameObject.tag == "Player" && distance == "Close"
                                || (
                                    distance == "Far"
                                    && !Physics2D.OverlapCircle(
                                        Player.instance.transform.position + pos,
                                        1
                                    )
                                    && hit.collider.gameObject.tag == "Box"
                                )
                            )
                            {
                                dict[(distance, i)] = false;
                                if (oldPos != -1)
                                    dict[(distance, oldPos)] = true;
                                newPos = i;
                                return Player.instance.transform.position + pos;
                            }
                        }
                }
        if (oldPos != -1)
            dict[(distance, oldPos)] = true;
        newPos = -1;
        return new Vector3(0, 0, 0);
    }
}
