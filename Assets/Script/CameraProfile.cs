using UnityEngine;

public class CameraProfileLoop : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Reference to the target transform")]
    public Transform target; 

    [Header("Profile Positioning")]
    [Tooltip("Distance to the side of the robot (negative values switch sides)")]
    public float sideDistance = 8f; 
    [Tooltip("Camera height relative to the robot's back")]
    public float heightOffset = 2f;
    [Tooltip("Offset to frame ahead of the robot (anticipates movement)")]
    public float leadOffset = 1.5f;

    [Header("Smoothing")]
    public float followSpeed = 10f;
    public float rotationSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. LOCAL POSITION CALCULATION ---
        // Position the camera relative to the robot's local axes.
        // Placed to the side (forward), slightly elevated (up), and slightly ahead (right).
        Vector3 targetPos = target.position 
                          + (target.forward * sideDistance) 
                          + (target.up * heightOffset) 
                          + (target.right * leadOffset);

        // Smooth position update
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 2. RELATIVE ROTATION CALCULATION ---
        // Orient the camera to look slightly ahead of the target
        Vector3 lookPoint = target.position + (target.right * leadOffset);
        Vector3 directionToTarget = lookPoint - transform.position;

        // Using 'target.up' instead of 'Vector3.up' ensures the camera maintains the 
        // same local orientation as the robot. This creates a looping effect where 
        // the environment rotates on screen while the robot remains stable.
        Quaternion targetRot = Quaternion.LookRotation(directionToTarget, target.up);

        // Smooth rotation update
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}