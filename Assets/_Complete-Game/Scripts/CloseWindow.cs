using UnityEngine;
using UnityEngine.UI;


namespace Completed
{
    public class CloseWindow : MonoBehaviour
    {

        private GameObject dialogBox;

        private void Start()
        {
            dialogBox = GameObject.Find("Dialog");
        }

        public void CloseDialog()
        {
            dialogBox.GetComponent<Image>().enabled = false;

            for (int i = 0; i < dialogBox.transform.childCount; ++i)
            {
                dialogBox.transform.GetChild(i).gameObject.SetActive(false);
            }

            GameManager.instance.ResumeGame();
        }

        private void CloseDialogAfterShortTime()
        {
            Invoke("CloseDialog", 0.5f);
        }

        private void OnEnable()
        {
            Player.PlayerCloseDialogEvent += CloseDialogAfterShortTime;
        }

        private void OnDisable()
        {
            Player.PlayerCloseDialogEvent -= CloseDialogAfterShortTime;
        }
    }
}
