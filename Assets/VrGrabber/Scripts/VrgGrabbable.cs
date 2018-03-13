using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VrGrabber
{

[RequireComponent(typeof(Rigidbody))]
public class VrgGrabbable : MonoBehaviour 
{
    public bool isScalable = true;
    public bool avoidIntersection = false;
    public float maxSpeed = 10f;

    public UnityEvent onGrabbed = new UnityEvent();
    public UnityEvent onReleased = new UnityEvent();
    public UnityEvent onGrabClicked = new UnityEvent();
    public UnityEvent onGrabMoved = new UnityEvent();

    int grabId_ = 0;
    float grabClickCooldown_ = 0f;

    List<VrgGrabber> grabbers_ = new List<VrgGrabber>();
    public List<VrgGrabber> grabbers
    {
        get { return grabbers_; }
    }

    Rigidbody rigidbody_;
    public new Rigidbody rigidbody
    {
        get { return rigidbody_; }
    }

    public bool isGrabbed
    {
        get { return grabbers_.Count > 0; }
    }

    public bool isMultiGrabbed
    {
        get { return grabbers_.Count > 1; }
    }

    public Vector3 position
    {
        get { return rigidbody.position; }
        set { SetPosition(value); }
    }

    public Quaternion rotation
    {
        get { return rigidbody.rotation; }
        set { SetRotation(value); }
    }

    public Vector3 scale
    {
        get { return transform.localScale; }
        set { transform.localScale = value; }
    }

    public Vector3 velocity
    {
        get { return rigidbody.velocity; }
        set { rigidbody.velocity = value; }
    }

    public Vector3 angularVelocity
    {
        get { return rigidbody.angularVelocity; }
        set { rigidbody.angularVelocity = value; }
    }

    private Vector3 vrWorldPos
    {
        get 
        {
            var vrLocalPos = InputTracking.GetLocalPosition(XRNode.CenterEye);
            var vrWorldPos = Camera.main.cameraToWorldMatrix.MultiplyPoint(vrLocalPos);
            return vrWorldPos;
        }
    }

    void Awake()
    {
        rigidbody_ = GetComponent<Rigidbody>();
    }

    void Update()
    {
        grabClickCooldown_ -= Time.deltaTime;

        if (isGrabbed)
        {
            onGrabMoved.Invoke();
        }
    }

    void FixedUpdate()
    {
        if (isGrabbed && rigidbody.useGravity && !rigidbody.isKinematic)
        {
            rigidbody.AddForce(-Physics.gravity);
        }
    }

    public int OnGrabbed(VrgGrabber grabber)
    {
        grabbers_.Add(grabber);

        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;

        onGrabbed.Invoke();

        return grabId_++;
    }

    public void OnReleased(VrgGrabber grabber)
    {
        grabbers_.Remove(grabber);

        onReleased.Invoke();
    }

    public void OnGrabClicked(VrgGrabber grabber)
    {
        if (grabClickCooldown_ > 0f) return;
        onGrabClicked.Invoke();
        grabClickCooldown_ = 0.1f;
    }

    void SetPosition(Vector3 dest)
    {
        if (avoidIntersection)
        {
            var v = (dest - position) / Time.fixedUnscaledDeltaTime;
            if (v.magnitude > maxSpeed)
            {
                v = v.normalized * maxSpeed;
            }
            rigidbody.velocity = v;
        }
        else
        {
            rigidbody.MovePosition(dest);
        }
    }

    void SetRotation(Quaternion dest)
    {
        /*
        if (avoidIntersection)
        {
            var dRot = dest * Quaternion.Inverse(rotation);
            var dEuler = dRot.eulerAngles;
            if (dEuler.x > 180) dEuler.x -= 360;
            if (dEuler.y > 180) dEuler.y -= 360;
            if (dEuler.z > 180) dEuler.z -= 360;
            var w = dEuler / Time.fixedUnscaledDeltaTime;
            rigidbody.angularVelocity = (w - rigidbody.angularVelocity) * 0.01f;
        }
        else
        {
            rigidbody.MoveRotation(dest);
        }
        */
        rigidbody.MoveRotation(dest);
    }
}

}