using System.Collections;
using UnityEngine;

public class SphereManager : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LineCreatorRenew lineCreatorRenew;
    private ObjectLineManager objectLineManager;

    public void Initialize(ObjectLineManager lineManager)
    {
        objectLineManager = lineManager;
        objectLineManager.RegisterSphere(this);
    }

    public void StartMoving()
    {
        StartCoroutine(MoveAlongLine());
    }

    private IEnumerator MoveAlongLine()
    {
        int index = 0;
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        while (index < positions.Length)
        {
            Vector3 targetPosition = positions[index];
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, lineCreatorRenew.sphereMoveSpeed * Time.deltaTime);
                yield return null;
            }
            index++;
        }

        Destroy(gameObject);
        objectLineManager.UnregisterSphere(this);
    }

    private void OnDestroy()
    {
        if (objectLineManager != null)
        {
            objectLineManager.UnregisterSphere(this);
        }
    }
}
