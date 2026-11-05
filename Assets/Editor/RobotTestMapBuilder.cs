using System.Collections.Generic;
using Karantina13.Testing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RobotTestMapBuilder
{
    public const string ScenePath = "Assets/Scenes/RobotTest.unity";

    private const int RoomWidth = 14;
    private const int RoomHeight = 9;
    private static readonly Vector2Int RobotStart = new Vector2Int(1, 4);

    private static readonly RectInt[] InnerWalls =
    {
        LevelBuildKit.Run(3, 0, 3, 5),
        LevelBuildKit.Run(7, 3, 7, 8),
        LevelBuildKit.Run(10, 0, 10, 5),
        LevelBuildKit.Run(4, 7, 5, 7)
    };

    private static readonly Vector2Int[] BarrelCells =
    {
        new Vector2Int(5, 3),
        new Vector2Int(12, 7),
        new Vector2Int(8, 1)
    };

    [MenuItem("Karantina 13/Robot Test Haritasi Olustur")]
    public static void BuildFromMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build();
    }

    public static void Build()
    {
        Scene scene = LevelBuildKit.BeginScene(UsedSpritePaths());
        HashSet<Vector2Int> blocked = BlockedCells();
        List<Vector2Int> open = BuildFloors(blocked);
        BuildWalls();
        BuildBarrels();

        GameObject robot = LevelBuildKit.SpawnRobot(RobotStart);
        GameObject crate = CreateTargetCrate(LevelBuildKit.CellCenter(open[open.Count - 1]));

        float right = RoomWidth * LevelBuildKit.CellSize + 1f;
        float top = RoomHeight * LevelBuildKit.CellSize + 1.2f;
        Camera camera = LevelBuildKit.CreateCamera(robot.transform, Rect.MinMaxRect(-3f, -3f, right, top));
        camera.orthographicSize = 10f;

        var spawnPoints = new List<Vector2>();
        foreach (Vector2Int cell in open) spawnPoints.Add(LevelBuildKit.CellCenter(cell));
        var runner = new GameObject("Robot Test").AddComponent<RobotTrialRunner>();
        runner.Configure(robot.transform, crate.transform, spawnPoints);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("RobotTest olusturuldu: " + spawnPoints.Count + " spawn noktasi");
    }

    private static IEnumerable<string> UsedSpritePaths()
    {
        string[] tiles = { "floor_clean_metal", "floor_worn_metal", "glow_light_tile", "wall_straight", "wall_side", "wall_bottom" };
        foreach (string tile in tiles) yield return LevelBuildKit.TilePath(tile);
        yield return LevelBuildKit.PropPath("barrel");
        yield return LevelBuildKit.PropPath("crate");
    }

    private static HashSet<Vector2Int> BlockedCells()
    {
        var blocked = new HashSet<Vector2Int>();
        foreach (RectInt run in InnerWalls)
        {
            foreach (Vector2Int cell in run.allPositionsWithin) blocked.Add(cell);
        }
        return blocked;
    }

    private static List<Vector2Int> BuildFloors(HashSet<Vector2Int> blocked)
    {
        var barrels = new HashSet<Vector2Int>(BarrelCells);
        var open = new List<Vector2Int>();
        for (int x = 0; x < RoomWidth; x++)
        {
            for (int y = 0; y < RoomHeight; y++)
            {
                var cell = new Vector2Int(x, y);
                if (blocked.Contains(cell)) continue;
                LevelBuildKit.PlaceFloor(cell, LevelBuildKit.PickRoomTile(cell, y == RoomHeight - 1));
                if (!barrels.Contains(cell) && cell != RobotStart) open.Add(cell);
            }
        }
        return open;
    }

    private static void BuildWalls()
    {
        var runs = new List<RectInt>
        {
            LevelBuildKit.Run(-1, -1, RoomWidth, -1),
            LevelBuildKit.Run(-1, RoomHeight, RoomWidth, RoomHeight),
            LevelBuildKit.Run(-1, 0, -1, RoomHeight - 1),
            LevelBuildKit.Run(RoomWidth, 0, RoomWidth, RoomHeight - 1)
        };
        runs.AddRange(InnerWalls);
        LevelBuildKit.BuildWalls(runs);
    }

    private static void BuildBarrels()
    {
        for (int i = 0; i < BarrelCells.Length; i++)
        {
            Vector2 feet = LevelBuildKit.CellBottom(BarrelCells[i]) + Vector2.up * 0.5f;
            LevelBuildKit.PlaceProp("Barrel " + (i + 1), "barrel", feet, 0.9f);
        }
    }

    private static GameObject CreateTargetCrate(Vector2 position)
    {
        var root = new GameObject("Hedef Kutu");
        root.transform.position = position;
        string path = LevelBuildKit.PropPath("crate");
        GameObject visual = LevelBuildKit.BottomSprite("Visual", path, new Vector2(0f, -0.65f), LevelBuildKit.SizeFor(path, 1.5f), 0, root.transform);
        LevelBuildKit.AddYSort(visual, false);
        return root;
    }
}
