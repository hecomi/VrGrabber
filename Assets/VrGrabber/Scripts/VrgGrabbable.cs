using UnityEngine;
using UnityEngine.VR;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VrGrabber
{

[RequireComponent(typeof(Rigidbody))]
public class VrgGrabbable : MonoBehaviour 
{
    public bool isScalable = true;

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
        get { return rigidbody_.position; }
        set { rigidbody_.MovePosition(value); }
    }

    public Quaternion rotation
    {
        get { return rigidbody_.rotation; }
        set { rigidbody_.MoveRotation(value); }
    }

    public Vector3 scale
    {
        get { return transform.localScale; }
        set { transform.localScale = value; }
    }

    public Vector3 velocity
    {
        get { return rigidbody_.velocity; }
        set { rigidbody_.velocity = value; }
    }

    public Vector3 angularVelocity
    {
        get { return rigidbody_.angularVelocity; }
        set { rigidbody_.angularVelocity = value; }
    }

    private Vector3 vrWorldPos
    {
        get 
        {
            var vrLocalPos = InputTracking.GetLocalPosition(VRNode.CenterEye);
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
        onReleased.Invoke();

        grabbers_.Remove(grabber);
    }

    public void OnGrabClicked(VrgGrabber grabber)
    {
        if (grabClickCooldown_ > 0f) return;
        onGrabClicked.Invoke();
        grabClickCooldown_ = 0.1f;
    }
}

}