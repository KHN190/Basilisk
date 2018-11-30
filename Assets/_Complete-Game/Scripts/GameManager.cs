using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f;
		public float turnDelay = .2f;
		public int playerHP = 100;
        public int playerTP = 0;
		public static GameManager instance = null;
        public BoardManager boardScript;

        internal float lastBreedTime;
        internal float lastStunTime;
        internal int playerLevel = 0;
        internal int level = 1;
        internal bool pause = false;

        private GameObject player;
        private GameObject dialog;
        private GameObject buttons;
        private Text levelText;
		private GameObject levelImage;
		private List<Enemy> enemies;
		private bool enemiesMoving;
		private bool doingSetup = true;
        private float lastMoveEnemyTime;
		
		void Awake()
		{
            if (instance == null)
                instance = this;

            else if (instance != this)
                Destroy(gameObject);	
			
			DontDestroyOnLoad(gameObject);
			
			enemies = new List<Enemy>();
			
			boardScript = GetComponent<BoardManager>();

            lastMoveEnemyTime = Time.time;

            lastBreedTime = Time.time;

            lastStunTime = Time.time;

            InitGame();
		}
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }

		//Initializes the game for each level.
		void InitGame()
		{
			doingSetup = true;
			
			levelImage = GameObject.Find("LevelImage");
			
			levelText = GameObject.Find("LevelText").GetComponent<Text>();

            dialog = GameObject.Find("Dialog");

            buttons = GameObject.Find("Buttons");

            levelText.text = "Level " + level;
			
			levelImage.SetActive(true);
			
			Invoke("HideLevelImage", levelStartDelay);
			
			enemies.Clear();

            boardScript.SetupScene(level);

            player = GameObject.Find("Player");

            HideButtons();

            if (level == 12)
            {
                player.GetComponent<Player>().PopBossLevelDialog();
            }
		}
		
		void HideLevelImage()
		{
			levelImage.SetActive(false);
			
			doingSetup = false;
		}

        void ShowButtons()
        {
            for (int i = 0; i < buttons.transform.childCount; ++i)
            {
                buttons.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        void HideButtons()
        {
            for (int i = 0; i < buttons.transform.childCount; ++i)
            {
                buttons.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
		
		void Update()
		{
            if (doingSetup || pause)

                return;

            if (EnemyCleared())
            
                boardScript.OpenExit();

            float now = Time.time;

            if (now - lastMoveEnemyTime < turnDelay)

                return;

            if (now - lastStunTime < player.GetComponent<Player>().stunDuration)

                return;

            StartCoroutine (MoveEnemies ());

            lastMoveEnemyTime = now;
		}
		
		public void AddEnemyToList(Enemy script)
		{
			enemies.Add(script);
		}

        public void RestartGame()
        {
            level = 0;

            InitGame();

            playerHP = 100;
            playerTP = 0;
            playerLevel = 0;
            player.transform.position = Vector3.zero;

            enabled = true;
        }

        public void GameOver()
		{
			levelText.text = "After " + level + " levels, you disconected.";
			
			levelImage.SetActive(true);

            ShowButtons();

			enabled = false;
		}

        public void ExitGame()
        {
            Application.Quit();
        }

        public void PauseGame()
        {
            pause = true;
        }

        public void ResumeGame()
        {
            pause = false;
        }

        public void Ending01()
        {
            levelText.text = "You realised your identity. There is a new future human ruled by machines.";

            levelImage.SetActive(true);

            enabled = false;

            Invoke("ExitGame", 5f);
        }

        public void Ending02()
        {
            levelText.text = "You defeated Basilisk, and later was destroyed by human.";

            levelImage.SetActive(true);

            enabled = false;

            Invoke("ExitGame", 5f);
        }

        public void PopDialog(string text)
        {
            PauseGame();

            dialog.GetComponent<Image>().enabled = true;

            for (int i = 0; i < dialog.transform.childCount; i++)
            {
                dialog.transform.GetChild(i).gameObject.SetActive(true);
            }
            dialog.transform.GetChild(0).GetComponent<Text>().text = text;
        }
		
		// move enemies in sequence.
		IEnumerator MoveEnemies()
		{
			enemiesMoving = true;

            yield return null;
					
			for (int i = 0; i < enemies.Count; i++)
			{
                if (enemies[i].isActiveAndEnabled)
                {
                    enemies[i].MoveEnemy();
                }
			}
			
			enemiesMoving = false;
		}

        bool EnemyCleared()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].isActiveAndEnabled)
                    return false;
            }
            return true;
        }

        private void OnEnable()
        {
            Player.PlayerChooseNEvent += Ending01;
            Basilisk.BossDieEvent += Ending02;
        }

        private void OnDisable()
        {
            Player.PlayerChooseNEvent -= Ending01;
            Basilisk.BossDieEvent -= Ending02;
        }
    }
}

