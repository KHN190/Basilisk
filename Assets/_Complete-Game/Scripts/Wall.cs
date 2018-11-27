using UnityEngine;

namespace Completed
{
	public class Wall : MonoBehaviour
	{
        public GameObject[] items;

		public AudioClip chopSound;
		public Sprite dmgSprite;
		public int hp = 2;
		
		private SpriteRenderer spriteRenderer;

		void Awake ()
		{
			spriteRenderer = GetComponent<SpriteRenderer> ();
		}
		
		public void TakeDamage(int loss)
		{
            hp -= loss;

			SoundManager.instance.RandomizeSfx(chopSound);
			
			spriteRenderer.sprite = dmgSprite;

            if (hp <= 0)
            {
                float val = Random.value;

                if (val < 0.08f)
                {
                    DropItem();
                    return;
                }

                gameObject.SetActive(false);
            }
		}

        private void DropItem()
        {
            float value = Random.value;

            GameObject toDrop = items[Random.Range(0, items.Length)];

            Instantiate(toDrop, transform.position, Quaternion.identity);

            gameObject.SetActive(false);
        }
	}
}
