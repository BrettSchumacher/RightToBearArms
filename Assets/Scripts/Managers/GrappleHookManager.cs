using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnchorPoint
{
    public bool active;
    public Transform anchor;
    public Vector3 localPos;
    public Vector3 prevPos;

    public int pivotDirection;

    public Vector3 GetPosition()
    {
        return anchor.TransformPoint(localPos);
    }

    public void UpdatePrevPos(Vector3 pos)
    {
        prevPos = pos;
    }
}

public class GrappleHookManager : MonoBehaviour
{
    public static GrappleHookManager instance;
    public static UnityAction OnGrappleSuccess;
    public static UnityAction OnGrappleFailure;
    public static UnityAction OnGrappleRetracted;

    public MovementDataSO movementData;
    public Transform grappleOrigin;
    public Collider2D grappleClearZone;
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

        int frame = Time.frameCount;

        // Debug.Log("Updating anchors: " + frame);
        for (int i = 0; i < ropeAnchors.Count; i++)
        {
            UpdateRopeAnchor(i);
        }

        if (retracting)
        {
            RetractLeadRope(retractSpeed * Time.unscaledDeltaTime);
            if (!retracting)
            {
                return;
            }
        }

        UpdateHook();

        // Debug.Log("Trying remove points: " + frame);
        for (int i = ropePoints.Count - 2; i >= 1; i--)
        {
            TryRemovePoint(i);
        }

        // Debug.Log("Checking for new geometry: " + frame);
        try
        {
            int maxNewGeo = 5;
            for (int i = 0; i < ropePoints.Count - 1; i++)
            {
                if (CheckForNewGeometry(i))
                {
                    maxNewGeo--;
                }

                if (maxNewGeo <= 0)
                {
                    break;
                }
            }
        }
        catch (NoPivotFoundException _e)
        {

        }

        if (RopeLength() > movementData.grappleRange + 0.01f)
        {
            OnGrappleFailure?.Invoke();
        }

        // Debug.Log("Updating line renderer: " + frame);

        ropeRenderer.positionCount = ropePoints.Count;
        ropeRenderer.SetPositions(ropePoints.ToArray());

        for (int i = 0; i < ropeAnchors.Count; i++)
        {
            ropeAnchors[i].UpdatePrevPos(ropePoints[i]);
        }

        // Debug.Log("Done! : " + frame);
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

