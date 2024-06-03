using System.Collections.Generic;
using UnityEngine;

public class ObjectLineManager : MonoBehaviour
{
    public Transform obj1;
    public Transform obj2;
    public LineRenderer lineRenderer;

    private List<SphereManager> spheres = new List<SphereManager>(); // 구체 리스트

    public void Initialize(Transform object1, Transform object2, LineRenderer line)
    {
        obj1 = object1;
        obj2 = object2;
        lineRenderer = line;

        //오브젝트 삭제 이벤트 연결
        obj1.GetComponent<LineManager>().OnObjectRemoved += OnObjectRemoved;
        obj2.GetComponent<LineManager>().OnObjectRemoved += OnObjectRemoved;
        //오브젝트 이동 이벤트 연결
        obj1.GetComponent<LineManager>().OnObjectMoved += OnObjectMoved;
        obj2.GetComponent<LineManager>().OnObjectMoved += OnObjectMoved;
        
    }

    public void RegisterSphere(SphereManager sphere)
    {
        if (!spheres.Contains(sphere))
        {
            spheres.Add(sphere);
        }
    }

    public void UnregisterSphere(SphereManager sphere)
    {
        if (spheres.Contains(sphere))
        {
            spheres.Remove(sphere);
        }
    }

    private void OnObjectRemoved()
    {
        DeleteLineAndSpheres();
    }

    private void OnObjectMoved()
    {
        DeleteLineAndSpheres();
    }

    private void DeleteLineAndSpheres()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
        }

        foreach (var sphere in new List<SphereManager>(spheres))
        {
            if (sphere != null)
            {
                Destroy(sphere.gameObject);
            }
        }
        spheres.Clear();
    }

    private void OnDestroy()
    {
        OnObjectRemoved();
    }
}
