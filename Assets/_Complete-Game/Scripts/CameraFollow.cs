using UnityEngine;
using System.Collections;

namespace Completed
{
    public class CameraFollow : MonoBehaviour
    {
        public float panSpeed = 30f;
        public float panBorderThickness = 20f;
        public Vector2 limitAxisX;
        public float moveTime = 2.0f;

        void Start()
        {
            limitAxisX = new Vector2(7, 17);
        }

        private void OnEnable()
        {
            Player.PlayerMoveEvent += FollowPlayer;
        }

        private void OnDisable()
        {
            Player.PlayerMoveEvent -= FollowPlayer;
        }

        void FollowPlayer(int xDir) 
        {
            Vector3 end = new Vector3
            {
                x = transform.position.x + xDir,
                y = transform.position.y,
                z = transform.position.z
            };
            end.x = Mathf.Clamp(end.x, limitAxisX.x, limitAxisX.y);

            StartCoroutine(SmoothMovement(end));
        }

        protected IEnumerator SmoothMovement(Vector3 end)
        {
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            while (sqrRemainingDistance > float.Epsilon)
            {
                Vector3 newPostion = Vector3.MoveTowards(transform.position, end, 1.0f / moveTime * Time.deltaTime);

                transform.position = newPostion;

                sqrRemainingDistance = (transform.position - end).sqrMagnitude;

                yield return null;
            }
        }
    }
}
