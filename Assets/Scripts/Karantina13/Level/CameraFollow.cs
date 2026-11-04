using UnityEngine;

namespace Karantina13.Level
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Rect bounds;

        private Camera cam;
        private Vector3 velocity;

        public void Configure(Transform target, Rect worldBounds)
        {
            this.target = target;
            bounds = worldBounds;
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (target != null) transform.position = GoalPosition();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, GoalPosition(), ref velocity, smoothTime);
        }

        private Vector3 GoalPosition()
        {
            Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
            if (cam == null || !cam.orthographic || bounds.width <= 0f || bounds.height <= 0f) return goal;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            goal.x = ClampAxis(goal.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
            goal.y = ClampAxis(goal.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);
            return goal;
        }

        private static float ClampAxis(float value, float min, float max)
        {
            if (min > max) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min, max);
        }
    }
}
