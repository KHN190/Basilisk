using System.Collections;
using UnityEngine;

namespace Completed
{
	public class Enemy : MovingObject
	{
        /*
         * Fields & Properties
        */

		public int playerDamage;
		public AudioClip attackSound;
        public int hp = 2;
        public int vision = 6;
        public float attackCooldown = .5f;
        public float dashCooldown = .5f;
        public float breedCooldown = 3.0f;
        public float buildWallCooldown = 3.0f;

        private Animator animator;
        private SpriteRenderer sprite;
        private Transform target;
        private Transform player;
		private float lastAttack;
        private float lastDash;
        private float lastBuildWall;
        private bool freeze = false;
        private bool dead = false;

        /*
         * Delegate & Events
        */

        public delegate void OnEnemyDie();
        public static OnEnemyDie EnemyDieEvent;

        /*
         * Public Methods
        */
		
        // used by game manager per frame
		public void MoveEnemy ()
		{
            if (freeze || GameManager.instance.pause)
                return;

            MoveInFreeWill();
        }

        // used by player to apply damage
        public void TakeDamage(int loss)
        {
            hp -= loss;

            sprite.color = Color.red;

            Invoke("ResetColor", 0.5f);

            if (hp <= 0 && !dead)
            {
                dead = true;

                if (EnemyDieEvent != null && GetComponent<BoxCollider2D>().enabled)
                    EnemyDieEvent();

                SoundManager.instance.PlaySingle(attackSound);

                GetComponent<BoxCollider2D>().enabled = false;

                animator.SetTrigger("enemyDie");

                StartCoroutine(DieAfterAnim());
            }
        }

        private void TakeBomb()
        {
            TakeDamage(1);

            FreezeForSeconds(1f);
        }

        /*
         * Protected Methods 
        */
        protected override void Start()
        {
            GameManager.instance.AddEnemyToList(this);

            animator = GetComponent<Animator>();

            sprite = GetComponent<SpriteRenderer>();

            target = GameObject.FindGameObjectWithTag("Player").transform;

            player = target;

            lastAttack = Time.time;

            lastDash = Time.time;

            lastBuildWall = Time.time;

            base.Start();
        }

        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            float now = Time.time;

            speed = 1f;

            if (now - lastDash < dashCooldown || GameManager.instance.playerLevel < 1)
            {
                base.AttemptMove<T>(xDir, yDir);

                return;
            }

            lastDash = now;

            speed = 3f;

            base.AttemptMove<T>(xDir, yDir);
        }

        protected override bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            Vector2 start = transform.position;

            Vector2 end = start + new Vector2(xDir, yDir);

            hit = IsBlocked(start, end);

            if (isMoving)

                return false;

