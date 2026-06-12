using UnityEngine;

public class CameraSideScroller : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Positioning (Fixed Profile)")]
    [Tooltip("Distance to the side of the robot")]
    public float sideDistance = 10f; 
    [Tooltip("Absolute height relative to the robot's center")]
    public float heightOffset = 1f;

    [Header("Smoothing")]
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. POSITION ---
        // Follow the robot from the side (target.forward)
        // Use Vector3.up for height to ignore the robot's tilt
        Vector3 targetPos = target.position 
                          + (target.forward * sideDistance) 
                          + (Vector3.up * heightOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 2. STRICTLY LOCKED ROTATION ---
        // Look perpendicularly towards the robot
        Vector3 lookDirection = sideDistance > 0 ? -target.forward : target.forward;

        // Vector3.up forces the camera to remain perfectly upright.
        // This ensures the camera stays oriented correctly even if the robot flips upside down.
        Quaternion targetRot = Quaternion.LookRotation(lookDirection, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 15f * Time.deltaTime);
    }
}