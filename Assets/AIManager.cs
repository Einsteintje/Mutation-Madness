using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance { get; private set; }
    GameObject player;

    public Dictionary<int, bool> dict = new Dictionary<int, bool>();
    List<int> list = new List<int>();
    public int spots = 10;
    int radius = 10;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        for (int i = 0; i < spots; i++)
        {
            dict[i] = true;
            list.Add(i);
        }
    }

    public Vector3 GetPos(int oldPos, out int newPos)
    {
        list.Shuffle();
        for (int j = 1; j < 4; j++)
        {
            foreach (int i in list)
            {
                if (dict[i])
                {
                    Vector3 pos = new Vector3(
                        Mathf.Cos(Mathf.Deg2Rad * 360 / spots * i) * radius / j,
                        Mathf.Sin(Mathf.Deg2Rad * 360 / spots * i) * radius / j,
                        0
                    );
                    RaycastHit2D hit = Physics2D.Linecast(
                        player.transform.position + pos,
                        player.transform.position
                    );
                    if (hit)
                    {
                        if (
                            hit.collider.gameObject.tag == "Player"
                            && Manager.instance.InScreen(player.transform.position + pos)
                        )
                        {
                            dict[i] = false;
                            if (oldPos != -1)
                                dict[oldPos] = true;
                            newPos = i;
                            return player.transform.position + pos;
                        }
                    }
                }
            }
        }
        if (oldPos != -1)
            dict[oldPos] = true;
        newPos = -1;
        return new Vector3(0, 0, 0);
    }
}
