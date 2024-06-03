using UnityEngine;
using System;

public class LineManager : MonoBehaviour
{
    public event Action OnObjectRemoved; 
    public event Action OnObjectMoved;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Start()
    {
        lastPosition = transform.position; // 초기 위치
        lastRotation = transform.rotation; // 초기 회전
    }

    private void OnDestroy()
    {
        OnObjectRemoved?.Invoke(); //오브젝트가 삭제될 때 이벤트 호출
    }

    private void Update()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            OnObjectMoved?.Invoke(); //변경될 때 호출
            lastPosition = transform.position; 
            lastRotation = transform.rotation;
        }
    }
}
