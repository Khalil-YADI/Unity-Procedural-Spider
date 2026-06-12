using UnityEngine;

public class CameraFrontAction : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Front Positioning")]
    [Tooltip("Distance placed IN FRONT of the robot")]
    public float distanceInFront = 6f; 
    [Tooltip("Height relative to the robot's center")]
    public float heightOffset = 1.5f;

    [Header("Smoothing")]
    public float followSpeed = 10f;
    public float rotationSpeed = 15f;

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. POSITION ---
        // The robot moves along its local right axis (target.right). 
        // To position the camera in front, it is placed further along this axis.
        // It is elevated using target.up to adjust dynamically with the robot's height.
        Vector3 targetPos = target.position 
                          + (target.right * distanceInFront) 
                          + (target.up * heightOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 2. ROTATION (Follows robot orientation) ---
        // Target a point on the robot (slightly elevated for better framing)
        Vector3 lookPoint = target.position + (target.up * heightOffset);
        Vector3 directionToTarget = lookPoint - transform.position;

        // Using 'target.up' as the "Up" reference ensures the camera 
        // rolls and loops in sync with the robot's orientation.
        Quaternion targetRot = Quaternion.LookRotation(directionToTarget, target.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}