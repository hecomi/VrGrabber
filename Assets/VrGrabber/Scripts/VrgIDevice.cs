using UnityEngine;

namespace VrGrabber
{

public enum ControllerSide
{
    Left,
    Right,
}

public interface IDevice
{
    Vector3 GetLocalPosition(ControllerSide side);
    Quaternion GetLocalRotation(ControllerSide side);
    bool GetHold(ControllerSide side);
    bool GetRelease(ControllerSide side);
    bool GetHover(ControllerSide side);
    bool GetClick(ControllerSide side);
    Vector2 GetCoord(ControllerSide side);
}

}
