using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct AnchorPoint
{
    public bool active;
    public Transform anchor;
    public Vector3 localPos;

    public int pivotDirection;
}

public class GrappleHookManager : MonoBehaviour
{
    public static GrappleHookManager instance;
    public static UnityAction OnGrappleSuccess;
    public static UnityAction OnGrappleFailure;
    public static UnityAction OnGrappleRetracted;

    public MovementDataSO movementData;
    public Transform grappleOrigin;
    public Transform grappleRopeFolder;
    public LineRenderer ropeRenderer;

    // List<GameObject> ropeSegments;
    List<Vector3> ropePoints;
    List<AnchorPoint> ropeAnchors;
    GameObject hookObj;

    bool deploying = false;
    bool retracting = false;
    bool inUse = false;

    float shotSpeed;
    float retractSpeed;

    Vector2 grappleGoalPoint;
    Vector2 startingPoint;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate Grapple Hook Manager found, deleting");
            Destroy(gameObject);
            return;
        }

        instance = this;

        // ropeSegments = new List<GameObject>();
        ropePoints = new List<Vector3>();
        ropeAnchors = new List<AnchorPoint>();

        shotSpeed = movementData.grappleRange / movementData.grappleShotDuration;
        retractSpeed = movementData.grappleRange / movementData.grappleRetractDuration;
    }

    public void GrappleUpdate(float dt)
    {
        if (!inUse)
        {
            return;
        }

        if (deploying)
        {
            AdvanceLeadRope(Time.unscaledDeltaTime);
        }

        for (int i = 0; i < ropeAnchors.Count; i++)
        {
            UpdateRopeAnchor(i);
        }

        AdvanceBearRope();

        if (retracting)
        {
            Debug.Log("RetrACt");
            RetractLeadRope(retractSpeed * Time.unscaledDeltaTime);
            if (!retracting)
            {
                return;
            }
        }

        UpdateHook();

        for (int i = ropePoints.Count - 1; i >= 0; i--)
        {
            TryRemovePoint(i);
        }

        try
        {
            for (int i = 0; i < ropePoints.Count; i++)
            {
                CheckForNewGeometry(i);
            }
        }
        catch (NoPivotFoundException _e)
        {

        }

        if (RopeLength() > movementData.grappleRange + 0.01f)
        {
            OnGrappleFailure?.Invoke();
        }

        ropePoints.Add(grappleOrigin.position);
        ropeRenderer.positionCount = ropePoints.Count;
        ropeRenderer.SetPositions(ropePoints.ToArray());
        ropePoints.RemoveAt(ropePoints.Count - 1);
    }

    void UpdateRopeAnchor(int anchorInd)
    {
        if (anchorInd < 0 || anchorInd > ropeAnchors.Count - 1)
        {
            return;
        }

        AnchorPoint anchor = ropeAnchors[anchorInd];

        if (!anchor.active)
        {
            return;
        }

        Vector3 newPos = anchor.anchor.TransformPoint(anchor.localPos);

        ropePoints[anchorInd] = newPos;
    }

    void AdvanceLeadRope(float dt)
    {
        // GameObject segment = ropeSegments[1];
        Vector2 end = ropePoints[0];
        Vector2 start;
        if (ropePoints.Count > 1)
        {
            start = ropePoints[1];
        }
        else
        {
            start = grappleOrigin.position;
        }

        Vector2 dir = (grappleGoalPoint - startingPoint).normalized;
        float advanceAmt = shotSpeed * dt;

        Vector2 newEnd = (Vector2)ropePoints[0] + advanceAmt * dir;
        dir = newEnd - end;
        float length = dir.magnitude;
        dir.Normalize();

        RaycastHit2D hit = Physics2D.CircleCast(end, movementData.ropeWidth / 2f, dir, movementData.grappleHeadLength + length, movementData.grappleInteractMask);

        bool grappleHit = false;
        if (hit)
        {
            deploying = false;
            grappleHit = ((1 << hit.collider.gameObject.layer) & movementData.grappleBlockMask) == 0; // make sure we didn't hit blocked material
            length = hit.distance;
            newEnd = end + dir * (hit.distance - 0.01f);
        }

        if (grappleHit)
        {
            OnGrappleSuccess?.Invoke();
            AnchorPoint hookAnchor = ropeAnchors[0];
            hookAnchor.active = true;
            hookAnchor.anchor = hit.transform;
            hookAnchor.localPos = hit.transform.InverseTransformPoint(newEnd);
        }
        else if (hit)
        {
            OnGrappleFailure?.Invoke();
        }

        // segment.transform.position = 0.5f * (start + newEnd);
        // segment.transform.localScale = new Vector3(movementData.ropeWidth, length, 1f);
        // segment.transform.up = dir;

        ropePoints[0] = newEnd;
    }

    void AdvanceBearRope()
    {
        // GameObject segment = ropeSegments[ropeSegments.Count - 1];
        Vector2 origin = grappleOrigin.position;
        Vector2 point = ropePoints[ropePoints.Count - 1];

        Vector2 dir = (point - origin).normalized;
        float length = Vector2.Distance(origin, point);

        // segment.transform.position = 0.5f * (origin + point);
        // segment.transform.localScale = new Vector3(movementData.ropeWidth, length, 1f);
        // segment.transform.up = dir;
    }

    void RetractLeadRope(float retractAmt)
    {
        if (retractAmt < 0f)
        {
            return;
        }

        Vector2 start;
        if (ropePoints.Count < 2)
        {
            start = grappleOrigin.position;
        }
        else
        {
            start = ropePoints[1];
        }

        Vector2 end = ropePoints[0];
        Vector2 dir = start - end;
        float segLength = dir.magnitude;

        if (segLength <= retractAmt)
        {
            RemoveSegment(0);
            if (ropePoints.Count == 0)
            {
                inUse = false;
                retracting = false;
                OnGrappleRetracted?.Invoke();
                ClearRope();
            }
            else
            {
                RetractLeadRope(retractAmt - segLength);
            }
            return;
        }

        dir.Normalize();
        end += dir * retractAmt;
        ropePoints[0] = end;
    }

    void UpdateHook()
    {
        Vector2 end = ropePoints[0];
        Vector2 start;
        if (ropePoints.Count > 1)
        {
            start = ropePoints[1];
        }
        else
        {
            start = grappleOrigin.position;
        }

        Vector2 dir = end - start;
        dir.Normalize();

        hookObj.transform.position = end;
        hookObj.transform.up = dir;
    }

    float RopeLength()
    {
        float len = 0;

        Vector2 lastPoint = grappleOrigin.position;

        for (int i = ropePoints.Count - 1; i >= 0; i--)
        {
            len += Vector2.Distance(lastPoint, ropePoints[i]);
            lastPoint = ropePoints[i];
        }

        return len;
    }

    void CheckForNewGeometry(int pointInd)
    {
        // GameObject lastSegment = ropeSegments[pointInd];
        Vector2 start;
        if (pointInd >= ropePoints.Count - 1)
        {
            start = grappleOrigin.position;
        }
        else
        {
            start = ropePoints[pointInd+1];
        }
        Vector2 end = ropePoints[pointInd];
        Vector2 dir = end - start;
        float length = dir.magnitude;
        dir.Normalize();

        float eps = 0.01f;

        RaycastHit2D hit = Physics2D.CircleCast(start, movementData.ropeWidth / 2f, dir, length - eps, movementData.grappleInteractMask);

        if (!hit)
        {
            return;
        }


        Vector2 newPoint = FindGeometryPoint(hit.collider, start, end);
        AnchorPoint newAnchor = new AnchorPoint();
        newAnchor.active = true;
        newAnchor.anchor = hit.transform;
        newAnchor.localPos = hit.transform.InverseTransformPoint(newPoint);
        newAnchor.pivotDirection = (int)Mathf.Sign(Vector2.SignedAngle(end - start, newPoint - start));
        // Vector2 newDir = end - newPoint;
        // length = newDir.magnitude;
        // newDir.Normalize();
        // 
        // lastSegment.transform.position = 0.5f * (newPoint + end);
        // lastSegment.transform.localScale = new Vector3(movementData.ropeWidth, length, 1f);
        // lastSegment.transform.up = newDir;

        // GameObject newSegment = Instantiate(movementData.ropeSegmentPrefab, grappleRopeFolder);
        // newDir = newPoint - start;
        // length = newDir.magnitude;
        // newDir.Normalize();
        // 
        // newSegment.transform.position = 0.5f * (start + newPoint);
        // newSegment.transform.localScale = new Vector3(movementData.ropeWidth, length, 1f);
        // newSegment.transform.up = newDir;

        if (pointInd >= ropePoints.Count - 1)
        {
            // adding on the end
            // ropeSegments.Add(newSegment);
            ropePoints.Add(newPoint);
            ropeAnchors.Add(newAnchor);
        }
        else
        {
            // adding in the middle
            //ropeSegments.Insert(pointInd + 1, newSegment);
            ropePoints.Insert(pointInd, newPoint);
            ropeAnchors.Insert(pointInd, newAnchor);
        }
        
    }

    // need to define a new point of geometry
    // find which corner will let the rope connect to the start and ends points
    Vector2 FindGeometryPoint(Collider2D blocker, Vector2 start, Vector2 end)
    {
        float eps = 0.01f;

        // first top right
        Vector2 testCorner1 = (Vector2)blocker.bounds.max + (0.5f * movementData.ropeWidth + eps) * Vector2.one;
        float dist1 = float.PositiveInfinity;
        if (TryCorner(start, end, testCorner1))
        {
            dist1 = Vector2.Distance(start, testCorner1) + Vector2.Distance(testCorner1, end);
        }

        // then bottom right
        Vector2 testCorner2 = new Vector2(blocker.bounds.max.x, blocker.bounds.min.y) + (0.5f * movementData.ropeWidth + eps) * new Vector2(1f, -1f);
        float dist2 = float.PositiveInfinity;
        if (TryCorner(start, end, testCorner2))
        {
            dist2 = Vector2.Distance(start, testCorner2) + Vector2.Distance(testCorner2, end);
        }

        // then bottom left
        Vector2 testCorner3 = (Vector2)blocker.bounds.min - (0.5f * movementData.ropeWidth + eps) * Vector2.one;
        float dist3 = float.PositiveInfinity;
        if (TryCorner(start, end, testCorner3))
        {
            dist3 = Vector2.Distance(start, testCorner3) + Vector2.Distance(testCorner3, end);
        }

        // finallow top left
        Vector2 testCorner4 = new Vector2(blocker.bounds.min.x, blocker.bounds.max.y) + (0.5f * movementData.ropeWidth + eps) * new Vector2(-1f, 1f);
        float dist4 = float.PositiveInfinity;
        if (TryCorner(start, end, testCorner4))
        {
            dist4 = Vector2.Distance(start, testCorner4) + Vector2.Distance(testCorner4, end);
        }

        if (!float.IsFinite(dist1) && !float.IsFinite(dist2) && !float.IsFinite(dist3) && !float.IsFinite(dist4))
        {
            Debug.LogWarning("Couldn't find good corner to add rope geometry");
            throw new NoPivotFoundException();
        }

        if (dist1 < dist2 && dist1 < dist3 && dist1 < dist4)
        {
            return testCorner1;
        }

        if (dist2 < dist3 && dist2 < dist4)
        {
            return testCorner2;
        }

        if (dist3 < dist4)
        {
            return testCorner3;
        }

        return testCorner4;
    }

    bool TryCorner(Vector2 start, Vector2 end, Vector2 testPoint)
    {
        float eps = 0.01f;

        Vector2 dir = testPoint - start;
        float length = dir.magnitude;
        dir.Normalize();

        if (Physics2D.CircleCast(start, movementData.ropeWidth / 2f, dir, length - eps, movementData.grappleInteractMask))
        {
            return false;
        }

        dir = testPoint - end;
        length = dir.magnitude;
        dir.Normalize();

        if (Physics2D.CircleCast(end, movementData.ropeWidth / 2f, dir, length - eps, movementData.grappleInteractMask))
        {
            return false;
        }

        return true;
    }

    void TryRemovePoint(int pointInd)
    {
        if (pointInd < 1)
        {
            return;
        }

        AnchorPoint anchor = ropeAnchors[pointInd];
        Vector2 end = ropePoints[pointInd - 1];
        Vector2 midPoint = ropePoints[pointInd];

        if (Vector2.Distance(midPoint, end) < 0.01f)
        {
            RemoveSegment(pointInd);
            return;
        }

        Vector2 start;
        if (pointInd == ropePoints.Count - 1)
        {
            start = grappleOrigin.position;
        }
        else
        {
            start = ropePoints[pointInd + 1];
        }

        int direction = (int)Mathf.Sign(Vector2.SignedAngle(end - start, midPoint - start));
        if (direction != anchor.pivotDirection)
        {
            RemoveSegment(pointInd);
        }
    }

    void RemoveSegment(int pointInd)
    {
        // Destroy(ropeSegments[segmentInd]);
        // ropeSegments.RemoveAt(segmentInd);

        ropePoints.RemoveAt(pointInd);
        ropeAnchors.RemoveAt(pointInd);

        if (pointInd == 0 && ropeAnchors.Count > 0)
        {
            AnchorPoint newAnchor = new AnchorPoint();
            newAnchor.active = false;
            ropeAnchors[0] = newAnchor;
        }
    }

    public void ClearRope()
    {
        // foreach (GameObject segment in ropeSegments)
        // {
        //     Destroy(segment);
        // }
        // 
        // ropeSegments.Clear();
        ropePoints.Clear();
        ropeAnchors.Clear();
        ropeRenderer.positionCount = 0;

        Destroy(hookObj);
        hookObj = null;

        deploying = false;
        retracting = false;
        inUse = false;
    }

    public static void DeployGrappleHook(Vector2 goal)
    {
        instance.ClearRope();

        instance.deploying = true;
        instance.inUse = true;
        instance.startingPoint = instance.grappleOrigin.position;
        instance.grappleGoalPoint = goal;

        GameObject grappleHead = Instantiate(instance.movementData.grappleHeadPrefab, instance.grappleRopeFolder);
        grappleHead.transform.position = instance.grappleOrigin.position;

        // GameObject ropeSegment = Instantiate(instance.movementData.ropeSegmentPrefab, instance.grappleRopeFolder);
        // ropeSegment.transform.localScale = new Vector3(instance.movementData.ropeWidth, 0f, 1f);
        // ropeSegment.transform.position = instance.grappleOrigin.position;

        Vector3 dir = (goal - instance.startingPoint).normalized;

        grappleHead.transform.up = dir;
        // ropeSegment.transform.up = dir;

        //instance.ropeSegments.Add(grappleHead);
        //instance.ropeSegments.Add(ropeSegment);

        instance.hookObj = grappleHead;

        instance.ropePoints.Add(instance.grappleOrigin.position);

        AnchorPoint headAnchor = new AnchorPoint();
        headAnchor.active = false;
        instance.ropeAnchors.Add(headAnchor);
    }

    public static void RetractGrapplingHook()
    {
        if (!instance.inUse)
        {
            OnGrappleRetracted?.Invoke();
            return;
        }

        instance.deploying = false;
        instance.retracting = true;

        AnchorPoint headAnchor = new AnchorPoint();
        headAnchor.active = false;
        instance.ropeAnchors[0] = headAnchor;
    }

    public static float GetRopeLength()
    {
        return instance.RopeLength();
    }

    public static float GetBearSegmentLength()
    {
        if (!instance.inUse)
        {
            return 0f;
        }

        return Vector2.Distance(instance.grappleOrigin.position, instance.ropePoints[instance.ropePoints.Count-1]);
    }

    public static bool GrappleSecured()
    {
        if (instance.ropeAnchors.Count < 1)
        {
            return false;
        }

        return instance.ropeAnchors[0].active;
    }

    public static bool GrappleInUse()
    {
        return instance.inUse;
    }

    public static Vector2 GetBearRopePivot()
    {
        if (instance.ropePoints.Count < 1)
        {
            return Vector2.zero;
        }

        return instance.ropePoints[instance.ropePoints.Count - 1];
    }

    public static void StopDeploy()
    {
        instance.deploying = false;
    }
}

public class NoPivotFoundException : System.Exception
{ }

