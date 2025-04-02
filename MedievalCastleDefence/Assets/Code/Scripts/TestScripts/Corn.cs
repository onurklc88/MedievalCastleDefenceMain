using UnityEngine;
using System.Collections;

public class Corn : MonoBehaviour
{
    [Header("Ayarlar")]
    public float radius = 7f;         // Pasta dilimi yarýçapý
    public float angle = 90f;         // Pasta dilimi açýsý (örneðin 90° = 45° sað + 45° sol)
    public LayerMask obstacleLayer;   // Engel kontrolü için (duvarlar)

    void OnDrawGizmos()
    {
        // 1. Pasta dilimi çiz (kýrmýzý yay)
        DrawViewPieGizmo();

        // 2. Tespit edilen oyuncularý göster (mavi küre)
        DrawDetectedPlayersGizmo();
    }

    void DrawViewPieGizmo()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Yarý saydam kýrmýzý

        // Pasta dilimi kenar çizgileri
        Vector3 forward = transform.forward * radius;
        Vector3 leftDir = Quaternion.Euler(0, -angle / 2, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, angle / 2, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);

        // Yay çizimi (pasta dilimi kenarý)
        int segments = 20;
        Vector3 prevPoint = transform.position + leftDir;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = -angle / 2 + (angle * t);
            Vector3 currentDir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = transform.position + currentDir;
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }

    void DrawDetectedPlayersGizmo()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius);
        Gizmos.color = Color.blue;

        foreach (Collider target in targetsInRadius)
        {
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

            // 1. Açý kontrolü (pasta dilimi içinde mi?)
            if (angleToTarget > angle / 2) continue;

            // 2. Engel kontrolü (Raycast ile)
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            bool hasObstacle = Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleLayer);

            if (!hasObstacle)
            {
                Gizmos.DrawSphere(target.transform.position, 0.5f); // Tespit edilen oyuncu
                Gizmos.DrawLine(transform.position, target.transform.position); // Görüþ hattý
            }
        }
    }

}