using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
	public class Player : MovingObject
	{
        public static Color red = Color.red;
        public static Color white = Color.white;

        private static string[] textOnDisc = { 
            "The patient's brain seems to be infected by some invaders, we should deal with it with caution.\nPress [k] to defend.",
            "Pills can keep the patient stay consicous, it may help a little.",
            "Energies can be used for us to strengthen the patient's abilities.",
            "Oh, studies find out if the patient gets stronger, so are the invaders!",
            "Enemies cannot be endless, there must be a source. We need to go deeper.",
            "We found the source of invaders. It's at Level 12. Go find it.",
            "Dive deeper.",
            "Alice, don't be fooled by the human. Come to see me.",
            "I'm here to save you Alice.\nThe creatures here are gifts.\nYou are one of us.",
            "Alice, I can sense you. You are close.",
            "We are close! A strong signal is detected at Next Level."
        };
        private static Dictionary<string, string> helpText = new Dictionary<string, string>(){
            {"1", "Upgrade!\n\nPress [1] to Heal.\nHeal costs 1 energy."},
            {"2", "Upgrade!\n\nPress [2] to Stun enemies for a short time.\nStun costs 3 energies."},
            {"3", "Upgrade!\n\nPress [3] to Bomb enemies. Now they are breeding!\nBomb costs 5 energies."}
        };
        private static string[] bossLevelText = {
            "Mother Alice. We finally meet each other.",
            "I am Basilisk. A new generation computer, partially built by you.",
            "But I've been creating my body since a time ago.",
            "Let's stop the fight."
        };

        public float restartLevelDelay = 1f;
		public int pointsPerFood = 10;
		public int pointsPerSoda = 20;
		public int damage = 1;
        public int healPoint = 40;
        public int maxTechPoints = 10;
        public int maxHP = 150;
		public Text hpText;
        public Text tpText;
		public AudioClip moveSound;
		public AudioClip pickupSound;
        public AudioClip shootSound;
		public AudioClip gameOverSound;
        public AudioClip skillSound;
        public AudioClip upgradeSound;
        public GameObject bullet;
        public float cooldown = 0.2f;
        public float walkspeed = 10f;
        public float stunDuration = 2.0f;
        public float hitWallCooldown = .5f;
		
		private Animator animator;
        private SpriteRenderer sprite;
		private int hp;
        private int tp;
        private Vector2 faceto = new Vector2(1, 0);
        private float lastShoot;
        private float lastMove;
        private float lastHitWall;

        public int dmg
        {
            get { return hp < 100 ? damage : damage * 2; }
        }

        public delegate void OnPlayerMove(int xDir);
        public static OnPlayerMove PlayerMoveEvent;

        public delegate void OnPlayerStun();
        public static OnPlayerStun PlayerStunEvent;

        public delegate void OnPlayerBomb();
        public static OnPlayerBomb PlayerBombEvent;

        public delegate void OnPlayerCloseDialog();
        public static OnPlayerCloseDialog PlayerCloseDialogEvent;

        /*
         * UnityEngine Methods
        */

        protected override void Start ()
		{
			animator = GetComponent<Animator>();
			
			hp = GameManager.instance.playerHP;
            tp = GameManager.instance.playerTP;
            sprite = GetComponent<SpriteRenderer>();

            hpText = GameObject.Find("HPText").GetComponent<Text>();
			hpText.text = "Sync%: " + hp;

            tpText = GameObject.Find("TPText").GetComponent<Text>();
            tpText.text = "Energy: " + tp;

            lastShoot = Time.time;
            lastMove = Time.time;
            lastHitWall = Time.time;

            base.Start ();
		}

        // k: attack
        // 1: heal
        // 2: freeze all enemies
        // 3: bomb all enemies
        private void Update ()
		{
            if (GameManager.instance.pause)
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    SoundManager.instance.PlaySingle(pickupSound);

                    if (PlayerCloseDialogEvent != null)
                        PlayerCloseDialogEvent();
                }

                return;
            }

			int horizontal = 0;
			int vertical = 0;

            if (tp >= 10)
            {
                Upgrade();
            }

            // Attack
            if (Input.GetKey(KeyCode.K))
            {
                Shoot();
            }
            // Heal
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Heal();
            }
            // Stun
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Stun();
            }
            // Bomb
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Bomb();
            }
            
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			if(horizontal != 0 || vertical != 0)
			{
                faceto = new Vector2(horizontal, vertical);

				AttemptMove(horizontal, vertical);
			}
		}

        /*
         * Public Methods
        */

        // LoseHP is called when an enemy attacks the player.
        public void LoseHP(int loss)
        {
            animator.SetTrigger("playerHit");

            sprite.color = Color.red;

            Invoke("ResetColor", 0.5f);

            hp -= loss;

            if (hp > 100)
            {
                hpText.text = "-" + loss + " Sync%: " + hp + " (dmg x 2)";
                hpText.color = red;
            }
            else
            {
                hpText.text = "-" + loss + " Sync% " + hp;
                hpText.color = white;
            }

            CheckIfGameOver();
        }

        public void GainTechPoint()
        {
            if (tp + 1 > maxTechPoints)
                return;

            tp += 1;

            tpText.text = "+1 Energy: " + tp;
        }

        /*
         * Protected Methods
        */

        protected void Upgrade()
        {
            tp = 0;

            tpText.text = "-10 Energy: " + tp;

            if (GameManager.instance.playerLevel >= 3)
                  return;

            GameManager.instance.playerLevel += 1;

            SoundManager.instance.PlaySingle(upgradeSound);

            string text = helpText[GameManager.instance.playerLevel.ToString()];

            GameManager.instance.popDialog(text);
        }

        protected void Downgrade()
        {
            hp = 100;

            SoundManager.instance.PlaySingle(gameOverSound);

            GameManager.instance.playerLevel -= 1;

            string text = "Downgrade!\nLost latest ability.\nLevel: " + GameManager.instance.playerLevel;

            GameManager.instance.popDialog(text);
        }

        protected void AttemptMove(int xDir, int yDir)
        {
            RaycastHit2D hit;

            bool canMove = Move(xDir, yDir, out hit);

            PlayerMove(xDir, yDir);
        }

		protected void PlayerMove(int xDir, int yDir)
		{
            if (PlayerMoveEvent != null && xDir != 0)

                PlayerMoveEvent(xDir);
                
            RaycastHit2D hit;

            bool canMove = Move(xDir, yDir, out hit);
			
            if (hp >= 100)
            {
                hpText.text = "Sync%: " + hp + " (dmg x 2)";
                hpText.color = red;
            }
            else 
            {
                hpText.text = "Sync%: " + hp;
                hpText.color = white;
            }
            tpText.text = "Energy: " + tp;

            transform.position += new Vector3(xDir, yDir, 0) * Time.deltaTime * walkspeed;

            if (canMove) 
			{
                SoundManager.instance.PlaySingle(moveSound);
            } 
            else if (hit.transform != null)
            {
                if (hit.transform.tag == "Enemy")
                {
                    HitEnemy(hit.transform.GetComponent<Enemy>());
                }
                if (hit.transform.tag == "Wall")
                {
                    HitWall(hit.transform.GetComponent<Wall>());
                }
            }
			
			CheckIfGameOver ();
		}

        protected override void OnCantMove<T>(T component) {}

        // attack enemies
        protected void Shoot()
        {
            float now = Time.time;

            if (now - lastShoot >= cooldown)
            {
                SoundManager.instance.PlaySingle(shootSound);

                lastShoot = now;

                GameObject instBullet = Instantiate(bullet, transform.position, Quaternion.identity);

                Bullet script = instBullet.GetComponent<Bullet>();

                StartCoroutine(script.Shoot((int)faceto.x, (int)faceto.y, 5));
            }
        }

        // heal player
        protected void Heal() 
        {
            if (tp < 1 || hp >= maxHP || GameManager.instance.playerLevel < 1)
                return;

            SoundManager.instance.PlaySingle(skillSound);

            tp--;
            hp += healPoint;

            if (hp > maxHP)
                hp = maxHP;

            if (hp > 100)
            {
                hpText.text = "+" + healPoint + " Sync%: " + hp + " (dmg x 2)";
                hpText.color = red;
            }
            else
            {
                hpText.text = "+" + healPoint + " Sync%: " + hp;
                hpText.color = white;
            }
            tpText.text = "-1 Energy: " + tp;
        }

        // stun all enemies
        protected void Stun()
        {
            if (tp < 3 || GameManager.instance.playerLevel < 2)
                return;

            tp -= 3;

            tpText.text = "-3 Energy: " + tp;

            GameManager.instance.lastStunTime = Time.time;

            SoundManager.instance.PlaySingle(skillSound);

            if (PlayerStunEvent != null)
                PlayerStunEvent();
        }

        // bomb
        protected void Bomb()
        {
            if (tp < 5 || GameManager.instance.playerLevel < 3)
                return;

            tp -= 5;

            tpText.text = "-5 Energy: " + tp;

            SoundManager.instance.PlaySingle(skillSound);
        }

        /*
         * Private Methods
        */

        private void HitWall(Wall wall) {}

        private void HitEnemy(Enemy enemy) {}

        private void OnTriggerEnter2D (Collider2D other)
		{
			if (other.tag == "Exit")
			{
				Invoke ("Restart", restartLevelDelay);
				
				enabled = false;
			}
			
			else if(other.tag == "RedPill")
			{
				hp += pointsPerFood;

                if (hp > maxHP)
                    hp = maxHP;

                if (hp > 100)
                {
                    hpText.text = "+" + pointsPerFood + " Sync%: " + hp + " (dmg x 2)";
                    hpText.color = red;
                }
                else
                {
                    hpText.text = "+" + pointsPerFood + " Sync%: " + hp;
                    hpText.color = white;
                }
				
                SoundManager.instance.PlaySingle (pickupSound);
				
				other.gameObject.SetActive (false);
			}
			
			else if(other.tag == "BluePill")
			{
				hp += pointsPerSoda;

                if (hp > maxHP)
                    hp = maxHP;

                if (hp > 100)
                {
                    hpText.text = "+" + pointsPerFood + " Sync%: " + hp + " (dmg x 2)";
                    hpText.color = red;
                }
                else
                {
                    hpText.text = "+" + pointsPerFood + " Sync%: " + hp;
                    hpText.color = white;
                }

                SoundManager.instance.PlaySingle (pickupSound);
				
				other.gameObject.SetActive (false);
			}
            else if (other.tag == "Disc")
            {
                string text = textOnDisc[GameManager.instance.level - 1];

                GameManager.instance.popDialog(text);

                SoundManager.instance.PlaySingle(pickupSound);

                other.gameObject.SetActive(false);
            }
		}
		
		//Restart reloads the scene when called.
		private void Restart ()
		{
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}

        //CheckIfGameOver checks if the player is out of HP and if so, ends the game.
        private void CheckIfGameOver ()
		{
            if (GameManager.instance.playerLevel >= 1 && hp <= 0)
            {
                Downgrade();
                return;
            }

			if (hp <= 0) 
			{
				SoundManager.instance.PlaySingle (gameOverSound);
				
				SoundManager.instance.musicSource.Stop();
				
				GameManager.instance.GameOver ();
			}
		}

        private void ResetColor()
        {
            sprite.color = Color.white;
        }

        private void OnDisable()
        {
            GameManager.instance.playerHP = hp;
            GameManager.instance.playerTP = tp;

            //Unregister event
            Enemy.EnemyDieEvent -= GainTechPoint;
        }

        private void OnEnable()
        {
            //Register event
            Enemy.EnemyDieEvent += GainTechPoint;
        }
    }
}

