using System;
using System.Collections.Generic;
using System.Linq;
using Karantina13;
using Karantina13.Core;
using Karantina13.Level;
using Karantina13.Puzzle;
using Karantina13.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level01Builder
{
    public const string ScenePath = "Assets/Scenes/Level01.unity";
    private const string ArtRoot = "Assets/Karantina13_Bolum01_Tam_Paket_v2/Assets/Karantina13/";
    private const string MaterialPath = "Assets/Karantina13Test/KarantinaSprites.mat";
    private const string PlayerPrefabPath = "Assets/Karantina13Test/Prefabs/Player.prefab";
    private const string RobotPrefabPath = "Assets/Karantina13Test/Prefabs/Robot.prefab";
    private const float CellSize = 2f;
    private const int SortBase = 100;

    private static readonly string[] RequiredLayers =
    {
        GameLayers.Player, GameLayers.Companion, GameLayers.Pushable, GameLayers.Hazard, GameLayers.Environment
    };

    private static readonly Vector2 TileSize = new Vector2(2.02f, 2.02f);
    private static readonly Vector2 WallSize = new Vector2(2.02f, 2.1f);
    private static readonly Vector2 FrameSize = new Vector2(2.4f, 2.4f);
    private const float FramePadding = 0.2f;
    private const float StripThickness = 0.5f;
    private const int StripOrder = -800;

    private static readonly HashSet<Vector2Int> wallCells = new HashSet<Vector2Int>();
    private static readonly HashSet<Vector2Int> floorCells = new HashSet<Vector2Int>();
    private static readonly HashSet<string> placedStrips = new HashSet<string>();
    private static Material spriteMaterial;
    private static Transform floorRoot;
    private static Transform geometryRoot;
    private static Transform decorRoot;
    private static Transform puzzleRoot;
    private static Transform triggerRoot;

    [MenuItem("Karantina 13/Bolum 1 Olustur")]
    public static void BuildFromMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build();
    }

    public static void Build()
    {
        EnsureLayers();
        ConfigurePhysics();
        PrepareSprites();
        spriteMaterial = LoadSpriteMaterial();
        wallCells.Clear();
        floorCells.Clear();
        placedStrips.Clear();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateLevelHierarchy();
        BuildFloors();
        BuildWalls();
        BuildPuzzles();
        BuildDecor();
        BuildTriggers();

        GameObject player = SpawnCharacter(PlayerPrefabPath, "Player", new Vector2Int(2, 3), GameLayers.Player);
        GameObject robot = SpawnCharacter(RobotPrefabPath, "Robot", new Vector2Int(3, 2), GameLayers.Companion);
        SetupRobot(robot);
        CreateCamera(player.transform);
        CreateSystems(player.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        PutSceneFirstInBuild();
        AssetDatabase.SaveAssets();
        Debug.Log("Level01 olusturuldu");
    }

    private static void EnsureLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        foreach (string layerName in RequiredLayers)
        {
            if (HasLayer(layers, layerName)) continue;
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue)) continue;
                slot.stringValue = layerName;
                break;
            }
        }
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool HasLayer(SerializedProperty layers, string layerName)
    {
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return true;
        }
        return false;
    }

    private static int LayerIndex(string layerName)
    {
        int index = LayerMask.NameToLayer(layerName);
        if (index < 0) throw new InvalidOperationException("Layer bulunamadi: " + layerName);
        return index;
    }

    private static void ConfigurePhysics()
    {
        Physics2D.gravity = Vector2.zero;
        GameLayers.ApplyCollisionRules();
        Physics2D.IgnoreLayerCollision(LayerIndex(GameLayers.Companion), LayerIndex(GameLayers.Hazard), true);
        AssetDatabase.SaveAssets();
    }

    private static void PrepareSprites()
    {
        foreach (string path in UsedSpritePaths()) EnsureSpriteImport(ArtRoot + path);
    }

    private static IEnumerable<string> UsedSpritePaths()
    {
        string[] tiles =
        {
            "floor_clean_metal", "floor_worn_metal", "floor_caution", "floor_electric_hazard",
            "glow_light_tile", "wall_straight", "wall_side", "wall_bottom"
        };
        string[] props = { "crate", "barrel", "lab_table", "computer_terminal", "warning_sign", "wall_control_panel" };
        foreach (string tile in tiles) yield return TilePath(tile);
        foreach (string prop in props) yield return PropPath(prop);
        for (int i = 0; i < 4; i++)
        {
            yield return BarrierFrame(i);
            yield return DoorFrame(i);
        }
    }

    private static string TilePath(string name) => "Tiles/" + name + ".png";
    private static string PropPath(string name) => "Props/" + name + ".png";
    private static string BarrierFrame(int index) => "Animations/ElectricBarrier/electric_barrier_0" + index + ".png";
    private static string DoorFrame(int index) => "Animations/SlidingDoor/sliding_door_0" + index + ".png";

    private static void EnsureSpriteImport(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Sprite bulunamadi: " + assetPath);
        bool ready = importer.textureType == TextureImporterType.Sprite
            && importer.spriteImportMode == SpriteImportMode.Single
            && Mathf.Approximately(importer.spritePixelsPerUnit, 128f)
            && importer.filterMode == FilterMode.Point
            && importer.textureCompression == TextureImporterCompression.Uncompressed;
        if (ready) return;

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

    private static Material LoadSpriteMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        material = new Material(shader);
        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }

    private static void CreateLevelHierarchy()
    {
        Transform level = new GameObject("Level").transform;
        floorRoot = CreateChild("Floor", level);
        geometryRoot = CreateChild("Geometry", level);
        decorRoot = CreateChild("Decor", level);
        puzzleRoot = CreateChild("Puzzles", level);
        triggerRoot = CreateChild("Triggers", level);
    }

    private static Transform CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Vector2 CellCenter(Vector2Int cell) => new Vector2(cell.x * CellSize, cell.y * CellSize);
    private static Vector2 CellBottom(Vector2Int cell) => CellCenter(cell) + Vector2.down * (CellSize * 0.5f);
    private static int SortFor(float y) => SortBase - Mathf.RoundToInt(y * 20f);

    private static void BuildFloors()
    {
        FillRoomFloor(0, 7, 0, 6);
        FillRoomFloor(12, 21, 0, 8);
        for (int x = 8; x <= 11; x++) PlaceFloor(new Vector2Int(x, 4), "floor_worn_metal");
        for (int x = 22; x <= 25; x++) PlaceFloor(new Vector2Int(x, 3), "floor_worn_metal");
    }

    private static void FillRoomFloor(int minX, int maxX, int minY, int maxY)
    {
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var cell = new Vector2Int(x, y);
                PlaceFloor(cell, PickRoomTile(cell, y == maxY));
            }
        }
    }

    private static string PickRoomTile(Vector2Int cell, bool northRow)
    {
        if (northRow && cell.x % 3 == 1) return "glow_light_tile";
        int hash = Mathf.Abs((cell.x * 73856093) ^ (cell.y * 19349663));
        return hash % 5 == 0 ? "floor_worn_metal" : "floor_clean_metal";
    }

    private static void PlaceFloor(Vector2Int cell, string tile)
    {
        floorCells.Add(cell);
        CenteredSprite("Floor " + cell.x + "," + cell.y, TilePath(tile), CellCenter(cell), TileSize, -1000, floorRoot);
    }

    private static void BuildWalls()
    {
        var runs = new List<RectInt>
        {
            Run(-1, -1, 8, -1), Run(-1, 7, 8, 7), Run(-1, 0, -1, 6), Run(8, 0, 8, 3), Run(8, 5, 8, 6),
            Run(9, 3, 10, 3), Run(9, 5, 10, 5),
            Run(11, -1, 22, -1), Run(11, 9, 22, 9), Run(11, 0, 11, 3), Run(11, 5, 11, 8),
            Run(22, 0, 22, 2), Run(22, 4, 22, 8),
            Run(23, 2, 26, 2), Run(23, 4, 26, 4), Run(26, 3, 26, 3)
        };
        foreach (RectInt run in runs)
        {
            foreach (Vector2Int cell in run.allPositionsWithin) wallCells.Add(cell);
        }
        foreach (Vector2Int cell in wallCells.OrderByDescending(c => c.y).ThenBy(c => c.x))
        {
            if (floorCells.Contains(cell + Vector2Int.down)) PlaceFrontWall(cell);
        }
        foreach (Vector2Int floor in floorCells.OrderBy(c => c.y).ThenBy(c => c.x)) PlaceStripsAround(floor);
        for (int i = 0; i < runs.Count; i++) PlaceWallCollider("Wall Collider " + (i + 1), runs[i]);
    }

    private static RectInt Run(int fromX, int fromY, int toX, int toY)
    {
        return new RectInt(fromX, fromY, toX - fromX + 1, toY - fromY + 1);
    }

    private static void PlaceFrontWall(Vector2Int cell)
    {
        GameObject wall = BottomSprite("Wall " + cell.x + "," + cell.y, TilePath("wall_straight"), CellBottom(cell), WallSize, 0, geometryRoot);
        wall.layer = LayerIndex(GameLayers.Environment);
        AddYSort(wall, true);
    }

    private static void PlaceStripsAround(Vector2Int floor)
    {
        bool wallLeft = wallCells.Contains(floor + Vector2Int.left);
        bool wallRight = wallCells.Contains(floor + Vector2Int.right);
        bool wallBelow = wallCells.Contains(floor + Vector2Int.down);
        bool wallAbove = wallCells.Contains(floor + Vector2Int.up);

        if (wallLeft)
        {
            PlaceSideStrip(floor + Vector2Int.left, 1);
            if (wallAbove && wallCells.Contains(floor + Vector2Int.left + Vector2Int.up)) PlaceSideStrip(floor + Vector2Int.left + Vector2Int.up, 1);
        }
        if (wallRight)
        {
            PlaceSideStrip(floor + Vector2Int.right, -1);
            if (wallAbove && wallCells.Contains(floor + Vector2Int.right + Vector2Int.up)) PlaceSideStrip(floor + Vector2Int.right + Vector2Int.up, -1);
        }
        if (wallBelow) PlaceBottomStrip(floor + Vector2Int.down);

        float half = CellSize * 0.5f;
        Vector2 center = CellCenter(floor);
        if (wallBelow && wallLeft) PlaceStripCap(center + new Vector2(-half - StripThickness * 0.5f, -half - StripThickness * 0.5f));
        if (wallBelow && wallRight) PlaceStripCap(center + new Vector2(half + StripThickness * 0.5f, -half - StripThickness * 0.5f));
    }

    private static void PlaceSideStrip(Vector2Int wall, int towardFloor)
    {
        if (!placedStrips.Add("S" + wall + towardFloor)) return;
        float offset = (CellSize - StripThickness) * 0.5f * towardFloor;
        Vector2 center = CellCenter(wall) + new Vector2(offset, 0f);
        CenteredStrip("Side Wall " + wall.x + "," + wall.y, "wall_side", center, new Vector2(StripThickness, CellSize + 0.02f));
    }

    private static void PlaceBottomStrip(Vector2Int wall)
    {
        if (!placedStrips.Add("B" + wall)) return;
        Vector2 center = CellCenter(wall) + new Vector2(0f, (CellSize - StripThickness) * 0.5f);
        CenteredStrip("Bottom Wall " + wall.x + "," + wall.y, "wall_bottom", center, new Vector2(CellSize + 0.02f, StripThickness));
    }

    private static void PlaceStripCap(Vector2 center)
    {
        if (!placedStrips.Add("C" + center)) return;
        CenteredStrip("Wall Cap", "wall_bottom", center, new Vector2(StripThickness, StripThickness));
    }

    private static void CenteredStrip(string name, string tile, Vector2 center, Vector2 size)
    {
        GameObject strip = CenteredSprite(name, TilePath(tile), center, size, StripOrder, geometryRoot);
        strip.layer = LayerIndex(GameLayers.Environment);
    }

    private static void PlaceWallCollider(string name, RectInt run)
    {
        var go = new GameObject(name);
        go.transform.SetParent(geometryRoot, false);
        go.layer = LayerIndex(GameLayers.Environment);
        Vector2 size = new Vector2(run.width * CellSize, run.height * CellSize);
        Vector2 min = new Vector2(run.xMin * CellSize, run.yMin * CellSize) - Vector2.one * (CellSize * 0.5f);
        go.transform.localPosition = min + size * 0.5f;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    private static void BuildPuzzles()
    {
        SlidingDoor doorA = CreateSlidingDoor("Door A", new Vector2Int(8, 4));
        SlidingDoor doorB = CreateSlidingDoor("Door B", new Vector2Int(22, 3));
        ElectricFence fence = CreateElectricFence(17, 0, 8);

        CreatePushableBox("Crate A", new Vector2Int(4, 4));
        CreatePushableBox("Crate B", new Vector2Int(19, 6));

        CreatePressurePlate("Plate A", new Vector2Int(6, 4), new MonoBehaviour[] { doorA });
        CreatePressurePlate("Plate B", new Vector2Int(19, 3), new MonoBehaviour[] { fence, doorB });
    }

    private static SlidingDoor CreateSlidingDoor(string name, Vector2Int cell)
    {
        var root = new GameObject(name);
        root.transform.SetParent(puzzleRoot, false);
        root.transform.localPosition = CellCenter(cell);

        var blocker = root.AddComponent<BoxCollider2D>();
        blocker.size = new Vector2(CellSize, CellSize);

        Vector2 anchor = Vector2.down * (CellSize * 0.5f + FramePadding);
        GameObject visual = BottomSprite("Visual", DoorFrame(0), anchor, FrameSize, 0, root.transform);
        AddYSort(visual, true);
        var animator = visual.AddComponent<SpriteFrameAnimator>();
        animator.Configure(LoadFrames(DoorFrame), 8f, false);

        var door = root.AddComponent<SlidingDoor>();
        door.Configure(blocker, animator);
        SetLayerRecursively(root, LayerIndex(GameLayers.Environment));
        return door;
    }

    private static ElectricFence CreateElectricFence(int column, int fromRow, int toRow)
    {
        var root = new GameObject("Electric Fence");
        root.transform.SetParent(puzzleRoot, false);

        Sprite[] frames = LoadFrames(BarrierFrame);
        var blockers = new List<Collider2D>();
        var sparks = new List<SpriteFrameAnimator>();
        var offVisuals = new List<SpriteRenderer>();

        for (int row = fromRow; row <= toRow; row++)
        {
            var segment = new GameObject("Barrier " + row);
            segment.transform.SetParent(root.transform, false);
            segment.transform.localPosition = CellCenter(new Vector2Int(column, row));

            var blocker = segment.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(CellSize * 0.8f, CellSize);
            blockers.Add(blocker);

            GameObject off = CenteredSprite("Off", TilePath("floor_electric_hazard"), Vector2.zero, TileSize, -960, segment.transform);
            offVisuals.Add(off.GetComponent<SpriteRenderer>());

            Vector2 anchor = Vector2.down * (CellSize * 0.5f + FramePadding);
            GameObject spark = BottomSprite("Spark", BarrierFrame(0), anchor, FrameSize, -950, segment.transform);
            var animator = spark.AddComponent<SpriteFrameAnimator>();
            animator.Configure(frames, 10f, true);
            sparks.Add(animator);
        }

        var fence = root.AddComponent<ElectricFence>();
        fence.Configure(blockers.ToArray(), sparks.ToArray(), offVisuals.ToArray());
        SetLayerRecursively(root, LayerIndex(GameLayers.Hazard));
        return fence;
    }

    private static void CreatePushableBox(string name, Vector2Int cell)
    {
        var root = new GameObject(name);
        root.transform.SetParent(puzzleRoot, false);
        root.transform.localPosition = CellCenter(cell);

        var body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        var collider = root.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.6f, 1.2f);
        collider.offset = new Vector2(0f, -0.05f);
        root.AddComponent<PushableBox>();

        GameObject visual = BottomSprite("Visual", PropPath("crate"), new Vector2(0f, -0.65f), SizeFor(PropPath("crate"), 1.7f), 0, root.transform);
        AddYSort(visual, false);
        SetLayerRecursively(root, LayerIndex(GameLayers.Pushable));
    }

    private static void CreatePressurePlate(string name, Vector2Int cell, MonoBehaviour[] targets)
    {
        var root = new GameObject(name);
        root.transform.SetParent(puzzleRoot, false);
        root.transform.localPosition = CellCenter(cell);
        GameObject visual = CenteredSprite("Visual", TilePath("floor_caution"), Vector2.zero, new Vector2(1.8f, 1.8f), -900, root.transform);
        root.AddComponent<PressurePlate>().Configure(targets, visual.GetComponent<SpriteRenderer>());
    }

    private static void BuildDecor()
    {
        PlaceProp("Lab Table", "lab_table", new Vector2(2.6f, 11.1f), 2.5f);
        PlaceProp("Computer Terminal", "computer_terminal", new Vector2(9.6f, 11.2f), 1.5f);
        PlaceProp("Barrel A1", "barrel", new Vector2(0.3f, -0.8f), 0.8f);
        PlaceProp("Barrel A2", "barrel", new Vector2(1.4f, -0.8f), 0.8f);
        PlaceProp("Warning Plate A", "warning_sign", new Vector2(13.6f, 10.4f), 0.7f);
        PlaceWallDecor("Control Panel", "wall_control_panel", new Vector2(6f, 13.5f), 0.7f, 13f);

        PlaceProp("Warning Fence North", "warning_sign", new Vector2(31.6f, 12.4f), 0.7f);
        PlaceProp("Warning Fence South", "warning_sign", new Vector2(36.4f, 0.2f), 0.7f);
        PlaceProp("Barrel B1", "barrel", new Vector2(24.4f, 15.2f), 0.8f);
        PlaceProp("Barrel B2", "barrel", new Vector2(25.6f, 15.2f), 0.8f);
        PlaceProp("Barrel B3", "barrel", new Vector2(42.4f, -0.8f), 0.8f);
        PlaceProp("Crate Stack 1", "crate", new Vector2(24.6f, -0.8f), 1.5f);
        PlaceProp("Crate Stack 2", "crate", new Vector2(26.4f, -0.8f), 1.5f);
        PlaceProp("Crate Stack 3", "crate", new Vector2(42.2f, 15.2f), 1.5f);
        PlaceProp("Computer Terminal B", "computer_terminal", new Vector2(28f, 15.2f), 1.5f);
    }

    private static void PlaceProp(string name, string sprite, Vector2 feet, float width)
    {
        Vector2 size = SizeFor(PropPath(sprite), width);
        var root = new GameObject(name);
        root.transform.SetParent(decorRoot, false);
        root.transform.localPosition = feet;

        var collider = root.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(size.x * 0.8f, size.y * 0.35f);
        collider.offset = new Vector2(0f, collider.size.y * 0.5f);

        GameObject visual = BottomSprite("Visual", PropPath(sprite), Vector2.zero, size, 0, root.transform);
        AddYSort(visual, true);
        SetLayerRecursively(root, LayerIndex(GameLayers.Environment));
    }

    private static void PlaceWallDecor(string name, string sprite, Vector2 position, float width, float wallBaseY)
    {
        Vector2 size = SizeFor(PropPath(sprite), width);
        GameObject decor = BottomSprite(name, PropPath(sprite), position, size, SortFor(wallBaseY) + 1, decorRoot);
        decor.layer = LayerIndex(GameLayers.Environment);
    }

    private static void BuildTriggers()
    {
        CreateHintZone("Hint Start", CellCenter(new Vector2Int(2, 3)), new Vector2(6f, 6f),
            "Kapıyı aç: Robota kutuyu butona ittir (Q)", "Q tuşuyla robota komut verebilirsin.");
        CreateHintZone("Hint Electric Room", CellCenter(new Vector2Int(12, 4)), new Vector2(2f, 6f),
            "Elektriği kes: Robot tellerden geçebilir", "Tellere dokunamam ama robot yalıtımlı, o geçebilir.");

        GameObject exit = CreateTrigger("Level Exit", CellCenter(new Vector2Int(25, 3)), new Vector2(CellSize, CellSize));
        exit.AddComponent<LevelExit>().Configure("Level02", "Bölüm 1 tamamlandı!");
    }

    private static void CreateHintZone(string name, Vector2 center, Vector2 size, string objective, string message)
    {
        GameObject zone = CreateTrigger(name, center, size);
        zone.AddComponent<HintZone>().Configure(objective, message);
    }

    private static GameObject CreateTrigger(string name, Vector2 center, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(triggerRoot, false);
        go.transform.localPosition = center;
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.isTrigger = true;
        return go;
    }

    private static GameObject SpawnCharacter(string prefabPath, string name, Vector2Int cell, string layerName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) throw new InvalidOperationException("Prefab bulunamadi, once test alanini olusturun: " + prefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.position = CellCenter(cell);
        SetLayerRecursively(instance, LayerIndex(layerName));

        var body = instance.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }
        return instance;
    }

    private static void SetupRobot(GameObject robot)
    {
        if (robot.GetComponent<BoxPusher>() == null) robot.AddComponent<BoxPusher>();
        var controller = robot.GetComponent<KarantinaCharacterController>();
        if (controller != null) controller.controls = KarantinaCharacterController.ControlScheme.ArrowKeys;
    }

    private static void CreateCamera(Transform player)
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(player.position.x, player.position.y, -10f);

        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(0x0E, 0x14, 0x1B, 0xFF);

        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraFollow>().Configure(player, LevelBounds());
    }

    private static Rect LevelBounds()
    {
        return Rect.MinMaxRect(-3f, -3f, 53f, 19.2f);
    }

    private static void CreateSystems(Transform player)
    {
        var systems = new GameObject("Systems");
        systems.AddComponent<HintDisplay>();
        systems.AddComponent<CompanionCommandMenu>().Configure(player, null);
    }

    private static void PutSceneFirstInBuild()
    {
        var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
        var added = new HashSet<string> { ScenePath };
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (added.Add(scene.path)) scenes.Add(scene);
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + path);
        if (sprite == null) throw new InvalidOperationException("Sprite yuklenemedi: " + path);
        return sprite;
    }

    private static Sprite[] LoadFrames(Func<int, string> framePath)
    {
        var frames = new Sprite[4];
        for (int i = 0; i < frames.Length; i++) frames[i] = LoadSprite(framePath(i));
        return frames;
    }

    private static Vector2 SizeFor(string path, float width)
    {
        Sprite sprite = LoadSprite(path);
        return new Vector2(width, width * sprite.rect.height / sprite.rect.width);
    }

    private static GameObject CenteredSprite(string name, string path, Vector2 center, Vector2 size, int sortingOrder, Transform parent)
    {
        return BottomSprite(name, path, center + Vector2.down * (size.y * 0.5f), size, sortingOrder, parent);
    }

    private static GameObject BottomSprite(string name, string path, Vector2 bottomCenter, Vector2 size, int sortingOrder, Transform parent)
    {
        Sprite sprite = LoadSprite(path);
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Vector2 pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
        go.transform.localPosition = bottomCenter + new Vector2((pivot.x - 0.5f) * size.x, pivot.y * size.y);
        go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = spriteMaterial;
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    private static void AddYSort(GameObject go, bool isStatic)
    {
        go.GetComponent<SpriteRenderer>().sortingOrder = SortFor(go.transform.position.y);
        go.AddComponent<YSortRenderer>().Configure(SortBase, isStatic);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursively(child.gameObject, layer);
    }
}
