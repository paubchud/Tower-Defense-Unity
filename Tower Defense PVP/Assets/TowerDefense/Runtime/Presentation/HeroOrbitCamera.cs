using TowerDefense.Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefense.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class HeroOrbitCamera : MonoBehaviour
    {
        public float Distance = 20;
        public float Pitch = 55;
        public float Yaw;
        public float Sensitivity = 0.16f;
        private readonly RaycastHit[] obstructionHits = new RaycastHit[12];
        private NetworkHero target;

        private void LateUpdate()
        {
            var session = PrototypeSession.Instance;
            if (session == null) return;
            if (target == null) target = session.LocalHero;
            if (target == null) return;
            var controls = session.Controls;
            bool onUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!controls.BlockGameplay && !onUI && controls.Orbit.IsPressed())
            {
                Vector2 delta = controls.Look.ReadValue<Vector2>();
                Yaw += delta.x * Sensitivity;
                Pitch = Mathf.Clamp(Pitch - delta.y * Sensitivity, 30, 78);
            }
            var rotation = Quaternion.Euler(Pitch, Yaw, 0);
            var focus = target.transform.position + Vector3.up;
            var direction = rotation * Vector3.back;
            float distance = Distance;
            int count = Physics.SphereCastNonAlloc(focus, 0.25f, direction, obstructionHits, Distance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (!obstructionHits[i].transform.IsChildOf(target.transform) && obstructionHits[i].distance > 0.1f)
                    distance = Mathf.Min(distance, Mathf.Max(1.5f, obstructionHits[i].distance - 0.3f));
            transform.SetPositionAndRotation(focus + direction * distance, rotation);
        }
    }
}
