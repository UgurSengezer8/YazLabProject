using System;
using System.IO;
using System.Linq;
using Karantina13;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KarantinaTestAreaBuilder
{
    private const string Source = "Assets/Karantina13_Bolum01_Tam_Paket_v2/Assets/Karantina13";
    private const string Output = "Assets/Karantina13Test";
    public const string ScenePath = "Assets/Scenes/Karantina13_TestArea.unity";
    private static Material spriteMaterial;

    [MenuItem("Karantina 13/Test Alanini Olustur")]
    public static void BuildFromMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build();
    }

    public static void Build()
    {
        Directory.CreateDirectory(Output + "/Animations");
        Directory.CreateDirectory(Output + "/Prefabs");
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Source, "*.png", SearchOption.AllDirectories))
        {
            string assetPath = path.Replace('\\', '/');
            if (assetPath.Contains("/Previews/") || assetPath.Contains("/Spritesheets/") || assetPath.Contains("/Map/")) continue;
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.spritePivot = new Vector2(0.5f, 0f);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Output + "/KarantinaSprites.mat");
        if (spriteMaterial == null)
        {
            spriteMaterial = new Material(shader);
            AssetDatabase.CreateAsset(spriteMaterial, Output + "/KarantinaSprites.mat");
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var environment = new GameObject("Test Room").transform;
        var floors = new GameObject("Metal Floor").transform;
        floors.SetParent(environment);
        for (int x = 0; x < 12; x++)
        for (int y = 0; y < 7; y++)
        {
            string tile = (x < 2 || x > 9) && (y == 2 || y == 3) ? "floor_caution" :
                ((x + y * 3) % 9 == 0 ? "floor_worn_metal" : "floor_clean_metal");
            Visual("Floor " + x + "," + y, "Tiles/" + tile + ".png", new Vector2(-11 + x * 2, -7 + y * 2), new Vector2(2.01f, 2.01f), -1000, floors);
        }
        var walls = new GameObject("Walls and Bounds").transform;
        walls.SetParent(environment);
        for (int x = 0; x < 12; x++)
        {
            Visual("North Wall", "Tiles/wall_straight.png", new Vector2(-11 + x * 2, 6.2f), new Vector2(2.02f, 1.3f), -100, walls);
            Visual("South Wall", "Tiles/wall_straight.png", new Vector2(-11 + x * 2, -7.45f), new Vector2(2.02f, 0.9f), 400, walls);
        }
        for (int y = 0; y < 7; y++)
        {
            Visual("West Wall", "Tiles/wall_end_cap.png", new Vector2(-12.5f, -6.8f + y * 2), new Vector2(0.7f, 2.02f), 390, walls);
            Visual("East Wall", "Tiles/wall_end_cap.png", new Vector2(11.8f, -6.8f + y * 2), new Vector2(0.7f, 2.02f), 390, walls);
        }
        Solid("North Collision", new Vector2(0, 6.5f), new Vector2(25, 0.5f), walls);
        Solid("South Collision", new Vector2(0, -7), new Vector2(25, 0.5f), walls);
        Solid("West Collision", new Vector2(-12, 0), new Vector2(0.5f, 14), walls);
        Solid("East Collision", new Vector2(12, 0), new Vector2(0.5f, 14), walls);

        var props = new GameObject("Obstacle Course").transform;
        props.SetParent(environment);
        Prop("Lab Table", "Props/lab_table.png", new Vector2(-8.7f, 3.7f), new Vector2(2.5f, 1.6f), props);
        Prop("Computer Terminal", "Props/computer_terminal.png", new Vector2(-5.6f, 4.2f), new Vector2(1.5f, 1.5f), props);
        Prop("ARK 13", "Props/ark13_full_console.png", new Vector2(8.1f, 3.4f), new Vector2(3.4f, 2.5f), props);
        Prop("Crate A", "Props/crate.png", new Vector2(-5.8f, -4.3f), new Vector2(1.4f, 1.4f), props);
        Prop("Crate B", "Props/crate.png", new Vector2(-4.3f, -4.3f), new Vector2(1.4f, 1.4f), props);
        Prop("Barrel A", "Props/barrel.png", new Vector2(5.6f, -4.2f), new Vector2(0.8f, 1.4f), props);
        Prop("Barrel B", "Props/barrel.png", new Vector2(7f, -4.2f), new Vector2(0.8f, 1.4f), props);
        Prop("Evidence Terminal", "Props/evidence_terminal_static.png", new Vector2(0, 4.3f), new Vector2(2f, 1.8f), props);
        Visual("Warning", "Props/warning_sign.png", new Vector2(-10.5f, 5.3f), new Vector2(0.65f, 0.8f), -50, props);

        var player = Character("Player", new Vector2(-2.2f, -0.8f), 1.7f, 8f, KarantinaCharacterController.ControlScheme.WASD);
        var robot = Character("Robot", new Vector2(2.2f, -0.8f), 1.2f, 7f, KarantinaCharacterController.ControlScheme.ArrowKeys);
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.transform.position = new Vector3(0, 0.2f, -10);
        camera.orthographic = true;
        camera.orthographicSize = 8.8f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.045f, 0.065f);
        var area = new GameObject("Test Area Controls").AddComponent<KarantinaTestArea>();
        area.player = player;
        area.robot = robot;
        area.arenaCamera = camera;
        EditorSceneManager.SaveScene(scene, ScenePath);
        var existing = EditorBuildSettings.scenes.Where(s => s.path != ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }.Concat(existing).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("KARANTINA_TEST_AREA_READY: " + ScenePath);
    }

    private static GameObject Visual(string name, string spritePath, Vector2 position, Vector2 size, int sorting, Transform parent)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Source + "/" + spritePath);
        if (sprite == null) throw new InvalidOperationException("Missing sprite: " + spritePath);
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = spriteMaterial;
        renderer.sortingOrder = sorting;
        return go;
    }

    private static void Solid(string name, Vector2 position, Vector2 size, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    private static void Prop(string name, string path, Vector2 position, Vector2 size, Transform parent)
    {
        var go = Visual(name, path, position, size, 100 - Mathf.RoundToInt(position.y * 20), parent);
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(size.x * 0.8f / go.transform.localScale.x, size.y * 0.35f / go.transform.localScale.y);
        collider.offset = new Vector2(0, collider.size.y * 0.5f);
    }

    private static KarantinaCharacterController Character(string kind, Vector2 position, float height, float fps, KarantinaCharacterController.ControlScheme scheme)
    {
        string prefix = kind.ToLowerInvariant();
        var go = Visual(kind, "Characters/" + kind + "/Frames/" + prefix + "_down_0_idle.png", position, new Vector2(1, height), 100, null);
        float uniformScale = go.transform.localScale.y;
        go.transform.localScale = Vector3.one * uniformScale;
        var body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var collider = go.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.65f, 0.38f) / uniformScale;
        collider.offset = new Vector2(0, 0.22f) / uniformScale;
        collider.direction = CapsuleDirection2D.Horizontal;
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = Controller(kind, fps);
        var controller = go.AddComponent<KarantinaCharacterController>();
        controller.controls = scheme;
        controller.characterLabel = kind.ToUpperInvariant();
        controller.moveSpeed = kind == "Robot" ? 4.5f : 4f;
        PrefabUtility.SaveAsPrefabAsset(go, Output + "/Prefabs/" + kind + ".prefab");
        return controller;
    }

    private static AnimatorController Controller(string kind, float fps)
    {
        string path = Output + "/Animations/" + kind + ".controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller != null) return controller;
        controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var machine = controller.layers[0].stateMachine;
        int row = 0;
        foreach (string direction in new[] { "Down", "Left", "Right", "Up" })
        {
            string folder = Source + "/Characters/" + kind + "/Frames/";
            string prefix = kind.ToLowerInvariant() + "_" + direction.ToLowerInvariant() + "_";
            var sprites = Directory.GetFiles(folder, prefix + "*.png").OrderBy(p => p, StringComparer.Ordinal)
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
            if (sprites.Length != 5 || sprites.Any(s => s == null)) throw new InvalidOperationException("Expected 5 frames: " + prefix);
            for (int moving = 0; moving < 2; moving++)
            {
                string name = (moving == 1 ? "Move_" : "Idle_") + direction;
                Sprite[] frames = moving == 1 ? sprites.Skip(1).ToArray() : new[] { sprites[0] };
                var clip = new AnimationClip { name = kind + "_" + name, frameRate = fps };
                var keys = new ObjectReferenceKeyframe[frames.Length + 1];
                for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
                keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[0] };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                AssetDatabase.CreateAsset(clip, Output + "/Animations/" + kind + "_" + name + ".anim");
                var state = machine.AddState(name, new Vector3(250 * moving, 65 * row, 0));
                state.motion = clip;
                if (row == 0 && moving == 0) machine.defaultState = state;
            }
            row++;
        }
        return controller;
    }
}
