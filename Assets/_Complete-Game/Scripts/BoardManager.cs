using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed
	
{
	
	public class BoardManager : MonoBehaviour
	{		
		[Serializable]
		public class Count
		{
			public int minimum;
			public int maximum;
			
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}
		
		public int columns = 20;
		public int rows = 8;
		public Count wallCount = new Count (6, 10);
		public Count foodCount = new Count (0, 2);					
		public GameObject exit;
        public GameObject disc;
		public GameObject[] floorTiles;
		public GameObject[] wallTiles;
		public GameObject[] foodTiles;
		public GameObject[] enemyTiles;
		public GameObject[] outerWallTiles;
		
		private Transform boardHolder;
		private List <Vector3> gridPositions = new List <Vector3> ();
		
		
		void InitialiseList ()
		{
			gridPositions.Clear ();
			
			for(int x = 1; x < columns-1; x++)
			{
				for(int y = 1; y < rows-1; y++)
				{
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}
		
		
		void BoardSetup ()
		{
			boardHolder = new GameObject ("Board").transform;
			
			for(int x = -1; x < columns + 1; x++)
			{
				for(int y = -1; y < rows + 1; y++)
				{
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
					
					if(x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
					
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					
					instance.transform.SetParent (boardHolder);
				}
			}
		}
		
		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition ()
		{
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			Vector3 randomPosition = gridPositions[randomIndex];
			
			gridPositions.RemoveAt (randomIndex);
			
			return randomPosition;
		}

        Vector3 RandomPositionExceptAroundPlayer()
        {
            int range = 5;

            List<Vector3> properPositions = new List<Vector3>();

            List<int> indexGrid = new List<int>();

            int i = 0;

            foreach (Vector3 pos in gridPositions)
            {
                if (pos.x >= range && pos.y >= range)
                {
                    properPositions.Add(pos);

                    indexGrid.Add(i);
                }
                i++;
            }

            int randomIndex = Random.Range(0, properPositions.Count);

            Vector3 randomPosition = properPositions[randomIndex];

            gridPositions.RemoveAt(indexGrid[randomIndex]);

            return randomPosition;
        }
		
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
		{
			int objectCount = Random.Range (minimum, maximum+1);
			
			for(int i = 0; i < objectCount; i++)
			{
				Vector3 randomPosition = RandomPosition();
				
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
			
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}

        void LayoutObjectAtRandomExceptAroundPlayer(GameObject[] tileArray, int minimum, int maximum)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = RandomPositionExceptAroundPlayer();

                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                
                tileChoice.name = "enemy" + randomPosition; 

                Instantiate(tileChoice, randomPosition, Quaternion.identity);
            }
        }


        public void SetupScene (int level)
		{
			BoardSetup ();
			
			InitialiseList ();
			
			LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
			
			LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);

            int enemyCount = Mathf.CeilToInt(level * 1f / 2) + 4;

            LayoutObjectAtRandomExceptAroundPlayer(enemyTiles, enemyCount, enemyCount);
			
			Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);

            Instantiate(disc, new Vector3(0, 1, 0), Quaternion.identity);

            DisableExit();
		}

        public void LayoutEnemyAt(Vector2 pos)
        {
            GameObject tileChoice = enemyTiles[Random.Range(0, enemyTiles.Length)];

            Instantiate(tileChoice, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        }

        public void LayoutWallAt(Vector2 pos)
        {
            GameObject tileChoice = wallTiles[Random.Range(0, wallTiles.Length)];

            Instantiate(tileChoice, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        }

        public void DisableExit()
        {
            exit.GetComponent<SpriteRenderer>().enabled = false;
            exit.GetComponent<Collider2D>().enabled = false;
        }

        public void OpenExit() 
        {
            GameObject target = GameObject.FindGameObjectWithTag("Exit");
            if (target)
            {
                target.GetComponent<SpriteRenderer>().enabled = true;
                target.GetComponent<Collider2D>().enabled = true;
            }
        }
    }
}
