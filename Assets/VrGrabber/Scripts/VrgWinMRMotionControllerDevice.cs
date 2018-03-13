#if UNITY_WSA
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

namespace VrGrabber
{

public class VrgWinMRMotionControllerDevice : IDevice 
{
    public bool GetClick(ControllerSide side) 
    {
        switch (side) 
        {
            case ControllerSide.Left:
                return Input.GetKey(KeyCode.JoystickButton16);
            case ControllerSide.Right:
                return Input.GetKey(KeyCode.JoystickButton17);
            default:
                return false;
        }
    }

    public Vector2 GetCoord(ControllerSide side) 
    {
        var interactionStates = InteractionManager.GetCurrentReading();
        foreach (var state in interactionStates) 
        {
            if (side == ControllerSide.Left  && state.source.handedness == InteractionSourceHandedness.Left || 
                side == ControllerSide.Right && state.source.handedness == InteractionSourceHandedness.Right) 
            {
                return state.touchpadPosition;
            }
        }
        return Vector2.zero;
    }

    public float GetHold(ControllerSide side) 
    {
        var interactionStates = InteractionManager.GetCurrentReading();
        foreach (var state in interactionStates) 
        {
            if (side == ControllerSide.Left  && state.source.handedness == InteractionSourceHandedness.Left || 
                side == ControllerSide.Right && state.source.handedness == InteractionSourceHandedness.Right) 
            {
                if (state.grasped) 
                {
                    return 1f;
                } 
                else 
                {
                    return 0f;
                }
            }
        }
        return 0.0f;
    }

    public bool GetHover(ControllerSide side) 
    {
        switch (side) 
        {
            case ControllerSide.Left:
                return Input.GetKey(KeyCode.JoystickButton18);
            case ControllerSide.Right:
                return Input.GetKey(KeyCode.JoystickButton19);
            default:
                return false;
        }
    }

    public Vector3 GetLocalPosition(ControllerSide side) 
    {
        switch (side) 
        {
            case ControllerSide.Left:
                return InputTracking.GetLocalPosition(XRNode.LeftHand);
            case ControllerSide.Right:
                return InputTracking.GetLocalPosition(XRNode.RightHand);
            default:
                return Vector2.zero;
        }
    }

    public Quaternion GetLocalRotation(ControllerSide side) 
    {
        switch (side) 
        {
            case ControllerSide.Left:
                return InputTracking.GetLocalRotation(XRNode.LeftHand);
            case ControllerSide.Right:
                return InputTracking.GetLocalRotation(XRNode.RightHand);
            default:
                return Quaternion.identity;
        }
    }
}

}
#endif