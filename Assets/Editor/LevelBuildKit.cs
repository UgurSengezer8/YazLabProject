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

public static class LevelBuildKit
{
    public const string ArtRoot = "Assets/Karantina13_Bolum01_Tam_Paket_v2/Assets/Karantina13/";
    public const float CellSize = 2f;
    public const int SortBase = 100;
    public const float FramePadding = 0.2f;

    private const string MaterialPath = "Assets/Karantina13Test/KarantinaSprites.mat";
    private const string PlayerPrefabPath = "Assets/Karantina13Test/Prefabs/Player.prefab";
    private const string RobotPrefabPath = "Assets/Karantina13Test/Prefabs/Robot.prefab";
    private const float StripThickness = 0.5f;
    private const int StripOrder = -800;

    public static readonly Vector2 TileSize = new Vector2(2.02f, 2.02f);
    public static readonly Vector2 WallSize = new Vector2(2.02f, 2.1f);
    public static readonly Vector2 FrameSize = new Vector2(2.4f, 2.4f);

    private static readonly string[] RequiredLayers =
    {
        GameLayers.Player, GameLayers.Companion, GameLayers.Pushable, GameLayers.Hazard, GameLayers.Environment, GameLayers.Laser
    };

    private static readonly HashSet<Vector2Int> wallCells = new HashSet<Vector2Int>();
    private static readonly HashSet<Vector2Int> floorCells = new HashSet<Vector2Int>();
    private static readonly HashSet<string> placedStrips = new HashSet<string>();
    private static Material spriteMaterial;
    private static Transform floorRoot;
    private static Transform geometryRoot;
    private static Transform decorRoot;
    private static Transform puzzleRoot;
    private static Transform triggerRoot;

    public static Scene BeginScene(IEnumerable<string> spritePaths)
    {
        EnsureLayers();
        ConfigurePhysics();
        foreach (string path in spritePaths) EnsureSpriteImport(ArtRoot + path);
        spriteMaterial = LoadSpriteMaterial();
        wallCells.Clear();
        floorCells.Clear();
        placedStrips.Clear();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateLevelHierarchy();
        return scene;
    }

    public static void FinishScene(Scene scene, string scenePath, int buildIndex)
    {
        EditorSceneManager.SaveScene(scene, scenePath);
        PutSceneInBuild(scenePath, buildIndex);
        AssetDatabase.SaveAssets();
    }

