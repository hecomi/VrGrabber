using UnityEngine;

namespace VrGrabber
{

[RequireComponent(typeof(VrgGrabber)),
 RequireComponent(typeof(LineRenderer))]
public class VrgTargetLine : MonoBehaviour 
{
    private VrgGrabber grabber_;
    private LineRenderer line_;

    public float maxAngle = 60f;
    public int jointNum = 32;

    void Awake()
    {
        grabber_ = GetComponent<VrgGrabber>();
        line_ = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!grabber_ || !grabber_.enabled)
        {
            line_.enabled = false;
            return;
        }
        line_.enabled = true;

        var startPos = transform.position;
        var dir = grabber_.gripDir.normalized;
        var to = grabber_.targetPos - startPos;

        var d = to.magnitude;
        if (d < Mathf.Epsilon) 
        {
            line_.enabled = false;
            return;
        }

        var xAxis = to.normalized;

        var maxRadian = maxAngle * Mathf.Deg2Rad;
        var minProd = Mathf.Cos(maxRadian);

        var prodX = Mathf.Max(Vector3.Dot(xAxis, dir), minProd);
        var up = dir - prodX * xAxis;
        var yAxis = up.normalized;

        var prodY = Vector3.Dot(yAxis, dir);
        var dydx0 = prodY / prodX;
        var xc = d / 2;
        var a = - dydx0 / (2 * xc);
        var b = - a * xc * xc;

        line_.positionCount = jointNum;

        var dx = d / (jointNum - 1);
        for (int i = 0; i < jointNum; ++i) 
        {
            var x = dx * i;
            var y = a * Mathf.Pow(x - xc, 2f) + b;
            var pos = startPos + (x * xAxis) + (y * yAxis);
            line_.SetPosition(i, pos);
        }
    }
}

}