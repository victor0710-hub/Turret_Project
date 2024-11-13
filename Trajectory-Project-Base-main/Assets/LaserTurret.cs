using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] int maxBounces = 3; // Maximum number of bounces

    List<Vector3> laserPoints = new List<Vector3>();

    void Start()
    {

    }

    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        CastLaser(barrelEnd.position, barrelEnd.forward, maxBounces);

        line.positionCount = laserPoints.Count;
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void CastLaser(Vector3 startPosition, Vector3 direction, int remainingBounces)
    {
        if (remainingBounces <= 0) return;

        if (Physics.Raycast(startPosition, direction, out RaycastHit hit, 1000.0f, targetLayer))
        {
            laserPoints.Add(hit.point);

            // Calculate the reflection direction
            Vector3 incomingDirection = direction.normalized;
            Vector3 normal = hit.normal;
            Vector3 reflection = incomingDirection - 2 * Vector3.Dot(incomingDirection, normal) * normal;

            // Recursively cast the laser in the reflection direction
            CastLaser(hit.point, reflection, remainingBounces - 1);
        }
    }
}