    public static void PutSceneInBuild(string scenePath, int index)
    {
        var scenes = new List<EditorBuildSettingsScene>();
        var added = new HashSet<string> { scenePath };
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (added.Add(scene.path)) scenes.Add(scene);
        }
        scenes.Insert(Mathf.Clamp(index, 0, scenes.Count), new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    public static string TilePath(string name) => "Tiles/" + name + ".png";
    public static string PropPath(string name) => "Props/" + name + ".png";
    public static string BarrierFrame(int index) => "Animations/ElectricBarrier/electric_barrier_0" + index + ".png";
    public static string DoorFrame(int index) => "Animations/SlidingDoor/sliding_door_0" + index + ".png";
    public static string LaserFrame(int index) => "Animations/LaserBeam/laser_beam_0" + index + ".png";

    public static IEnumerable<string> FramePaths(Func<int, string> framePath)
    {
        for (int i = 0; i < 4; i++) yield return framePath(i);
    }

    public static int LayerIndex(string layerName)
    {
        int index = LayerMask.NameToLayer(layerName);
        if (index < 0) throw new InvalidOperationException("Layer bulunamadi: " + layerName);
        return index;
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

    private static void ConfigurePhysics()
    {
        Physics2D.gravity = Vector2.zero;
        GameLayers.ApplyCollisionRules();
        int companion = LayerIndex(GameLayers.Companion);
        int laser = LayerIndex(GameLayers.Laser);
        Physics2D.IgnoreLayerCollision(companion, LayerIndex(GameLayers.Hazard), true);
        Physics2D.IgnoreLayerCollision(companion, laser, true);
        Physics2D.IgnoreLayerCollision(LayerIndex(GameLayers.Pushable), laser, true);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureSpriteImport(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        }
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

    public static Vector2 CellCenter(Vector2Int cell) => new Vector2(cell.x * CellSize, cell.y * CellSize);
    public static Vector2 CellBottom(Vector2Int cell) => CellCenter(cell) + Vector2.down * (CellSize * 0.5f);
    public static int SortFor(float y) => SortBase - Mathf.RoundToInt(y * 20f);

    public static void FillRoomFloor(int minX, int maxX, int minY, int maxY)
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

    public static string PickRoomTile(Vector2Int cell, bool northRow)
    {
        if (northRow && cell.x % 3 == 1) return "glow_light_tile";
        int hash = Mathf.Abs((cell.x * 73856093) ^ (cell.y * 19349663));
        return hash % 5 == 0 ? "floor_worn_metal" : "floor_clean_metal";
    }

    public static void PlaceFloor(Vector2Int cell, string tile)
    {
        floorCells.Add(cell);
        CenteredSprite("Floor " + cell.x + "," + cell.y, TilePath(tile), CellCenter(cell), TileSize, -1000, floorRoot);
    }

    public static RectInt Run(int fromX, int fromY, int toX, int toY)
    {
        return new RectInt(fromX, fromY, toX - fromX + 1, toY - fromY + 1);
    }

    public static void BuildWalls(List<RectInt> runs)
    {
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

    public static SlidingDoor CreateSlidingDoor(string name, Vector2Int cell)
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

    public static ElectricFence CreateElectricFence(int column, int fromRow, int toRow)
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

    public static LaserBeam CreateLaserBeam(string name, int column, int topRow, int bottomRow)
    {
        var root = new GameObject(name);
        root.transform.SetParent(puzzleRoot, false);
        Vector2 origin = CellCenter(new Vector2Int(column, topRow)) + Vector2.up * (CellSize * 0.5f);
        root.transform.localPosition = origin;

        float length = (topRow - bottomRow + 1) * CellSize;
        var blockerObject = new GameObject("Blocker");
        blockerObject.transform.SetParent(root.transform, false);
        var blocker = blockerObject.AddComponent<BoxCollider2D>();
        blocker.size = new Vector2(CellSize * 0.8f, length);
        blocker.offset = Vector2.down * (length * 0.5f);

        Sprite[] frames = LoadFrames(LaserFrame);
        var segments = new List<SpriteFrameAnimator>();
        for (int row = topRow; row >= bottomRow; row--)
        {
            Vector2 center = CellCenter(new Vector2Int(column, row)) - origin;
            Vector2 anchor = center + Vector2.down * (CellSize * 0.5f + FramePadding);
            GameObject segment = BottomSprite("Segment " + (topRow - row), LaserFrame(0), anchor, FrameSize, -950, root.transform);
            var animator = segment.AddComponent<SpriteFrameAnimator>();
            animator.Configure(frames, 10f, true);
            segments.Add(animator);
        }

        var beam = root.AddComponent<LaserBeam>();
        beam.Configure(blocker, segments.ToArray(), CellSize, length, Vector2.right);
        SetLayerRecursively(root, LayerIndex(GameLayers.Laser));
        return beam;
    }

    public static GameObject CreatePushableBox(string name, Vector2Int cell)
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
        return root;
    }

    public static PressurePlate CreatePressurePlate(string name, Vector2Int cell, MonoBehaviour[] targets)
    {
        var root = new GameObject(name);
        root.transform.SetParent(puzzleRoot, false);
        root.transform.localPosition = CellCenter(cell);
        GameObject visual = CenteredSprite("Visual", TilePath("floor_caution"), Vector2.zero, new Vector2(1.8f, 1.8f), -900, root.transform);
        var plate = root.AddComponent<PressurePlate>();
        plate.Configure(targets, visual.GetComponent<SpriteRenderer>());
        return plate;
    }

    public static GameObject PlaceProp(string name, string sprite, Vector2 feet, float width)
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
        return root;
    }

    public static GameObject PlaceWallDecor(string name, string sprite, Vector2 position, float width, float wallBaseY)
    {
        Vector2 size = SizeFor(PropPath(sprite), width);
        GameObject decor = BottomSprite(name, PropPath(sprite), position, size, SortFor(wallBaseY) + 1, decorRoot);
        decor.layer = LayerIndex(GameLayers.Environment);
        return decor;
    }

    public static HintZone CreateHintZone(string name, Vector2 center, Vector2 size, string objective, string message)
    {
        GameObject zone = CreateTrigger(name, center, size);
        var hintZone = zone.AddComponent<HintZone>();
        hintZone.Configure(objective, message);
        return hintZone;
    }

    public static LevelExit CreateLevelExit(Vector2Int cell, string nextSceneName, string completeMessage)
    {
        GameObject exit = CreateTrigger("Level Exit", CellCenter(cell), new Vector2(CellSize, CellSize));
        var levelExit = exit.AddComponent<LevelExit>();
        levelExit.Configure(nextSceneName, completeMessage);
        return levelExit;
    }

    public static GameObject CreateTrigger(string name, Vector2 center, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(triggerRoot, false);
        go.transform.localPosition = center;
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.isTrigger = true;
        return go;
    }

    public static GameObject SpawnPlayer(Vector2Int cell)
    {
        return SpawnCharacter(PlayerPrefabPath, "Player", cell, GameLayers.Player);
    }

    public static GameObject SpawnRobot(Vector2Int cell)
    {
        GameObject robot = SpawnCharacter(RobotPrefabPath, "Robot", cell, GameLayers.Companion);
        if (robot.GetComponent<BoxPusher>() == null) robot.AddComponent<BoxPusher>();
        var controller = robot.GetComponent<KarantinaCharacterController>();
        if (controller != null) controller.controls = KarantinaCharacterController.ControlScheme.ArrowKeys;
        return robot;
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

    public static Camera CreateCamera(Transform player, Rect worldBounds)
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
        cameraObject.AddComponent<CameraFollow>().Configure(player, worldBounds);
        return camera;
    }

    public static GameObject CreateSystems(Transform player)
    {
        var systems = new GameObject("Systems");
        systems.AddComponent<HintDisplay>();
        systems.AddComponent<CompanionCommandMenu>().Configure(player, null);
        return systems;
    }

    public static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + path);
        if (sprite == null) throw new InvalidOperationException("Sprite yuklenemedi: " + path);
        return sprite;
    }

    public static Sprite[] LoadFrames(Func<int, string> framePath)
    {
        var frames = new Sprite[4];
        for (int i = 0; i < frames.Length; i++) frames[i] = LoadSprite(framePath(i));
        return frames;
    }

    public static Vector2 SizeFor(string path, float width)
    {
        Sprite sprite = LoadSprite(path);
        return new Vector2(width, width * sprite.rect.height / sprite.rect.width);
    }

    public static GameObject CenteredSprite(string name, string path, Vector2 center, Vector2 size, int sortingOrder, Transform parent)
    {
        return BottomSprite(name, path, center + Vector2.down * (size.y * 0.5f), size, sortingOrder, parent);
    }

    public static GameObject BottomSprite(string name, string path, Vector2 bottomCenter, Vector2 size, int sortingOrder, Transform parent)
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

    public static void AddYSort(GameObject go, bool isStatic)
    {
        go.GetComponent<SpriteRenderer>().sortingOrder = SortFor(go.transform.position.y);
        go.AddComponent<YSortRenderer>().Configure(SortBase, isStatic);
    }

    public static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursively(child.gameObject, layer);
    }
}
