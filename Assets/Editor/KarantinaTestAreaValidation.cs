using System;
using System.Linq;
using Karantina13;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class KarantinaTestAreaValidation
{
    [MenuItem("Karantina 13/Test Alanini Dogrula")]
    public static void Validate()
    {
        KarantinaTestAreaBuilder.Build();
        var actors = UnityEngine.Object.FindObjectsByType<KarantinaCharacterController>(FindObjectsSortMode.None);
        Require(actors.Length == 2, "Exactly two independently controlled characters");
        Require(actors.Select(a => a.controls).Distinct().Count() == 2, "Separate control schemes");
        var keyboard = InputSystem.AddDevice<Keyboard>();
        try
        {
            Keys(keyboard, Key.W, Key.D);
            var diagonal = KarantinaCharacterController.ReadMovement(keyboard, KarantinaCharacterController.ControlScheme.WASD);
            Require(Mathf.Abs(diagonal.magnitude - 1f) < 0.0001f, "Normalized diagonal movement");
            Require(KarantinaCharacterController.ReadMovement(keyboard, KarantinaCharacterController.ControlScheme.ArrowKeys) == Vector2.zero, "WASD does not move the robot");
            Keys(keyboard, Key.LeftArrow, Key.UpArrow);
            Require(KarantinaCharacterController.ReadMovement(keyboard, KarantinaCharacterController.ControlScheme.WASD) == Vector2.zero, "Arrows do not move the player");
            Require(KarantinaCharacterController.ReadMovement(keyboard, KarantinaCharacterController.ControlScheme.ArrowKeys).x < 0, "Robot arrow direction");
            Keys(keyboard, Key.S, Key.RightArrow);
            Require(KarantinaCharacterController.ReadMovement(keyboard, KarantinaCharacterController.ControlScheme.WASD) == Vector2.down, "Simultaneous player control");
            Require(KarantinaCharacterController.ReadMovement(keyboard, KarantinaCharacterController.ControlScheme.ArrowKeys) == Vector2.right, "Simultaneous robot control");
            Keys(keyboard, Key.W, Key.S, Key.LeftArrow, Key.RightArrow);
            foreach (var actor in actors) Require(KarantinaCharacterController.ReadMovement(keyboard, actor.controls) == Vector2.zero, "Opposite keys cancel");
            Keys(keyboard);
            foreach (var actor in actors) Require(KarantinaCharacterController.ReadMovement(keyboard, actor.controls) == Vector2.zero, "Key release stops movement");
        }
        finally { InputSystem.RemoveDevice(keyboard); }

        foreach (var actor in actors)
        {
            var animator = actor.GetComponent<Animator>();
            Require(animator.runtimeAnimatorController.animationClips.Length == 8, "Eight directional idle/move clips: " + actor.name);
            animator.Rebind();
            foreach (string direction in new[] { "Down", "Left", "Right", "Up" })
            {
                foreach (string mode in new[] { "Idle_", "Move_" })
                {
                    string state = mode + direction;
                    Require(animator.HasState(0, Animator.StringToHash(state)), "Animator state exists: " + state);
                    animator.Play(state, 0, 0f);
                    animator.Update(0f);
                    Sprite first = actor.GetComponent<SpriteRenderer>().sprite;
                    Require(first != null && first.name.Contains(direction.ToLowerInvariant()), "Directional sprite: " + state);
                    animator.Update(1f / (actor.name == "Player" ? 8f : 7f) + 0.01f);
                    if (mode == "Move_") Require(actor.GetComponent<SpriteRenderer>().sprite != first, "Movement animation advances: " + state);
                }
            }
            animator.Play("Idle_Down", 0, 0f);
            animator.Update(0f);
        }
        Physics2D.SyncTransforms();
        Require(Physics2D.Raycast(new Vector2(0, 1), Vector2.up, 10).collider != null, "Room has collision geometry");
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("KARANTINA_VALIDATION_PASSED: controls, simultaneous input, diagonals, release, 16 animations, collision geometry.");
    }

    private static void Keys(Keyboard keyboard, params Key[] keys)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.Update();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Karantina validation failed: " + message);
        Debug.Log("Karantina PASS: " + message);
    }
}
