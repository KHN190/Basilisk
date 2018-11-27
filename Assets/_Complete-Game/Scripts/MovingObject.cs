using UnityEngine;
using System.Collections;

namespace Completed
{
    public abstract class MovingObject : MonoBehaviour
    {
		public float moveTime = 0.1f;
		public LayerMask blockingLayer;
		
		private BoxCollider2D boxCollider;
		private Rigidbody2D rb2D;
		private float inverseMoveTime;

        protected float speed = 1f;
        protected bool isMoving = false;

        /*
         * Protected Methods
        */

		protected virtual void Start ()
		{
			boxCollider = GetComponent <BoxCollider2D> ();
			
			rb2D = GetComponent <Rigidbody2D> ();
			
			inverseMoveTime = 1f / moveTime;
		}

		protected virtual bool Move (int xDir, int yDir, out RaycastHit2D hit)
		{
            Vector2 start = transform.position;
			
			Vector2 end = start + new Vector2 (xDir, yDir);

            hit = IsBlocked(start, end);

            if ((hit.transform == null) && !isMoving)
			{
                StartCoroutine (SmoothMovement (end));
				
				return true;
			}
			return false;
		}

        protected virtual IEnumerator SmoothMovement (Vector3 end)
		{
            isMoving = true;

			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			while(sqrRemainingDistance > float.Epsilon)
			{
                Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime *  speed * Time.deltaTime);
				
				rb2D.MovePosition (newPostion);
				
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;
				
				yield return null;
			}

            isMoving = false;
        }
		
		protected virtual void AttemptMove <T> (int xDir, int yDir)
			where T : Component
		{
			RaycastHit2D hit;
			
			bool canMove = Move (xDir, yDir, out hit);

			if(hit.transform == null)

				return;
                
			T hitComponent = hit.transform.GetComponent <T> ();
			
			if(!canMove && hitComponent != null)
				
				OnCantMove (hitComponent);
		}

		protected abstract void OnCantMove <T> (T component)
			where T : Component;

        // can be hit by raycast
        protected virtual RaycastHit2D IsBlocked(Vector2 start, Vector2 end)
        {
            boxCollider.enabled = false;

            RaycastHit2D hit = Physics2D.Linecast(start, end, blockingLayer);

            boxCollider.enabled = true;

            return hit;
        }

        // can be hit by raycast & is within 1 distance
        protected virtual bool IsAdjacent(Vector2 start, Vector2 end) 
        {
            if (Mathf.Abs(start.x - end.x) <= 1 || Mathf.Abs(start.y - end.y) <= 1)
            {
                return true;
            }
            return false;
        }

        protected virtual int CountAdj() 
        {
            Vector2 start = transform.position;
            Vector2 end = Vector2.zero;

            end.x = start.x - 1;
            end.y = start.y;

            int i = 0;

            if (IsBlocked(start, end))
                i++;

            end.x = start.x + 1;
            end.y = start.y;

            if (IsBlocked(start, end))
                i++;

            end.x = start.x;
            end.y = start.y - 1;

            if (IsBlocked(start, end))
                i++;

            end.x = start.x;
            end.y = start.y + 1;

            if (IsBlocked(start, end))
                i++;

            return i;
        }

        protected virtual Vector2 FindEmptyAdj() 
        {
            Vector2 start = transform.position;
            Vector2 end = Vector2.zero;

            end.x = start.x - 1;
            end.y = start.y;

            if (!IsBlocked(start, end))
                return end;

            end.x = start.x + 1;
            end.y = start.y;

            if (!IsBlocked(start, end))
                return end;

            end.x = start.x;
            end.y = start.y - 1;

            if (!IsBlocked(start, end))
                return end;

            end.x = start.x;
            end.y = start.y + 1;

            if (!IsBlocked(start, end))
                return end;

            return Vector2.negativeInfinity;
        }

        protected virtual Vector2 RandomAdj() 
        {
            Vector2 pos = transform.position;
            float seed = Random.value;
            if (seed < 0.25)
                return new Vector2(pos.x - 1, pos.y);
            if (seed < 0.5f)
                return new Vector2(pos.x, pos.y - 1);
            if (seed < 0.75)
                return new Vector2(pos.x + 1, pos.y);
            return new Vector2(pos.x, pos.y + 1);
        }
    }
}
