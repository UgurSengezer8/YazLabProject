using System;
using System.IO;
using System.Linq;
using Karantina13;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[InitializeOnLoad]
public static class KarantinaPlayValidation
{
    private const string Request = "Library/KarantinaPlayCheck.request";
    private const string Report = "Library/KarantinaPlayCheck.txt";
    private const string Running = "Karantina13.PlayValidation";
    private static Keyboard keyboard;
    private static KarantinaCharacterController player;
    private static KarantinaCharacterController robot;
    private static Vector2 playerStart;
    private static Vector2 robotStart;
    private static int step;
    private static float nextTime;

    static KarantinaPlayValidation() => EditorApplication.update += Tick;

    [MenuItem("Karantina 13/Play Modunda Kontrolleri Dogrula")]
    public static void Begin()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != KarantinaTestAreaBuilder.ScenePath)
            throw new InvalidOperationException("Open the Karantina13_TestArea scene first.");
        SessionState.SetBool(Running, true);
        step = 0;
        File.WriteAllText(Report, "RUNNING");
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (File.Exists(Request) && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            File.Delete(Request);
            Begin();
        }
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying || Time.time < 0.5f) return;
        if (Time.time < nextTime) return;
        try
        {
            if (step == 0)
            {
                var actors = UnityEngine.Object.FindObjectsByType<KarantinaCharacterController>(FindObjectsSortMode.None);
                player = actors.Single(a => a.controls == KarantinaCharacterController.ControlScheme.WASD);
                robot = actors.Single(a => a.controls == KarantinaCharacterController.ControlScheme.ArrowKeys);
                playerStart = player.transform.position;
                robotStart = robot.transform.position;
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Keys(Key.D, Key.UpArrow);
            }
            else if (step == 1)
            {
                Check(player.transform.position.x > playerStart.x + 0.2f, "Player moves with D");
                Check(robot.transform.position.y > robotStart.y + 0.2f, "Robot moves simultaneously with Up");
                Check(player.Facing == "Right" && robot.Facing == "Up", "Both directional animations respond");
                Keys();
            }
            else if (step == 2)
            {
                Check(!player.IsMoving && !robot.IsMoving, "Release enters idle");
                Check(player.GetComponent<Rigidbody2D>().linearVelocity == Vector2.zero, "Player stops");
                Check(robot.GetComponent<Rigidbody2D>().linearVelocity == Vector2.zero, "Robot stops");
                Keys(Key.R);
            }
            else if (step == 3)
            {
                Check(Vector2.Distance(player.transform.position, playerStart) < 0.05f, "R resets player");
                Check(Vector2.Distance(robot.transform.position, robotStart) < 0.05f, "R resets robot");
                Keys();
                player.GetComponent<Rigidbody2D>().position = new Vector2(11.3f, 0);
                Keys(Key.D);
            }
            else if (step == 4)
            {
                Check(player.transform.position.x < 11.6f, "East wall blocks movement");
                Keys();
                player.ResetToSpawn();
                robot.ResetToSpawn();
                ScreenCapture.CaptureScreenshot("Library/Karantina13_TestArea.png");
            }
            else
            {
                Finish("PASSED: Play mode simultaneous movement, facing animations, release/idle, R reset and physical wall collision.");
                return;
            }
            step++;
            nextTime = Time.time + 0.4f;
        }
        catch (Exception error) { Finish("FAILED: " + error); }
    }

    private static void Keys(params Key[] keys)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    }

    private static void Check(bool valid, string description)
    {
        if (!valid) throw new InvalidOperationException(description);
    }

    private static void Finish(string result)
    {
        SessionState.SetBool(Running, false);
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        File.WriteAllText(Report, result);
        Debug.Log("KARANTINA_PLAY_VALIDATION: " + result);
        EditorApplication.ExitPlaymode();
    }
}
