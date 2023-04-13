using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class navMeshScript : MonoBehaviour
{
    public NavMeshSurface2d surface;

    void Start()
    {
        InvokeRepeating("UpdateMesh", 0.0f, 0.5f);
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
