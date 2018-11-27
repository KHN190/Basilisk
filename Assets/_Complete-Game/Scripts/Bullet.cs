using System.Collections;
using UnityEngine;

namespace Completed
{
    public class Bullet : MovingObject
    {
        Player playerScript;

        public delegate void OnBulletHit(int loss);
        public static OnBulletHit BulletHitEvent;

        public delegate void OnEndMove();
        public static OnEndMove EndMoveEvent;

        private void Awake()
        {
            base.Start();

            playerScript = GameObject.Find("Player").GetComponent<Player>();
        }

        public IEnumerator Shoot(int xDir, int yDir, int N)
        {
            RaycastHit2D hit;

            Thrown(xDir, yDir, N, out hit);

            yield return null;
        }

        protected override void OnCantMove<T>(T component) {}

        private void OnEnable()
        {
            EndMoveEvent += DestroyBullet;
        }

        private void OnDisable()
        {
            EndMoveEvent -= DestroyBullet;
        }

        private void DestroyBullet()
        {
            GameObject obj = GameObject.Find(this.name);
           
            Destroy(obj);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player" || other.tag == "RedPill" || other.tag == "BluePill")

                return;

            DestroyBullet();

            if (other.tag == "Enemy")

                other.GetComponent<Enemy>().TakeDamage(playerScript.dmg);
        }

        // Thrown N tiles away
        public void Thrown(int xDir, int yDir, int N)
        {
            RaycastHit2D hit;

            Thrown(xDir, yDir, N, out hit);
        }

        public void Thrown(int xDir, int yDir, int N, out RaycastHit2D hit)
        {
            if (xDir != 0)
                yDir = 0;

            Vector2 start = transform.position;

            Vector2 end = start + new Vector2(xDir * N, yDir * N);
            
            hit = IsBlocked(start, end);

            if (GameManager.instance.pause)
                return;

            if (hit.transform != null && hit.transform.tag != "RedPill" && hit.transform.tag != "BluePill")
            {
                end = hit.transform.position;
                end.x -= xDir;
                end.y -= yDir;
            }

            StartCoroutine(SmoothMovement(end));
        }

        protected override IEnumerator SmoothMovement(Vector3 end)
        {
            yield return StartCoroutine(base.SmoothMovement(end));

            if (EndMoveEvent != null)
                EndMoveEvent();
        }
    }
}