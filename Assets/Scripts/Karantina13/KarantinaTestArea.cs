using UnityEngine;
using UnityEngine.InputSystem;

namespace Karantina13
{
    public sealed class KarantinaTestArea : MonoBehaviour
    {
        public KarantinaCharacterController player;
        public KarantinaCharacterController robot;
        public Camera arenaCamera;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                player.ResetToSpawn();
                robot.ResetToSpawn();
            }
            // Keep the whole test room visible, including in a narrow Game view.
            if (arenaCamera != null)
            {
                float scale = Mathf.Clamp(Screen.width / 1100f, 0.55f, 1.5f);
                float top = Mathf.Min(0.35f, 112f * scale / Screen.height);
                float bottom = Mathf.Min(0.18f, 52f * scale / Screen.height);
                arenaCamera.rect = new Rect(0f, bottom, 1f, 1f - top - bottom);
                float aspect = Screen.width / (Screen.height * arenaCamera.rect.height);
                arenaCamera.orthographicSize = Mathf.Max(8.2f, 13f / Mathf.Max(0.1f, aspect));
            }
        }

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                titleStyle.normal.textColor = new Color(0.4f, 0.94f, 1f);
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 15 };
                textStyle.normal.textColor = new Color(0.86f, 0.91f, 0.95f);
            }
            float scale = Mathf.Clamp(Screen.width / 1100f, 0.55f, 1.5f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            GUI.color = new Color(0.025f, 0.045f, 0.065f, 1f);
            GUI.DrawTexture(new Rect(0, 0, width, 112), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height / scale - 52, width, 52), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Box(new Rect(12, 12, width - 24, 90), GUIContent.none);
            GUI.Label(new Rect(28, 20, width - 50, 30), "KARANTINA 13 / TEST ALANI", titleStyle);
            GUI.Label(new Rect(28, 54, width - 50, 24), "PLAYER: W A S D     |     ROBOT: OK TUŞLARI     |     R: BAŞLANGICA DÖN", textStyle);
            GUI.Label(new Rect(28, 76, width - 50, 22), "Hareket etmek için Game penceresine tıkla. İki karakter aynı anda kontrol edilebilir.", textStyle);
            float bottom = Screen.height / scale - 46;
            GUI.Box(new Rect(12, bottom, width - 24, 34), GUIContent.none);
            GUI.Label(new Rect(28, bottom + 6, width - 50, 24),
                Status(player) + "       |       " + Status(robot), textStyle);
            GUI.matrix = Matrix4x4.identity;
        }

        private static string Status(KarantinaCharacterController character)
        {
            if (character == null) return "";
            return character.characterLabel + ": " + (character.IsMoving ? "HAREKET" : "BEKLEME") + " / " + character.Facing.ToUpperInvariant();
        }
    }
}
