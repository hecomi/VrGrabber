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

    public float GetHold(ControllerSide side) 
    {
        return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetOVRController(side));
    }

    public bool GetHover(ControllerSide side) 
    {
        return OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, GetOVRController(side));
    }

    public bool GetClick(ControllerSide side) 
    {
        return OVRInput.Get(OVRInput.Button.PrimaryThumbstick, GetOVRController(side));
    }

    public Vector2 GetCoord(ControllerSide side) 
    {
        return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, GetOVRController(side));
    }
}

}
#endif