            if (hit.transform == null)
            {
                StartCoroutine(SmoothMovement(end));

                return true;
            }
            if (IsBlockedByNonEnemy(start, end))
            {
                end = RandomAdj();

                if (IsBlockedByNonEnemy(start, end))
                    return false;

                StartCoroutine(SmoothMovement(end));
                return true;
            }
            if (hit.transform.tag == tag)
            {
                SmoothMoveAfterShortPeriod(end);

                return true;
            }
            return false;
        }

        protected override void OnCantMove<T>(T component)
        {
            if (typeof(T) == typeof(Player))
            {
                if (Time.time - lastAttack <= attackCooldown)

                    return;

                lastAttack = Time.time;

                Player hitPlayer = component as Player;

                hitPlayer.LoseHP(playerDamage);

                animator.SetTrigger("enemyAttack");

                SoundManager.instance.PlaySingle(attackSound);

                return;
            }
        }

        /*
         * Private Methods - Abilities
        */

        // ability: destroy a wall
        private void DamageWall(Transform wall)
        {
            if (speed > 1)

                wall.GetComponent<Wall>().TakeDamage(1);
        }

        // ability: build a wall
        private void BuildWall()
        {
            if (GameManager.instance.playerLevel < 1)
                return;

            float now = Time.time;

            if (now - lastBuildWall >= buildWallCooldown)
            {
                Vector2 slot = RandomAdj();

                BoardManager board = GameManager.instance.boardScript;

                if (slot.x < 0 || slot.x >= board.columns ||
                    slot.y < 0 || slot.y >= board.rows)
                    return;

                if (!IsBlocked(transform.position, slot) && CountAdj() <= 2)
                {
                    GameManager.instance.boardScript.LayoutWallAt(slot);
                }
                lastBuildWall = now;
            }
        }

        // ability: breed new enemies
        private void Breed()
        {
            if (GameManager.instance.playerLevel < 3)
                return;

            float now = Time.time;

            if (now - GameManager.instance.lastBreedTime > breedCooldown)
            {
                Vector2 slot = RandomAdj();

                BoardManager board = GameManager.instance.boardScript;

                if (slot.x < 0 || slot.x >= board.columns ||
                    slot.y < 0 || slot.y >= board.rows)
                    return;

                if (!IsBlocked(transform.position, slot))
                {
                    GameManager.instance.boardScript.LayoutEnemyAt(slot);
                }
                GameManager.instance.lastBreedTime = now;
            }
        }

        /*
         * Private Methods - Status
        */
        private void Freeze()
        {
            float time = player.GetComponent<Player>().stunDuration;

            FreezeForSeconds(time);
        }

        private void FreezeForSeconds(float time) 
        {
            freeze = true;

            sprite.color = Color.grey;

            Invoke("Unfreeze", time);
        }

        private void Unfreeze()
        {
            sprite.color = Color.white;

            freeze = false;
        }

        /*
         * Private Methods - Search
        */

        // search for a friend
        private void FindAlly() 
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag("Enemy");

            float distance = float.MaxValue;

            GameObject obj = null;

            foreach (GameObject ally in allies)
            {
                Vector3 pos1 = ally.transform.position;
                Vector3 pos2 = transform.position;

                if (pos1.Equals(pos2)) { continue; }
                if (!ally.activeSelf) { continue; }

                float curDistance = (pos1 - pos2).sqrMagnitude;

                if (curDistance < distance) 
                {
                    obj = ally;
                    distance = curDistance;
                }
            }

            if (obj != null)
                target = obj.transform;
        }

        // search for an enemy
        private void FindPlayer() 
        {
            target = player;
        }


        /*
         * Private Methods - Move & Actions
        */
        private void MoveInFreeWill()
        {
            Vector2 start = transform.position;
            Vector2 end = player.position;

            if (!WithinVision(start, end))
            {
                FindAlly();

                BuildWall();

                end = target.position;

                if (IsAdjacent(start, end) && target.gameObject.activeSelf)
                {
                    Breed();
                }
                if (target.tag == "Player")
                {
                    return;
                }
            }
            else
            {
                FindPlayer();

                end = target.position;
            }

            end.x = Mathf.Round(end.x);
            end.y = Mathf.Round(end.y);

            Vector2 move = DecideMovement(start, end);

            AttemptMove<Player>((int)move.x, (int)move.y);
        }

        private Vector2 DecideMovement(Vector2 start, Vector2 end)
        {
            Transform rt = IsBlocked(start, new Vector2(start.x + 1, start.y)).transform;
            Transform lt = IsBlocked(start, new Vector2(start.x - 1, start.y)).transform;
            Transform up = IsBlocked(start, new Vector2(start.x, start.y + 1)).transform;
            Transform dn = IsBlocked(start, new Vector2(start.x, start.y - 1)).transform;

            Vector2 move = Vector2.zero;

            if (Mathf.Abs(end.x - start.x) > Mathf.Abs(end.y - start.y))
            {
                if (end.x > start.x)
                {
                    if (rt != null && rt.tag == "Wall")
                    {
                        move.y = Random.value < 0.5f ? -1 : 1;

                        DamageWall(rt);
                    }
                    else
                        move.x = 1;
                }
                else if (end.x < start.x)
                {
                    if (lt != null && lt.tag == "Wall")
                    {
                        move.y = Random.value < 0.5f ? -1 : 1;

                        DamageWall(lt);
                    }
                    else
                        move.x = -1;
                }
            }
            else
            {
                if (end.y > start.y)
                {
                    if (up != null && up.tag == "Wall")
                    {
                        move.x = Random.value < 0.5f ? -1 : 1;

                        DamageWall(up);
                    }
                    else
                        move.y = 1;
                }
                else if (end.y < start.y)
                {
                    if (dn != null && dn.tag == "Wall")
                    {
                        move.x = Random.value < 0.5f ? -1 : 1;

                        DamageWall(dn);
                    }
                    else
                        move.y = -1;
                }
            }
            return move;
        }

        private void SmoothMoveAfterShortPeriod(Vector2 end) 
        {
            float time = Random.value;

            FreezeForSeconds(time);

            StartCoroutine(SmoothMovement(end));
        }

        private bool IsBlockedByNonEnemy(Vector2 start, Vector2 end)
        {
            RaycastHit2D[] hits =  Physics2D.LinecastAll(start, end);

            foreach( RaycastHit2D hit in hits)
            {
                if (hit.transform.tag == "Wall")
                    return true;
            }

            return false;
        }

        // some place in vision?
        private bool WithinVision(Vector2 start, Vector2 end)
        {
            if (Mathf.Abs(start.x - end.x) <= vision &&
                Mathf.Abs(start.y - end.y) <= vision)

                return true;

            return false;
        }

        /*
         * Private Methods - Anim
        */
        private IEnumerator DieAfterAnim()
        {
            GetComponent<Collider2D>().enabled = false;

            AnimatorStateInfo state =  animator.GetCurrentAnimatorStateInfo(0);

            yield return new WaitForSeconds(state.length);

            gameObject.SetActive(false);
        }

        private void ResetColor()
        {
            sprite.color = Color.white;
        }

        /*
         * Private Methods - Events
        */
            private void OnEnable()
        {
            Player.PlayerStunEvent += Freeze;
            Player.PlayerBombEvent += TakeBomb;
        }

        private void OnDisable()
        {
            Player.PlayerStunEvent -= Freeze;
            Player.PlayerBombEvent -= TakeBomb;
        }
    }
}
