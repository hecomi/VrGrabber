#if !UNITY_WSA
using UnityEngine;

namespace VrGrabber
{

public class VrgOculusTouchDevice : IDevice
{
    private OVRInput.Controller GetOVRController(ControllerSide side)
    {
        return (side == ControllerSide.Left) ?
            OVRInput.Controller.LTouch :
            OVRInput.Controller.RTouch;
    }

    public Vector3 GetLocalPosition(ControllerSide side)
    {
        return OVRInput.GetLocalControllerPosition(GetOVRController(side));
    }

    public Quaternion GetLocalRotation(ControllerSide side)
    {
        return OVRInput.GetLocalControllerRotation(GetOVRController(side));
    }

    public bool GetHold(ControllerSide side)
    {
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
    }

    public bool GetRelease(ControllerSide side)
    {
        return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger);
    }

    public bool GetHover(ControllerSide side)
    {
        return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger);
    }

    public bool GetClick(ControllerSide side)
    {
        return OVRInput.Get(OVRInput.Button.PrimaryTouchpad);
    }

    public Vector2 GetCoord(ControllerSide side)
    {
        return OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
    }
}

}
#endif
