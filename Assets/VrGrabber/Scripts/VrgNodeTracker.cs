using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VrgNodeTracker : MonoBehaviour {
    [SerializeField]
    XRNode node = XRNode.LeftHand;

    private void Update() {
        transform.SetPositionAndRotation(InputTracking.GetLocalPosition(node), InputTracking.GetLocalRotation(node));
    }
}
