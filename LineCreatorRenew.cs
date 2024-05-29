using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCreatorRenew : MonoBehaviour
{
    public GameObject spherePrefab; // 이동할 구체 프리팹 참조
    public float sphereSpawnInterval = 50f; // 구체 생성 간격
    public float sphereMoveSpeed = 7.0f; // 구체 이동 속도

    private List<LineRenderer> lines = new List<LineRenderer>(); // 모든 라인 저장
    private List<List<Vector3>> linePointsList = new List<List<Vector3>>(); // 각 라인의 점들 저장
    private Dictionary<LineRenderer, Coroutine> lineCoroutines = new Dictionary<LineRenderer, Coroutine>(); // 라인의 코루틴 저장
    private LineRenderer currentLineRenderer;
    private List<Vector3> currentLinePoints = new List<Vector3>();
    private bool isDrawing = false; // 라인 그리기 중인지 여부 확인
    public float lineWidth = 5f;
    public Color lineColor = Color.blue;

    private bool state = false; // 라인 그리기 활성화 여부
    private LineRenderer selectedLine; // 선택된 라인
    private List<Transform> selectedObjects = new List<Transform>(); // 선택된 오브젝트 리스트

    public void onoff()
    {
        state = !state;
        if (state)
        {
            gameObject.GetComponent<LineCreatorRenew>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<LineCreatorRenew>().enabled = false;
        }
    }

    void Update()
    {
        if (state) // 라인 그리기 활성화 시에만 동작
        {
            if (Input.GetMouseButtonDown(1)) // 왼쪽 클릭으로 오브젝트 선택
            {
                SelectObject();
            }

            if (selectedObjects.Count == 2) // 두 개의 오브젝트가 선택되면 라인 생성
            {
                CreateLineBetweenObjects();
                selectedObjects.Clear(); // 선택된 오브젝트 리스트 초기화
            }

            if (Input.GetKeyDown(KeyCode.Delete)) // Delete 키로 선택된 라인 삭제
            {
                DeleteSelectedLine();
            }
        }

        // 라인 선택
        if (Input.GetMouseButtonDown(0) && !isDrawing)
        {
            SelectLine();
        }
    }

    private void SelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform selectedObject = hit.transform;
            if (!selectedObjects.Contains(selectedObject))
            {
                selectedObjects.Add(selectedObject);
            }
        }
    }

    public void CreateLineBetweenObjects()
    {
        Vector3 start = selectedObjects[0].position;
        Vector3 end = selectedObjects[1].position;
        CreateNewLine(start, end);
    }

    public void CreateNewLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        currentLineRenderer = lineObj.AddComponent<LineRenderer>();

        // 기본 제공 셰이더를 사용하는 Material 생성
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        currentLineRenderer.material = lineMaterial;

        currentLineRenderer.startColor = lineColor;
        currentLineRenderer.endColor = lineColor;
        currentLineRenderer.startWidth = lineWidth;
        currentLineRenderer.endWidth = lineWidth;
        currentLineRenderer.positionCount = 2;

        currentLineRenderer.SetPosition(0, start);
        currentLineRenderer.SetPosition(1, end);

        lines.Add(currentLineRenderer);
        currentLinePoints = new List<Vector3> { start, end };
        linePointsList.Add(currentLinePoints);

        // BoxCollider 추가
        BoxCollider boxCollider = lineObj.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        UpdateBoxCollider(currentLineRenderer);

        // 구체 이동 시작
        if (spherePrefab != null)
        {
            Coroutine coroutine = StartCoroutine(SpheresContinuously(currentLineRenderer, currentLinePoints));
            lineCoroutines[currentLineRenderer] = coroutine;
        }
    }

    private void UpdateBoxCollider(LineRenderer lineRenderer)
    {
        BoxCollider boxCollider = lineRenderer.GetComponent<BoxCollider>();
        if (boxCollider != null && lineRenderer.positionCount > 1)
        {
            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);

            Bounds bounds = mesh.bounds;
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
        }
    }

    private IEnumerator SpheresContinuously(LineRenderer lineRenderer, List<Vector3> points)
    {
        while (true)
        {
            if (spherePrefab != null && points.Count > 0)
            {
                GameObject sphere = Instantiate(spherePrefab, points[0], Quaternion.identity);
                StartCoroutine(MoveSphereAlongLine(sphere, points));
            }
            yield return new WaitForSeconds(sphereSpawnInterval);
        }
    }

    private IEnumerator MoveSphereAlongLine(GameObject sphere, List<Vector3> points)
    {
        int index = 0;
        while (index < points.Count)
        {
            Vector3 targetPosition = points[index];
            while (Vector3.Distance(sphere.transform.position, targetPosition) > 0.1f)
            {
                sphere.transform.position = Vector3.MoveTowards(sphere.transform.position, targetPosition, Time.deltaTime * sphereMoveSpeed);
                yield return null;
            }
            index++;
        }
        Destroy(sphere);
    }

    // private Vector3 GetWorldPositionFromMouse()
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     if (Physics.Raycast(ray, out RaycastHit hit))
    //     {
    //         return hit.point;
    //     }
    //     return Vector3.zero;
    // }

    private void SelectLine()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            LineRenderer lineRenderer = hit.collider.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                if (selectedLine != null)
                {
                    // 선택된 라인의 색상을 원래대로 되돌림
                    selectedLine.startColor = lineColor;
                    selectedLine.endColor = lineColor;
                }

                selectedLine = lineRenderer;
                // 선택된 라인의 색상을 변경
                selectedLine.startColor = Color.red;
                selectedLine.endColor = Color.red;
            }
        }
    }

    public void DeleteSelectedLine()
    {
        if (selectedLine != null)
        {
            if (lineCoroutines.ContainsKey(selectedLine))
            {
                StopCoroutine(lineCoroutines[selectedLine]);
                lineCoroutines.Remove(selectedLine);
            }

            lines.Remove(selectedLine);
            Destroy(selectedLine.gameObject);
            selectedLine = null;
        }
    }
}
