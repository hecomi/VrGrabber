using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace VrGrabber
{

[RequireComponent(typeof(Rigidbody))]
public class VrgGrabber : MonoBehaviour
{
    const float grabBeginThreshold = 0.55f;
    const float grabEndThreshold = 0.35f;
    const float minGrabSmoothDist = 0.5f;
    const float maxGrabSmoothDist = 2f;
    const float minGrabSmoothFilter = 0.15f;

    public ControllerSide side = ControllerSide.Left;

    public bool isLeft
    {
        get { return side == ControllerSide.Left; }
    }

    public bool isRight
    {
        get { return side == ControllerSide.Right; }
    }

    [SerializeField]
    Transform grip = null;

    [SerializeField]
    VrgTargetLine line;

    [SerializeField]
    float maxGrabDistance = 10f;

    [SerializeField]
    float stickMoveSpeed = 0.1f;

    [SerializeField]
    LayerMask layerMask = ~0;

    public class TargetClickEvent : UnityEvent<VrgGrabber, RaycastHit> {}
    public TargetClickEvent onTargetClicked = new TargetClickEvent();

    internal class AverageVelocity
    {
        private const int n = 3;
        private Vector3[] velocities_ = new Vector3[n];
        private int index_ = 0;

        public Vector3 average
        {
            get
            {
                var a = Vector3.zero;
                for (int i = 0; i < n; ++i)
                {
                    a += velocities_[i];
                }
                return a / n;
            }
        }

        public void Add(Vector3 velocity)
        {
            velocities_[index_] = velocity;
            index_ = (index_ + 1) % n;
        }
    }

    internal class GrabInfo
    {
        public int id = -1;
        public VrgGrabber grabber = null;
        public VrgGrabbable grabbable = null;
        public Matrix4x4 initGripToGrabbableMat;
        public Matrix4x4 initGrabbableToGrabMat;
        public float distance = 0f;
        public AverageVelocity velocity = new AverageVelocity();
        public bool isKinematic = false;
        public float smoothFilter = 0f;
        public float stickMove = 0f;

        public Matrix4x4 grabMat
        {
            get
            {
                var transMat = Matrix4x4.Translate(new Vector3(0, 0, distance));
                return grabber.gripTransform.localToWorldMatrix * transMat;
            }
        }

        public Matrix4x4 gripToGrabbableMat
        {
            get
            {
                return grabMat * initGripToGrabbableMat;
            }
        }
    }
    GrabInfo grabInfo_ = new GrabInfo();

    internal class DualGrabInfo
    {
        public Vector3 primaryToSecondary;
        public Vector3 pos;
        public Vector3 center;
        public Vector3 scale;
        public Quaternion rot;
    }
    DualGrabInfo dualGrabInfo_ = new DualGrabInfo();

    internal class CandidateInfo
    {
        public VrgGrabbable grabbable;
        public Collider collider;
        public int refCount = 0;
    }
    Dictionary<Collider, CandidateInfo> directGrabCandidates_ = new Dictionary<Collider, CandidateInfo>();

    RaycastHit targetHit_;
    bool holdInput_ = false;
    bool releaseInput_ = false;
    bool isHoldStart_ = false;
    bool isHoleEnd_ = false;
    Vector3 preRayDirection_;

    public Transform gripTransform
    {
        get { return grip ? grip : transform; }
    }

    public Vector3 gripDir
    {
        get { return gripTransform.forward; }
    }

    public Vector3 targetPos
    {
        get
        {
            if (isGrabbing)
            {
                var grabMat = grabInfo_.grabbable.transform.localToWorldMatrix;
                return (grabMat * grabInfo_.initGrabbableToGrabMat).GetPosition();
            }
            else if (targetHit_.transform)
            {
                return targetHit_.point;
            }
            else if (directGrabCandidates_.Count > 0)
            {
                return gripTransform.position;
            }
            else
            {
                return gripTransform.position + preRayDirection_ * maxGrabDistance;
            }
        }
    }

    public bool isGrabbing
    {
        get { return grabInfo_.grabbable != null; }
    }

    VrgGrabber opposite
    {
        get
        {
            if (!isGrabbing)
            {
                return null;
            }
            return grabInfo_.grabbable.grabbers.Find(grabber => grabber != this);
        }
    }

    public bool isPrimary
    {
        get
        {
            if (!isGrabbing) return false;

            if (!grabInfo_.grabbable.isMultiGrabbed) return true;

            return grabInfo_.id < opposite.grabInfo_.id;
        }
    }

    void Update()
    {
        UpdateInput();
        UpdateGrab();
    }

    void LateUpdate()
    {
        UpdateTransform();

        if (!isGrabbing)
        {
            UpdateTouch();
        }
    }

    void FixedUpdate()
    {
        if (isGrabbing)
        {
            FixedUpdateGrabbingObject();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        var grabbable =
            collider.GetComponent<VrgGrabbable>() ??
            collider.GetComponentInParent<VrgGrabbable>();
        if (!grabbable) return;

        CandidateInfo info;
        if (!directGrabCandidates_.TryGetValue(collider, out info))
        {
            info = new CandidateInfo();
            info.collider = collider;
            info.grabbable = grabbable;
            directGrabCandidates_.Add(collider, info);
        }
        info.refCount++;
    }

    void OnTriggerExit(Collider collider)
    {
        CandidateInfo info = null;
        if (!directGrabCandidates_.TryGetValue(collider, out info)) return;

        info.refCount--;
        if (info.refCount <= 0)
        {
            directGrabCandidates_.Remove(collider);
        }
    }

    void UpdateTransform()
    {
        transform.localPosition = Device.instance.GetLocalPosition(side);
        transform.localRotation = Device.instance.GetLocalRotation(side);
    }

    void UpdateInput()
    {
        var preHoldInput = holdInput_;
        holdInput_ = Device.instance.GetHold(side);
        releaseInput_ = Device.instance.GetRelease(side);
        isHoldStart_ = holdInput_ == true;
        isHoleEnd_ = releaseInput_ == true;
        //isHoldStart_ = (holdInput_ >= grabBeginThreshold) && (preHoldInput < grabBeginThreshold);
        //isHoleEnd_ = (holdInput_ <= grabEndThreshold) && (preHoldInput > grabEndThreshold);
    }

    void UpdateGrab()
    {
        if (isHoldStart_)
        {
            DirectGrab();
            RemoteGrab();
        }
        else if (isHoleEnd_)
        {
            Release();
        }
    }

    void UpdateTouch()
    {
        var forward = gripTransform.forward;

        var ray = new Ray();
        ray.origin = gripTransform.position;
        ray.direction = Vector3.Lerp(preRayDirection_, forward, 0.25f);

        targetHit_ = new RaycastHit();
        bool hit = Physics.Raycast(ray, out targetHit_, maxGrabDistance, layerMask);
        preRayDirection_ = hit ? ray.direction : forward;
    }

    void Grab(VrgGrabbable grabbable, float distance)
    {
        grabInfo_.grabber = this;
        grabInfo_.grabbable = grabbable;
        grabInfo_.distance = distance;
        var grabMat = grabInfo_.grabMat;
        grabInfo_.initGripToGrabbableMat = grabMat.inverse * grabbable.transform.localToWorldMatrix;
        grabInfo_.initGrabbableToGrabMat = grabbable.transform.worldToLocalMatrix * grabMat;
        grabInfo_.isKinematic = grabbable.rigidbody.isKinematic;

        if (!grabbable.avoidIntersection)
        {
            grabbable.rigidbody.isKinematic = true;
        }

        if (grabbable.isGrabbed)
        {
            SecondGrab(grabbable);
        }

        grabInfo_.id = grabbable.OnGrabbed(this);
    }

    void DirectGrab()
    {
        if (isGrabbing || directGrabCandidates_.Count == 0) return;

        VrgGrabbable grabbable = null;
        float minDist = float.MaxValue;

        var gripPos = gripTransform.position;
        foreach (var kv in directGrabCandidates_)
        {
            var candidate = kv.Value;
            var pos = candidate.collider.ClosestPoint(gripPos);
            var dist = Vector3.Distance(gripPos, pos);

            if (dist < minDist)
            {
                grabbable = candidate.grabbable;
                minDist = dist;
            }
        }

        if (grabbable)
        {
            Grab(grabbable, 0f);
        }
    }

    void RemoteGrab()
    {
        if (isGrabbing) return;

        var ray = new Ray();
        ray.origin = gripTransform.position;
        ray.direction = gripTransform.forward;
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, maxGrabDistance, layerMask))
        {
            return;
        }

        var grabbable =
            hit.collider.GetComponent<VrgGrabbable>() ??
            hit.collider.GetComponentInParent<VrgGrabbable>();

        if (grabbable)
        {
            Grab(grabbable, hit.distance);
        }
    }

    void SecondGrab(VrgGrabbable grabbable)
    {
        var primary = opposite;
        var secondary = this;

        var primaryMat = primary.grabInfo_.gripToGrabbableMat;
        var secondaryMat = secondary.grabInfo_.gripToGrabbableMat;
        var primaryPos = primaryMat.GetPosition();
        var secondaryPos = secondaryMat.GetPosition();
        var primaryGripPos = primary.gripTransform.position;
        var secondaryGripPos = secondary.gripTransform.position;

        primary.dualGrabInfo_.primaryToSecondary = primaryGripPos - secondaryGripPos;
        primary.dualGrabInfo_.pos = grabbable.transform.position;
        primary.dualGrabInfo_.center = (primaryPos + secondaryPos) / 2;
        primary.dualGrabInfo_.rot = grabbable.transform.rotation;
        primary.dualGrabInfo_.scale = grabbable.transform.localScale;

        grabInfo_.isKinematic = primary.grabInfo_.isKinematic;
    }

    void Release()
    {
        if (!isGrabbing) return;

        var grabbable = grabInfo_.grabbable;

        Assert.IsTrue(grabbable.isGrabbed);

        grabbable.velocity = grabInfo_.velocity.average;
        grabbable.OnReleased(this);

        if (grabbable.isGrabbed)
        {
            // opposite.ReGrab();
        }
        else
        {
            grabbable.rigidbody.isKinematic = grabInfo_.isKinematic;
        }

        grabInfo_ = new GrabInfo();
    }

    void ReGrab()
    {
        var grabbable = grabInfo_.grabbable;
        if (!grabbable) return;

        var grabMat = grabInfo_.grabMat;
        grabInfo_.initGripToGrabbableMat = grabMat.inverse * grabbable.transform.localToWorldMatrix;
    }

    void FixedUpdateGrabbingObject()
    {
        if (Device.instance.GetClick(side))
        {
            grabInfo_.grabbable.OnGrabClicked(this);
        }

        if (grabInfo_.grabbable.isMultiGrabbed)
        {
            FixedUpdateGrabbingObjectByDualHand();
        }
        else
        {
            FixedUpdateGrabbingObjectBySingleHand();
        }
    }

    void FixedUpdateGrabbingObjectBySingleHand()
    {
        var grabbable = grabInfo_.grabbable;

        var stickY = Device.instance.GetCoord(side).y;
        var stickMove = stickY * stickMoveSpeed;
        var stickMoveFilter = stickY > Mathf.Epsilon ? 0.1f : 0.3f;
        grabInfo_.stickMove += (stickMove - grabInfo_.stickMove) * stickMoveFilter;

        var dist = Mathf.Clamp(grabInfo_.distance + grabInfo_.stickMove, 0f, maxGrabDistance);
        var actualDist = (targetPos - gripTransform.position).magnitude;
        var deltaDist = dist - actualDist;
        var threshDist = Mathf.Max(dist * 0.1f, 0.1f);
        if (Mathf.Abs(deltaDist) > threshDist)
        {
            dist = Mathf.Lerp(grabInfo_.distance, actualDist, 0.05f);
        }
        grabInfo_.distance = dist;

        var mat = grabInfo_.gripToGrabbableMat;
        var pos = mat.GetPosition();
        var rot = mat.GetRotation();

        FixedUpdateGrabbingObjectTransform(pos, rot, grabbable.transform.localScale);
    }

    void FixedUpdateGrabbingObjectByDualHand()
    {
        if (!isPrimary) return;

        var secondary = opposite;
        Assert.IsNotNull(secondary);

        var primaryGripPos = gripTransform.position;
        var primaryGripRot = gripTransform.rotation;
        var secondaryGripPos = secondary.gripTransform.position;
        var secondaryGripRot = secondary.gripTransform.rotation;

        var primaryMat = grabInfo_.gripToGrabbableMat;
        var secondaryMat = secondary.grabInfo_.gripToGrabbableMat;
        var primaryPos = primaryMat.GetPosition();
        var secondaryPos = secondaryMat.GetPosition();

        var center = (primaryPos + secondaryPos) / 2;
        var dCenter = center - dualGrabInfo_.center;
        var pos = dualGrabInfo_.pos + dCenter;

        var primaryToSecondary = primaryGripPos - secondaryGripPos;
        var currentDir = primaryToSecondary.normalized;
        var initDir = dualGrabInfo_.primaryToSecondary.normalized;
        var dRot = Quaternion.FromToRotation(initDir, currentDir);
        var rot = dRot * dualGrabInfo_.rot;

        var scale = dualGrabInfo_.scale;
        if (grabInfo_.grabbable.isScalable)
        {
            var currentDistance = primaryToSecondary.magnitude;
            var initDistance = dualGrabInfo_.primaryToSecondary.magnitude;
            scale *= currentDistance / initDistance;
        }

        grabInfo_.smoothFilter = 0f;
        FixedUpdateGrabbingObjectTransform(pos, rot, scale);
    }

    void FixedUpdateGrabbingObjectTransform(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var grabbable = grabInfo_.grabbable;

        var a = (Mathf.Clamp(grabInfo_.distance, minGrabSmoothDist, maxGrabSmoothDist) - minGrabSmoothDist) / (maxGrabSmoothDist - minGrabSmoothDist);
        var targetFilter = 1f - (1f - minGrabSmoothFilter) * a;
        var filter = grabInfo_.smoothFilter + (targetFilter - grabInfo_.smoothFilter) * 0.1f;
        grabInfo_.smoothFilter = filter;

        pos = Vector3.Lerp(grabbable.position, pos, filter);
        scale = Vector3.Lerp(grabbable.transform.localScale, scale, filter);

        var v = (pos - grabbable.position) / Time.fixedDeltaTime;
        grabInfo_.velocity.Add(v);

        grabbable.scale = scale;
        grabbable.position = pos;
        grabbable.rotation = rot;
    }
}

}
