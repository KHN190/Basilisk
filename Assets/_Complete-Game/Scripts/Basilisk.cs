using UnityEngine;

namespace Completed
{
    public class Basilisk : MonoBehaviour
    {
        /*
         * Fields & Properties
        */

        public int hp = 20;
        public float spawnTime = 5f;

        private SpriteRenderer sprite;
        private GameObject player;
        private bool dead = false;
        private bool activated = false;
        private float lastSpawnTime;

        /*
         * Delegate & Events
        */

        public delegate void OnBossDie();
        public static OnBossDie BossDieEvent;

        public delegate void OnBossHit();
        public static OnBossHit BossHitEvent;

        /*
         * MonoBehaviour
        */
        void Start()
        {
            sprite = GetComponent<SpriteRenderer>();

            player = GameObject.Find("Player");

            lastSpawnTime = Time.time;
        }

        void Update()
        {
            if (!activated)
                return;

            float now = Time.time;

            if (now - lastSpawnTime < spawnTime)
                return;

            SpawnEnemies();

            lastSpawnTime = now;
        }

        /*
         * Public Methods
        */

        public void TakeDamage(int loss)
        {
            if (!activated)
                return;

            hp -= loss;

            sprite.color = Color.red;

            Invoke("ResetColor", 0.5f);

            if (hp <= 0 && !dead)
            {
                dead = true;

                if (BossDieEvent != null)
                    BossDieEvent();
            }
            if (BossHitEvent != null && !dead) {
                BossHitEvent();
            }
        }

        public void SpawnEnemies()
        {
            if (!activated)
                return;

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length >= 10)
                return;

            BoardManager board = GameManager.instance.boardScript;
            Vector2 pos = player.transform.position;
            Vector2 bos = transform.position;
            Vector2 new_pos = transform.position;

            if (pos.x < bos.x)
                new_pos.x += 2;
            else
                new_pos.x -= 2;

            for (int i = -2; i <= 2; i += 2)
            {
                Vector2 cur = new_pos;
                cur.y += i;

                if (HasGameObject(cur))
                    continue;

                board.LayoutEnemyAt(cur);
            }

            new_pos = transform.position;

            if (pos.x < bos.x)
                new_pos.x -= 4;
            else
                new_pos.x += 4;

            for (int i = -4; i <= 4; i += 8)
            {
                Vector2 cur = new_pos;
                cur.y += i;

                if (HasGameObject(cur))
                    continue;

                board.LayoutEnemyAt(cur);
            }
        }

        /*
         * Private Methods
        */

        private void ResetColor()
        {
            sprite.color = Color.white;
        }

        private bool HasGameObject(Vector2 pos)
        {
            RaycastHit2D hit = Physics2D.Linecast(pos, pos);

            return hit.transform != null;
        }

        private void ColorGray()
        {
            sprite.color = Color.gray;
        }

        private void Activate()
        {
            activated = true;
        }

        private void OnEnable()
        {
            BossDieEvent += ColorGray;
            BossHitEvent += SpawnEnemies;
            Player.PlayerChooseYEvent += Activate;
        }

        private void OnDisable()
        {
            BossDieEvent -= ColorGray;
            BossHitEvent -= SpawnEnemies;
            Player.PlayerChooseYEvent -= Activate;
        }
    }
}