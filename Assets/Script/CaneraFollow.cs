using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Positioning")]
    public float distanceInFront = 6f; 
    [Tooltip("The absolute height of the camera relative to world zero")]
    public float fixedHeight = 3f; 

    [Header("Smoothing")]
    public float followSpeed = 10f;
    public float rotationSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. POSITION CALCULATION ---
        // Position the camera in front of the robot (along its local Right axis)
        Vector3 targetPos = target.position + (target.right * distanceInFront);
        
        // Override the height to lock it to a strict world value.
        // This prevents the camera from bouncing or rising with the robot.
        targetPos.y = fixedHeight; 
        
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 2. ROTATION CALCULATION ---
        Vector3 directionToTarget = target.position - transform.position;
        
        // Use Vector3.up (absolute world up) instead of target.up.
        // This ensures the camera remains perfectly horizontal even if the robot tilts.
        Quaternion targetRot = Quaternion.LookRotation(directionToTarget, Vector3.up);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}