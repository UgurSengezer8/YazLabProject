using System.Collections.Generic;
using Karantina13.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level01Builder
{
    public const string ScenePath = "Assets/Scenes/Level01.unity";

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

        GameObject player = LevelBuildKit.SpawnPlayer(new Vector2Int(2, 3));
        LevelBuildKit.SpawnRobot(new Vector2Int(3, 2));
        LevelBuildKit.CreateCamera(player.transform, Rect.MinMaxRect(-3f, -3f, 53f, 19.2f));
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
        LevelBuildKit.FillRoomFloor(0, 7, 0, 6);
        LevelBuildKit.FillRoomFloor(12, 21, 0, 8);
        for (int x = 8; x <= 11; x++) LevelBuildKit.PlaceFloor(new Vector2Int(x, 4), "floor_worn_metal");
        for (int x = 22; x <= 25; x++) LevelBuildKit.PlaceFloor(new Vector2Int(x, 3), "floor_worn_metal");
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
        LevelBuildKit.BuildWalls(runs);
    }

    private static RectInt Run(int fromX, int fromY, int toX, int toY) => LevelBuildKit.Run(fromX, fromY, toX, toY);

    private static void BuildPuzzles()
    {
        SlidingDoor doorA = LevelBuildKit.CreateSlidingDoor("Door A", new Vector2Int(8, 4));
        SlidingDoor doorB = LevelBuildKit.CreateSlidingDoor("Door B", new Vector2Int(22, 3));
        ElectricFence fence = LevelBuildKit.CreateElectricFence(17, 0, 8);

        LevelBuildKit.CreatePushableBox("Crate A", new Vector2Int(4, 4));
        LevelBuildKit.CreatePushableBox("Crate B", new Vector2Int(19, 6));

        LevelBuildKit.CreatePressurePlate("Plate A", new Vector2Int(6, 4), new MonoBehaviour[] { doorA });
        LevelBuildKit.CreatePressurePlate("Plate B", new Vector2Int(19, 3), new MonoBehaviour[] { fence, doorB });
    }

    private static void BuildDecor()
    {
        LevelBuildKit.PlaceProp("Lab Table", "lab_table", new Vector2(2.6f, 11.1f), 2.5f);
        LevelBuildKit.PlaceProp("Computer Terminal", "computer_terminal", new Vector2(9.6f, 11.2f), 1.5f);
        LevelBuildKit.PlaceProp("Barrel A1", "barrel", new Vector2(0.3f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel A2", "barrel", new Vector2(1.4f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Warning Plate A", "warning_sign", new Vector2(13.6f, 10.4f), 0.7f);
        LevelBuildKit.PlaceWallDecor("Control Panel", "wall_control_panel", new Vector2(6f, 13.5f), 0.7f, 13f);

        LevelBuildKit.PlaceProp("Warning Fence North", "warning_sign", new Vector2(31.6f, 12.4f), 0.7f);
        LevelBuildKit.PlaceProp("Warning Fence South", "warning_sign", new Vector2(36.4f, 0.2f), 0.7f);
        LevelBuildKit.PlaceProp("Barrel B1", "barrel", new Vector2(24.4f, 15.2f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel B2", "barrel", new Vector2(25.6f, 15.2f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel B3", "barrel", new Vector2(42.4f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Crate Stack 1", "crate", new Vector2(24.6f, -0.8f), 1.5f);
        LevelBuildKit.PlaceProp("Crate Stack 2", "crate", new Vector2(26.4f, -0.8f), 1.5f);
        LevelBuildKit.PlaceProp("Crate Stack 3", "crate", new Vector2(42.2f, 15.2f), 1.5f);
        LevelBuildKit.PlaceProp("Computer Terminal B", "computer_terminal", new Vector2(28f, 15.2f), 1.5f);
    }

    private static void BuildTriggers()
    {
        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(2, 3)), new Vector2(6f, 6f),
            "Kapıyı aç: Robota kutuyu butona ittir (Q)", "Q tuşuyla robota komut verebilirsin.");
        LevelBuildKit.CreateHintZone("Hint Electric Room", LevelBuildKit.CellCenter(new Vector2Int(12, 4)), new Vector2(2f, 6f),
            "Elektriği kes: Robot tellerden geçebilir", "Tellere dokunamam ama robot yalıtımlı, o geçebilir.");
        LevelBuildKit.CreateLevelExit(new Vector2Int(25, 3), "Level02", "Bölüm 1 tamamlandı!");
    }
}
