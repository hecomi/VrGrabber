using UnityEngine;

namespace VrGrabber
{
    public class Device {
        static IDevice _instance;
        public static IDevice instance {
            get {
                if (_instance == null) {
#if !UNITY_WSA
                    _instance = new VrgOculusTouchDevice();
#elif UNITY_WSA
                    _instance = new VrgWinMRMotionControllerDevice();
#else
#error "Not implemented."
#endif
                }
                return _instance;
            }
        }

        public Vector3 GetLocalPosition(ControllerSide side) {
            return _instance.GetLocalPosition(side);
        }

        public Quaternion GetLocalRotation(ControllerSide side) {
            return _instance.GetLocalRotation(side);
        }

        public float GetHold(ControllerSide side) {
            return _instance.GetHold(side);
        }

        public bool GetHover(ControllerSide side) {
            return _instance.GetHover(side);
        }

        public bool GetClick(ControllerSide side) {
            return _instance.GetClick(side);
        }

        public Vector2 GetCoord(ControllerSide side) {
            return _instance.GetCoord(side);
        }
    }
}