        ropePoints[anchorInd] = anchor.GetPosition();
    }

    void AdvanceLeadRope(float dt)
    {
        Vector2 end = ropePoints[0];
        Vector2 start = ropePoints[1];

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
            newEnd = end + dir * Mathf.Max(length - movementData.grappleHeadLength, 0f);
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

        ropePoints[0] = newEnd;
    }

    void RetractLeadRope(float retractAmt)
    {
        if (retractAmt < 0f)
        {
            return;
        }

        Vector2 start = ropePoints[1];
        Vector2 end = ropePoints[0];
        Vector2 dir = start - end;
        float segLength = dir.magnitude;

        if (segLength <= retractAmt)
        {
            RemoveSegment(0);
            if (ropePoints.Count == 1)
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
        Vector2 start = ropePoints[1];

        Vector2 dir = end - start;
        dir.Normalize();

        hookObj.transform.position = end;
        hookObj.transform.up = dir;
    }

    float RopeLength()
    {
        float len = 0;

        for (int i = 1; i < ropePoints.Count; i++)
        {
            len += Vector2.Distance(ropePoints[i-1], ropePoints[i]);
        }

        return len;
    }

    bool CheckForNewGeometry(int pointInd)
    {
        // get points this frame
        Vector2 fromCur = ropePoints[pointInd]; // point closer to rope hook
        Vector2 toCur = ropePoints[pointInd + 1]; // point closer to bear

        // get points last frame
        Vector2 fromPrev = ropeAnchors[pointInd].prevPos;
        Vector2 toPrev = ropeAnchors[pointInd + 1].prevPos;

        // store points from last check
        Vector2 lastFrom = fromPrev;
        Vector2 lastTo = toPrev;

        float dist = Vector2.Distance(fromCur, fromPrev) + Vector2.Distance(toCur, toPrev); // total displacement of the anchor points for this section of rope
        float remaining = dist;
        int max = 100;
        bool added = false;
        while (remaining > 0f)
        { 
            if (max-- < 0)
            {
                print("YIKES");
                return false;
            }
            float curAmt = Mathf.Min(remaining, movementData.grappleMaxMoveDelta); // amount to move for this update
            remaining -= curAmt;
            float t = 1f - remaining / dist; // interp param
            t = Mathf.Clamp01(t);

            Vector2 from = Vector2.Lerp(fromPrev, fromCur, t);
            Vector2 to = Vector2.Lerp(toPrev, toCur, t);

            // only add 1 point of geometry
            if (CheckForNewGeometryHelper(pointInd, from, to, lastFrom, lastTo))
            {
                added = true;
                break;
            }

            lastFrom = from;
            lastTo = to;
        }

        return added;
    }

    // returns if new geometry was added due to swing between last from/to and current from/to
    bool CheckForNewGeometryHelper(int pointInd, Vector2 from, Vector2 to, Vector2 lastFrom, Vector2 lastTo)
    {
        Vector2 dir = to - from;
        float length = dir.magnitude;
        dir.Normalize();

        float eps = 0.05f;
        float rad = movementData.ropeWidth / 2f;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(from, rad, dir, length, movementData.grappleInteractMask);

        if (hits.Length == 0)
        {
            return false;
        }

        // Debug.Break();
        Collider2D[] colliders = new Collider2D[hits.Length];
        print("Hit the following colliders: ");
        for (int i = 0; i < hits.Length; i++)
        {
            print("   " + hits[i].collider.name);
            colliders[i] = hits[i].collider;
        }

        Transform anchorTransform;
        Vector2 newPoint = FindGeometryPoint(colliders, from, to, lastFrom, lastTo, out anchorTransform);

        AnchorPoint newAnchor = new AnchorPoint();
        newAnchor.active = true;
        newAnchor.anchor = anchorTransform;
        newAnchor.localPos = anchorTransform.InverseTransformPoint(newPoint);
        newAnchor.pivotDirection = (int)Mathf.Sign(Vector2.SignedAngle(from - to, newPoint - to));
        newAnchor.prevPos = newPoint;

        ropePoints.Insert(pointInd + 1, newPoint);
        ropeAnchors.Insert(pointInd + 1, newAnchor);

        print("Adding point at: " + newPoint);

        return true;
    }

    // need to define a new point of geometry
    // find which corner will let the rope connect to the start and ends points
    Vector2 FindGeometryPoint(Collider2D[] blockers, Vector2 from, Vector2 to, Vector2 lastFrom, Vector2 lastTo, out Transform anchorTransform)
    {
        float eps = 0.05f;
        Vector2 dir = to - from;
        Debug.DrawLine(from, to, Color.blue);
        Debug.DrawLine(lastFrom, lastTo, Color.red);
        float movementSign = Mathf.Sign(Vector2.SignedAngle(lastTo - lastFrom, dir));
        print("Initial Sign: " + movementSign);

        if (movementSign == 0f)
        {
            print("WHEW");
            throw new NoPivotFoundException();
        }

        List<Vector2> corners = new List<Vector2>();
        List<float> dists = new List<float>();

        Vector2 testCorner;

        foreach (Collider2D blocker in blockers)
        {
            corners.Add((Vector2)blocker.bounds.max + (0.5f * movementData.ropeWidth + eps) * Vector2.one);
            corners.Add(new Vector2(blocker.bounds.max.x, blocker.bounds.min.y) + (0.5f * movementData.ropeWidth + eps) * new Vector2(1f, -1f));
            corners.Add((Vector2)blocker.bounds.min - (0.5f * movementData.ropeWidth + eps) * Vector2.one);
            corners.Add(new Vector2(blocker.bounds.min.x, blocker.bounds.max.y) + (0.5f * movementData.ropeWidth + eps) * new Vector2(-1f, 1f));
        }

        for (int i = 0; i < corners.Count; i++)
        {
            testCorner = corners[i];
            if (Physics2D.OverlapPoint(testCorner, movementData.grappleInteractMask) || Vector2.Distance(from, testCorner) < movementData.minSegLength)
            {
                // corner is overlapping a collider
                print("AHHHH");
                dists.Add(-1f);
                continue;
            }

            Vector2 lineDir = to - from;
            Vector2 pointDir = testCorner - from;

            if (Vector2.Dot(lineDir, pointDir) < 0f)
            {
                dists.Add(-1f);
                continue;
            }

            Vector2 curLineToCorner = GetVectorToPointFromLine(from, to, testCorner);
            Vector2 lastLineToCorner = GetVectorToPointFromLine(lastFrom, lastTo, testCorner);

            if (Vector2.Dot(curLineToCorner, lastLineToCorner) > 0f && curLineToCorner.sqrMagnitude < lastLineToCorner.sqrMagnitude) // wrong side of the line
            {
                dists.Add(-1f);
            }
            else
            {
                dists.Add(curLineToCorner.sqrMagnitude);
                Debug.DrawLine(from, testCorner, Color.green);
            }
        }

        if (dists.Count < 1)
        {
            throw new NoPivotFoundException();
        }

        float maxDist = 0f;
        Vector2 maxCorner = Vector2.zero;
        int bestIndex = 0;

        for (int i = 0; i < dists.Count; i++)
        {
            if (dists[i] > maxDist && Mathf.Abs(dists[i]) < 90f)
            {
                maxDist = dists[i];
                maxCorner = corners[i];
                bestIndex = i / 4;
            }
        }

        if (maxDist <= 0f)
        {
            throw new NoPivotFoundException();
        }

        anchorTransform = blockers[bestIndex].transform;
        return maxCorner;
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

    Vector2 GetVectorToPointFromLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 toPoint = point - lineStart;
        Vector2 dir = (lineEnd - lineStart).normalized;

        float projLen = Vector2.Dot(toPoint, dir);
        Vector2 projectedPoint = lineStart + dir * projLen;

        Debug.DrawLine(projectedPoint, point, Color.magenta);

        return point - projectedPoint;
    }

    void TryRemovePoint(int pointInd)
    {
        if (pointInd < 1 || pointInd >= ropePoints.Count - 1)
        {
            return;
        }

        AnchorPoint anchor = ropeAnchors[pointInd];
        Vector2 next = ropePoints[pointInd + 1];
        Vector2 prev = ropePoints[pointInd - 1];
        Vector2 cur = ropePoints[pointInd];

        if (Vector2.Distance(cur, next) < movementData.minSegLength)
        {
            RemoveSegment(pointInd);
            return;
        }

        float angle = Vector2.SignedAngle(prev - next, cur - next);
        int direction = (int)Mathf.Sign(angle);
        if (direction != anchor.pivotDirection)
        {
            Vector2 cornerToLine = GetVectorToPointFromLine(prev, next, cur);
            if (cornerToLine.magnitude < 0.025f)
            {
                return;
            }
            RemoveSegment(pointInd);
        }
    }

    void RemoveSegment(int pointInd)
    {
        Vector2 lastPos = ropeAnchors[pointInd].prevPos;
        ropePoints.RemoveAt(pointInd);
        ropeAnchors.RemoveAt(pointInd);

        if (pointInd == 0 && ropeAnchors.Count > 0)
        {
            AnchorPoint newAnchor = new AnchorPoint();
            newAnchor.active = false;
            newAnchor.prevPos = ropeAnchors[0].prevPos;
            ropeAnchors[0] = newAnchor;
        }
    }

    public void ClearRope()
    {
        ropePoints.Clear();
        ropeAnchors.Clear();
        ropeRenderer.positionCount = 0;

        if (hookObj != null)
        {
            Destroy(hookObj);
            hookObj = null;
        }

        deploying = false;
        retracting = false;
        inUse = false;
    }

    public static void DeployGrappleHook(Vector2 goal)
    {
        instance.ClearRope();

        instance.retracting = false;
        instance.deploying = true;
        instance.inUse = true;
        instance.startingPoint = instance.grappleOrigin.position;
        instance.grappleGoalPoint = goal;

        GameObject grappleHead = Instantiate(instance.movementData.grappleHeadPrefab, instance.grappleRopeFolder);
        grappleHead.transform.position = instance.grappleOrigin.position;

        Vector3 dir = (goal - instance.startingPoint).normalized;

        grappleHead.transform.up = dir;

        instance.hookObj = grappleHead;

        instance.ropePoints.Add(instance.grappleOrigin.position);
        instance.ropePoints.Add(instance.grappleOrigin.position);

        AnchorPoint headAnchor = new AnchorPoint();
        headAnchor.active = false;
        headAnchor.prevPos = instance.ropePoints[0];
        AnchorPoint bearAnchor = new AnchorPoint();
        bearAnchor.anchor = instance.grappleOrigin;
        bearAnchor.prevPos = bearAnchor.GetPosition();
        bearAnchor.active = true;
        instance.ropeAnchors.Add(headAnchor);
        instance.ropeAnchors.Add(bearAnchor);
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
        headAnchor.prevPos = instance.ropePoints[0];
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

        int numPoints = instance.ropePoints.Count;
        Vector2 bear = instance.ropePoints[numPoints - 1];
        Vector2 nearest = instance.ropePoints[numPoints - 2];

        return Vector2.Distance(bear, nearest);
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

        return instance.ropePoints[instance.ropePoints.Count - 2];
    }

    public static void StopDeploy()
    {
        instance.deploying = false;
    }

    public static void ResetRope()
    {
        instance.ClearRope();
    }
}

public class NoPivotFoundException : System.Exception
{ }

