using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class navMeshScript : MonoBehaviour
{
    public NavMeshSurface2d surface;
    GameObject managerObject;
    managerScript manager;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateMesh", 0.0f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (surface.navMeshData != null)
        {
            surface.UpdateNavMesh(surface.navMeshData);
        }
        else
        {
            surface.BuildNavMesh();
        }
    }
}
