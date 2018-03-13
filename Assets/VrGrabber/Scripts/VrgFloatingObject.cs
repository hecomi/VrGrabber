using UnityEngine;

namespace VrGrabber
{

[RequireComponent(typeof(Rigidbody))]
public class VrgFloatingObject : MonoBehaviour 
{
    Rigidbody rigidbody_;

    [SerializeField, Range(0f, 1f)]
    float moveDamping = 0.95f;

    [SerializeField, Range(0f, 1f)]
    float rotationDamping = 0.95f;

    void Awake() 
    {
        rigidbody_ = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rigidbody_.velocity = Vector3.zero;
        rigidbody_.angularVelocity = Vector3.zero;
    }
    
    void Update() 
    {
        rigidbody_.velocity *= moveDamping;
        rigidbody_.angularVelocity *= rotationDamping;
    }
}

}