using UnityEngine;

namespace VrGrabber
{

public static class MatrixExtension
{
    public static Vector3 GetRight(this Matrix4x4 m)
    {
        return m.GetColumn(0);
    }

    public static Vector3 GetUp(this Matrix4x4 m)
    {
        return m.GetColumn(1);
    }

    public static Vector3 GetForward(this Matrix4x4 m)
    {
        return m.GetColumn(2);
    }

    public static Vector3 GetPosition(this Matrix4x4 m)
    {
        return m.GetColumn(3);
    }

    public static Quaternion GetRotation(this Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetForward(), m.GetUp());
    }

    public static Vector3 GetScale(this Matrix4x4 m)
    {
        return new Vector3(
            m.GetRight().magnitude,
            m.GetUp().magnitude,
            m.GetForward().magnitude);
    }
}

}