using UnityEditor;
using UnityEditor.SceneManagement;

public static class KarantinaSceneShortcuts
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Karantina 13/Sahneler/Ana Menu", priority = 0)]
    public static void OpenMainMenu() => Open(MainMenuPath);

    [MenuItem("Karantina 13/Sahneler/Bolum 1", priority = 11)]
    public static void OpenLevel01() => Open("Assets/Scenes/Level01.unity");

    [MenuItem("Karantina 13/Sahneler/Bolum 2", priority = 12)]
    public static void OpenLevel02() => Open("Assets/Scenes/Level02.unity");

    [MenuItem("Karantina 13/Sahneler/Bolum 3", priority = 13)]
    public static void OpenLevel03() => Open("Assets/Scenes/Level03.unity");

    [MenuItem("Karantina 13/Sahneler/Bolum 4", priority = 14)]
    public static void OpenLevel04() => Open("Assets/Scenes/Level04.unity");

    [MenuItem("Karantina 13/Sahneler/Robot Test", priority = 30)]
    public static void OpenRobotTest() => Open("Assets/Scenes/RobotTest.unity");

    [MenuItem("Karantina 13/Ana Menuden Oyna %#m", priority = -100)]
    public static void PlayFromMainMenu()
    {
        if (EditorApplication.isPlaying) return;
        if (!Open(MainMenuPath)) return;
        EditorApplication.isPlaying = true;
    }

    private static bool Open(string path)
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return false;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
        {
            EditorUtility.DisplayDialog("Sahne yok", path + " bulunamadi.", "Tamam");
            return false;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        EditorSceneManager.OpenScene(path);
        return true;
    }
}
