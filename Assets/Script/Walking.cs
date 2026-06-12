using UnityEngine;

public class Walker : MonoBehaviour
{
    [Header("Robot_Root")]
    public Transform robotRoot; 

    [Header("Targets (Positions)")]
    public Transform frontLeftLegTarget;
    public Transform frontRightLegTarget;
    public Transform backLeftLegTarget;
    public Transform backRightLegTarget;

    [Header("General Parameters")]
    public float moveSpeed = 1f;
    public float strideMultiplier = 1f; 
    public float footOffset = 0.95f;

    [Header("Animation Curves")]
    public AnimationCurve horizontalCurve; 
    public AnimationCurve verticalCurve;   

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float rayLength = 4f;

    Vector3 flBase, frBase, blBase, brBase;
    float t;
    
    private float initialHeightOffset;
    private Vector3 rootBaseLocalPos; 
    
    // Prevents asymmetrical teleportation on launch
    private float dynamicStride = 0f;

    void Start()
    {
        flBase = frontLeftLegTarget.localPosition;
        frBase = frontRightLegTarget.localPosition;
        blBase = backLeftLegTarget.localPosition;
        brBase = backRightLegTarget.localPosition;

        rootBaseLocalPos = robotRoot.localPosition;

        Vector3 startAvgPos = (frontLeftLegTarget.position + frontRightLegTarget.position + backLeftLegTarget.position + backRightLegTarget.position) * 0.25f;
        initialHeightOffset = Vector3.Dot(robotRoot.position - startAvgPos, transform.up);
    }

    void Update()
    {
        UpdateMaster();

        // Startup smoothing: Targets spread smoothly to prevent jumping
        dynamicStride = Mathf.Lerp(dynamicStride, strideMultiplier, 8f * Time.deltaTime);

        float currentWalkSpeed = moveSpeed / strideMultiplier;
        t += currentWalkSpeed * Time.deltaTime;
        if (t > 2f) t -= 2f; 

        UpdateLeg(frontLeftLegTarget, flBase, 0f);
        UpdateLeg(frontRightLegTarget, frBase, 1f);
        UpdateLeg(backLeftLegTarget, blBase, 1f);
        UpdateLeg(backRightLegTarget, brBase, 0f);

        UpdateRobotRoot();
    }

    void UpdateMaster()
    {
        transform.position += transform.right * moveSpeed * Time.deltaTime;

        // 1. Create a "rake" of 3 forward rays (Center, Left, Right)
        float sensorWidth = 0.8f; // Sensor spacing (adjust based on robot width)
        Vector3 originCenter = transform.position + (transform.right * 0.5f) + (transform.up * 2f);
        
        // Since the robot moves on the Right axis, left/right corresponds to the Forward axis (Z)
        Vector3 originLeft = originCenter + transform.forward * sensorWidth;
        Vector3 originRight = originCenter - transform.forward * sensorWidth;

        int hitCount = 0;
        Vector3 avgNormal = Vector3.zero;
        float avgHeightDist = 0f;

        // Center Ray
        if (Physics.Raycast(originCenter, -transform.up, out RaycastHit hitC, rayLength * 1.5f, groundMask))
        {
            hitCount++;
            avgNormal += hitC.normal;
            avgHeightDist += Vector3.Dot(hitC.point - transform.position, transform.up);
        }
        // Left Ray
        if (Physics.Raycast(originLeft, -transform.up, out RaycastHit hitL, rayLength * 1.5f, groundMask))
        {
            hitCount++;
            avgNormal += hitL.normal;
            avgHeightDist += Vector3.Dot(hitL.point - transform.position, transform.up);
        }
        // Right Ray
        if (Physics.Raycast(originRight, -transform.up, out RaycastHit hitR, rayLength * 1.5f, groundMask))
        {
            hitCount++;
            avgNormal += hitR.normal;
            avgHeightDist += Vector3.Dot(hitR.point - transform.position, transform.up);
        }

        // 2. Apply averages with smoothing
        if (hitCount > 0)
        {
            avgNormal = (avgNormal / hitCount).normalized;
            avgHeightDist /= hitCount;

            // --- ROTATION SMOOTHING ---
            
            // 1. New absolute "Up" is the surface normal
            Vector3 newUp = avgNormal;
            
            // 2. Project current walking direction (Right) onto this new ground plane
            Vector3 newRight = Vector3.ProjectOnPlane(transform.right, newUp).normalized;
            
            // 3. Mathematically calculate the "Forward" axis (Z) 
            // 3D Rule: Cross product of X axis (Right) and Y axis (Up) gives the Z axis (Forward)
            Vector3 newForward = Vector3.Cross(newRight, newUp).normalized;
            
            // 4. Create a clean rotation based purely on these local vectors
            Quaternion targetRot = Quaternion.LookRotation(newForward, newUp);
            
            // Apply smoothing
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 5f * Time.deltaTime); 

            // --- HEIGHT SMOOTHING ---
            Vector3 targetPos = transform.position + transform.up * avgHeightDist;
            transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);
        }
    }

    void UpdateLeg(Transform leg, Vector3 basePos, float phaseOffset)
    {
        float legT = t + phaseOffset;
        if (legT > 2f) legT -= 2f;

        float hEval = horizontalCurve.Evaluate(legT) - 0.5f;
        float vEval = verticalCurve.Evaluate(legT);

        // Use dynamicStride for a smooth start
        Vector3 local = basePos + Vector3.right * (hEval * dynamicStride);
        Vector3 worldPos = transform.TransformPoint(local);

        Vector3 origin = worldPos + transform.up * (rayLength * 0.5f);

        if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, rayLength, groundMask))
        {
            // Push the foot strictly along the robot's up axis (not hit.normal)
            worldPos = hit.point + transform.up * (footOffset + vEval);

            Quaternion targetFootRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            leg.rotation = Quaternion.Lerp(leg.rotation, targetFootRotation, 25f * Time.deltaTime);
        }
        else
        {
            worldPos += transform.up * vEval; 
            leg.rotation = Quaternion.Lerp(leg.rotation, transform.rotation, 25f * Time.deltaTime);
        }

        leg.position = worldPos;
    }

    void UpdateRobotRoot()
    {
        Vector3 avgPos = (
            frontLeftLegTarget.position +
            frontRightLegTarget.position +
            backLeftLegTarget.position +
            backRightLegTarget.position
        ) * 0.25f;

        float legsHeight = Vector3.Dot(avgPos - transform.position, transform.up);

        // The body remains perfectly locked to the center on X and Z axes
        Vector3 targetPos = transform.position 
                            + transform.right * rootBaseLocalPos.x 
                            + transform.forward * rootBaseLocalPos.z 
                            + transform.up * (legsHeight + initialHeightOffset);

        robotRoot.position = Vector3.Lerp(robotRoot.position, targetPos, 15f * Time.deltaTime);

        Vector3 d1 = backRightLegTarget.position - frontLeftLegTarget.position;
        Vector3 d2 = backLeftLegTarget.position - frontRightLegTarget.position;
        Vector3 terrainNormal = Vector3.Cross(d1, d2).normalized;

        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, terrainNormal) * transform.rotation;
        robotRoot.rotation = Quaternion.Lerp(robotRoot.rotation, targetRotation, 15f * Time.deltaTime);
    }
}