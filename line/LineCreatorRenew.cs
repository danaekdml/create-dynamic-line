using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCreatorRenew : MonoBehaviour
{
    public GameObject spherePrefab; // 이동할 구체 프리팹 참조
    public float sphereSpawnInterval = 1f; // 구체 생성 간격
    public float sphereMoveSpeed = 15f; // 구체 이동 속도
    public GameObject garbage; // 부모 오브젝트 참조

    public float lineWidth = 5f;
    public Color blue = Color.blue;
    public Color green = Color.green;
    public Color selectedLineColor; // 기본값을 빨간색으로 설정
    public Color red = Color.red;

    private bool state = false; // 라인 그리기 활성화 여부
    private List<Transform> selectedObjects = new List<Transform>(); // 선택된 오브젝트 리스트

    private List<ObjectLineManager> objectLineManagers = new List<ObjectLineManager>(); // 오브젝트와 라인 매니저 리스트
    private ObjectLineManager selectedLineManager = null; // 선택된 라인 매니저

    public void onoff()
    {
        state = !state;
        gameObject.GetComponent<LineCreatorRenew>().enabled = state;
    }

    void Update()
    {
        if (state) // 라인 그리기 활성화 시에만 동작
        {
            if (Input.GetMouseButtonDown(1))
            {
                SelectObject();
            }

            if (selectedObjects.Count == 2) // 두 개의 오브젝트가 선택되면 라인 생성
            {
                CreateLineBetweenObjects();
                selectedObjects.Clear(); // 선택된 오브젝트 리스트 초기화
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectLine();
        }

        if (Input.GetKeyDown(KeyCode.Delete) && selectedLineManager != null) // Delete 키가 눌렸을 때 선택된 라인 및 구체 삭제
        {
            DeleteSelectedLineAndSpheres();
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

                // 오브젝트에 LineManager 컴포넌트 추가
                if (selectedObject.GetComponent<LineManager>() == null)
                {
                    selectedObject.gameObject.AddComponent<LineManager>();
                }

                //layer 설정
                selectedObject.gameObject.layer = LayerMask.NameToLayer("line");
            }
        }
    }

    private void SelectLine()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("line");
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            LineRenderer selectedLine = hit.transform.GetComponent<LineRenderer>();
            if (selectedLine != null)
            {
                // 기존 선택된 라인 색상 초기화
                if (selectedLineManager != null)
                {
                    selectedLineManager.lineRenderer.startColor = selectedLineColor;
                    selectedLineManager.lineRenderer.endColor = selectedLineColor;
                }

                // 새로운 라인 선택 및 색상 변경
                selectedLineManager = selectedLine.GetComponent<ObjectLineManager>();
                if (selectedLineManager != null)
                {
                    selectedLineManager.lineRenderer.startColor = red;
                    selectedLineManager.lineRenderer.endColor = red;
                }
            }
        }
    }

    private void CreateLineBetweenObjects()
    {
        Vector3 start = selectedObjects[0].position;
        Vector3 end = selectedObjects[1].position;
        CreateNewLine(selectedObjects[0], selectedObjects[1], start, end);
    }

    private void CreateNewLine(Transform obj1, Transform obj2, Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = garbage.transform; // 라인을 garbage의 자식으로 설정
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        // 기본 제공 셰이더를 사용하는 Material 생성
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = lineMaterial;

        lineRenderer.startColor = selectedLineColor;
        lineRenderer.endColor = selectedLineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // BoxCollider 추가
        BoxCollider boxCollider = lineObj.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        UpdateBoxCollider(lineRenderer);

        // ObjectLineManager 설정
        ObjectLineManager objectLineManager = lineObj.AddComponent<ObjectLineManager>();
        objectLineManager.Initialize(obj1, obj2, lineRenderer);
        objectLineManagers.Add(objectLineManager);

        //라인오브젝트 layer -> line
        lineObj.layer = LayerMask.NameToLayer("line");

        // 구체 이동 시작
        if (spherePrefab != null)
        {
            StartCoroutine(SpheresContinuously(lineRenderer, objectLineManager));
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

    private IEnumerator SpheresContinuously(LineRenderer lineRenderer, ObjectLineManager objectLineManager)
    {
        while (lineRenderer != null)
        {
            if (spherePrefab != null)
            {
                GameObject sphere = Instantiate(spherePrefab, lineRenderer.GetPosition(0), Quaternion.identity);
                sphere.transform.parent = garbage.transform; // 구체를 garbage의 자식으로 설정
                SphereManager sphereManager = sphere.AddComponent<SphereManager>();
                sphereManager.lineRenderer = lineRenderer;
                sphereManager.lineCreatorRenew = this;
                sphereManager.Initialize(objectLineManager);
                sphereManager.StartMoving();
            }
            yield return new WaitForSeconds(sphereSpawnInterval);
        }
    }

    public void DeleteSelectedLineAndSpheres()
    {
        if (selectedLineManager != null)
        {
            Destroy(selectedLineManager.gameObject);
            selectedLineManager = null;
        }
    }

    // 라인 색상을 초록색으로 변경
    public void SetLineColorGreen()
    {
        selectedLineColor = green;
    }

    // 라인 색상을 파란색으로 변경
    public void SetLineColorBlue()
    {
        selectedLineColor = blue;
    }
}
