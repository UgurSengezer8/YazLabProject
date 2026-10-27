using System.Collections;
using Karantina13.Core;
using Karantina13.Puzzle;
using Karantina13.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Karantina13.Level
{
    public sealed class LevelExit : MonoBehaviour
    {
        private const float LoadDelay = 2f;

        [SerializeField] private string nextSceneName;
        [SerializeField] private string completeMessage = "Bölüm tamamlandı!";

        private bool reached;

        public event System.Action Reached;

        public void Configure(string nextSceneName, string completeMessage)
        {
            this.nextSceneName = nextSceneName;
            this.completeMessage = completeMessage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (reached || !IsPlayer(other)) return;
            reached = true;

            HintDisplay hints = FindFirstObjectByType<HintDisplay>();
            if (hints != null) hints.Show(completeMessage, 4f);
            Reached?.Invoke();

            if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
                StartCoroutine(LoadNextScene());
        }

        private IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(LoadDelay);
            SceneManager.LoadScene(nextSceneName);
        }

        private static bool IsPlayer(Collider2D other)
        {
            GameObject owner = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            int playerLayer = GameLayers.PlayerIndex;
            if (playerLayer >= 0) return owner.layer == playerLayer;
            return owner.GetComponent<KarantinaCharacterController>() != null && owner.GetComponent<BoxPusher>() == null;
        }
    }
}
