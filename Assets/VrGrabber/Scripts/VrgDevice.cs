using UnityEngine;

namespace VrGrabber
{

public static class Device
{
    public enum ControllerSide
    {
        Left,
        Right,
    }

    private static OVRInput.Controller GetOVRController(ControllerSide side)
    {
        return (side == ControllerSide.Left) ?
            OVRInput.Controller.LTouch :
            OVRInput.Controller.RTouch;
    }

    public static Vector3 GetLocalPosition(ControllerSide side)
    { 
        return OVRInput.GetLocalControllerPosition(GetOVRController(side));
    }

    public static Quaternion GetLocalRotation(ControllerSide side)
    { 
        return OVRInput.GetLocalControllerRotation(GetOVRController(side));
    }

    public static float GetHold(ControllerSide side)
    { 
        return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, GetOVRController(side));
    }

    public static bool GetHover(ControllerSide side)
    { 
        return OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, GetOVRController(side));
    }

    public static bool GetClick(ControllerSide side)
    { 
        return OVRInput.Get(OVRInput.Button.PrimaryThumbstick, GetOVRController(side));
    }

    public static Vector2 GetCoord(ControllerSide side)
    { 
        return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, GetOVRController(side));
    }
}

}