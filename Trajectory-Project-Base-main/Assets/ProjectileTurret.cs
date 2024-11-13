using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 10;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;
    [SerializeField] int trajectoryResolution = 30; // Number of points in the trajectory preview

    List<Vector3> points = new List<Vector3>();

    void Start()
    {
        line.positionCount = trajectoryResolution;
    }

    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();

        if (Input.GetButtonDown("Fire1"))
            Fire();

        DrawTrajectoryPath(); // Call to draw the trajectory path
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
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

    void RotateGun()
    {
        Vector3 targetDirection = (crosshair.transform.position - barrelEnd.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;

        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = Mathf.Atan2(v2 + root, g * x);
            float lowAngle = Mathf.Atan2(v2 - root, g * x);

            if (useLow)
                return Mathf.Rad2Deg * lowAngle;
            else
                return Mathf.Rad2Deg * highAngle;
        }
        else
            return null;
    }

    void DrawTrajectoryPath()
    {
        points.Clear();
        Vector3 startPosition = barrelEnd.position;
        Vector3 startVelocity = projectileSpeed * barrelEnd.transform.forward;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * 0.1f; // Adjust time step as needed
            Vector3 point = CalculateTrajectoryPoint(startPosition, startVelocity, time);
            points.Add(point);

            // Stop adding points if it hits something
            if (Physics.Raycast(point, startVelocity.normalized, out RaycastHit hit, 0.1f))
            {
                points.Add(hit.point);
                break;
            }
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    Vector3 CalculateTrajectoryPoint(Vector3 startPosition, Vector3 startVelocity, float time)
    {
        // Using kinematic equation for displacement under constant acceleration
        return startPosition + startVelocity * time + 0.5f * gravity * time * time;
    }
}
