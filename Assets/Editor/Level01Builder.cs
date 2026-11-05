using System.Collections.Generic;
using Karantina13.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level01Builder
{
    public const string ScenePath = "Assets/Scenes/Level01.unity";

    private const int RoomHeight = 5;
    private const int DoorRow = 2;
    private const int MiddleWall = 5;
    private const int EastWall = 13;
    private const int FenceColumn = 9;
    private const int CorridorEnd = 16;

    [MenuItem("Karantina 13/Bolum 1 Olustur")]
    public static void BuildFromMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build();
    }

    public static void Build()
    {
        Scene scene = LevelBuildKit.BeginScene(UsedSpritePaths());
        BuildFloors();
        BuildWalls();
        BuildPuzzles();
        BuildDecor();
        BuildTriggers();

        GameObject player = LevelBuildKit.SpawnPlayer(new Vector2Int(1, DoorRow));
        LevelBuildKit.SpawnRobot(new Vector2Int(1, 1));
        float right = (CorridorEnd + 1) * LevelBuildKit.CellSize;
        float top = RoomHeight * LevelBuildKit.CellSize + 2.2f;
        LevelBuildKit.CreateCamera(player.transform, Rect.MinMaxRect(-3f, -3f, right, top));
        LevelBuildKit.CreateSystems(player.transform);

        LevelBuildKit.FinishScene(scene, ScenePath, 0);
        Debug.Log("Level01 olusturuldu");
    }

    private static IEnumerable<string> UsedSpritePaths()
    {
        string[] tiles =
        {
            "floor_clean_metal", "floor_worn_metal", "floor_caution", "floor_electric_hazard",
            "glow_light_tile", "wall_straight", "wall_side", "wall_bottom"
        };
        string[] props = { "crate", "barrel", "lab_table", "computer_terminal", "warning_sign", "wall_control_panel" };
        foreach (string tile in tiles) yield return LevelBuildKit.TilePath(tile);
        foreach (string prop in props) yield return LevelBuildKit.PropPath(prop);
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.BarrierFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.DoorFrame)) yield return frame;
    }

    private static void BuildFloors()
    {
        LevelBuildKit.FillRoomFloor(0, MiddleWall - 1, 0, RoomHeight - 1);
        LevelBuildKit.FillRoomFloor(MiddleWall + 1, EastWall - 1, 0, RoomHeight - 1);
        foreach (Vector2Int cell in LevelBuildKit.DoorCells(new Vector2Int(MiddleWall, DoorRow), true))
        {
            LevelBuildKit.PlaceFloor(cell, "floor_worn_metal");
        }
        for (int x = EastWall; x < CorridorEnd; x++)
        {
            for (int y = DoorRow - 1; y <= DoorRow + 1; y++) LevelBuildKit.PlaceFloor(new Vector2Int(x, y), "floor_worn_metal");
        }
    }

    private static void BuildWalls()
    {
        int north = RoomHeight;
        var runs = new List<RectInt>
        {
            Run(-1, -1, EastWall, -1), Run(-1, north, EastWall, north), Run(-1, 0, -1, north - 1),
            Run(MiddleWall, 0, MiddleWall, DoorRow - 2), Run(MiddleWall, DoorRow + 2, MiddleWall, north - 1),
            Run(EastWall, 0, EastWall, DoorRow - 2), Run(EastWall, DoorRow + 2, EastWall, north - 1),
            Run(EastWall + 1, DoorRow - 2, CorridorEnd, DoorRow - 2), Run(EastWall + 1, DoorRow + 2, CorridorEnd, DoorRow + 2),
            Run(CorridorEnd, DoorRow - 1, CorridorEnd, DoorRow + 1)
        };
        LevelBuildKit.BuildWalls(runs);
    }

    private static RectInt Run(int fromX, int fromY, int toX, int toY) => LevelBuildKit.Run(fromX, fromY, toX, toY);

    private static void BuildPuzzles()
    {
        SlidingDoor doorA = LevelBuildKit.CreateSlidingDoor("Door A", new Vector2Int(MiddleWall, DoorRow), true);
        SlidingDoor doorB = LevelBuildKit.CreateSlidingDoor("Door B", new Vector2Int(EastWall, DoorRow), true);
        ElectricFence fence = LevelBuildKit.CreateElectricFence(FenceColumn, 0, RoomHeight - 1);

        LevelBuildKit.CreatePushableBox("Crate A", new Vector2Int(2, 3));
        LevelBuildKit.CreatePushableBox("Crate B", new Vector2Int(11, 3));

        LevelBuildKit.CreatePressurePlate("Plate A", new Vector2Int(4, 3), new MonoBehaviour[] { doorA });
        LevelBuildKit.CreatePressurePlate("Plate B", new Vector2Int(11, 1), new MonoBehaviour[] { fence, doorB });
    }

    private static void BuildDecor()
    {
        float wallBaseY = RoomHeight * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        LevelBuildKit.PlaceProp("Lab Table", "lab_table", new Vector2(1.2f, 7.1f), 2.5f);
        LevelBuildKit.PlaceProp("Barrel A1", "barrel", new Vector2(6.6f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel A2", "barrel", new Vector2(7.6f, -0.6f), 0.8f);
        LevelBuildKit.PlaceWallDecor("Control Panel A", "wall_control_panel", new Vector2(6f, wallBaseY + 0.3f), 0.7f, wallBaseY);

        LevelBuildKit.PlaceProp("Computer Terminal", "computer_terminal", new Vector2(12.9f, 7.1f), 1.5f);
        LevelBuildKit.PlaceProp("Warning Fence", "warning_sign", new Vector2(16.4f, -0.6f), 0.7f);
        LevelBuildKit.PlaceWallDecor("Control Panel B", "wall_control_panel", new Vector2(22f, wallBaseY + 0.3f), 0.7f, wallBaseY);
        LevelBuildKit.PlaceProp("Barrel B1", "barrel", new Vector2(24.3f, -0.8f), 0.8f);
    }

    private static void BuildTriggers()
    {
        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(1, DoorRow)), new Vector2(4f, 4f),
            "Kapıyı aç: Robota kutuyu butona ittir (Q)", "Q tuşuyla robota komut verebilirsin.");
        LevelBuildKit.CreateHintZone("Hint Electric Room", LevelBuildKit.CellCenter(new Vector2Int(MiddleWall + 1, DoorRow)), new Vector2(2f, 6f),
            "Elektriği kes: Robot tellerden geçebilir", "Tellere dokunamam ama robot yalıtımlı, o geçebilir.");
        LevelBuildKit.CreateLevelExit(new Vector2Int(CorridorEnd - 1, DoorRow), "Level02", "Bölüm 1 tamamlandı!",
            new Vector2(LevelBuildKit.CellSize, LevelBuildKit.CellSize * LevelBuildKit.DoorSpan));
    }
}
