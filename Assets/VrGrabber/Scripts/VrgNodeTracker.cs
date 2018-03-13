using UnityEngine;
using UnityEngine.XR;

public class VrgNodeTracker : MonoBehaviour 
{
    [SerializeField]
    XRNode node = XRNode.LeftHand;

    void Update() 
    {
        transform.SetPositionAndRotation(InputTracking.GetLocalPosition(node), InputTracking.GetLocalRotation(node));
    }
}
