using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level02Builder
{
    public const string ScenePath = "Assets/Scenes/Level02.unity";

    private const int RoomWidth = 16;
    private const int RoomHeight = 8;
    private const int ExitRow = 3;
    private static readonly int[] LaserColumns = { 5, 9, 13 };
    private static readonly Color EmitterColor = new Color(1f, 0.45f, 0.45f);

    [MenuItem("Karantina 13/Bolum 2 Olustur")]
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

        GameObject player = LevelBuildKit.SpawnPlayer(new Vector2Int(1, 3));
        LevelBuildKit.SpawnRobot(new Vector2Int(2, 2));
        LevelBuildKit.CreateCamera(player.transform, Rect.MinMaxRect(-3f, -3f, 41f, 17.2f));
        LevelBuildKit.CreateSystems(player.transform);

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Level01Builder.ScenePath) != null)
        {
            LevelBuildKit.PutSceneInBuild(Level01Builder.ScenePath, 0);
        }
        LevelBuildKit.FinishScene(scene, ScenePath, 1);
        Debug.Log("Level02 olusturuldu");
    }

    private static IEnumerable<string> UsedSpritePaths()
    {
        string[] tiles =
        {
            "floor_clean_metal", "floor_worn_metal", "floor_electric_hazard", "glow_light_tile",
            "wall_straight", "wall_side", "wall_bottom"
        };
        string[] props = { "crate", "barrel", "computer_terminal", "warning_sign", "wall_control_panel" };
        foreach (string tile in tiles) yield return LevelBuildKit.TilePath(tile);
        foreach (string prop in props) yield return LevelBuildKit.PropPath(prop);
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.DoorFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.LaserFrame)) yield return frame;
    }

    private static bool IsLaserColumn(int x)
    {
        foreach (int column in LaserColumns)
        {
            if (column == x) return true;
        }
        return false;
    }

    private static void BuildFloors()
    {
        int northRow = RoomHeight - 1;
        for (int x = 0; x < RoomWidth; x++)
        {
            for (int y = 0; y < RoomHeight; y++)
            {
                var cell = new Vector2Int(x, y);
                string tile = IsLaserColumn(x) ? "floor_electric_hazard" : LevelBuildKit.PickRoomTile(cell, y == northRow);
                LevelBuildKit.PlaceFloor(cell, tile);
            }
        }
        LevelBuildKit.PlaceFloor(new Vector2Int(-1, ExitRow), "floor_worn_metal");
        for (int x = RoomWidth; x <= RoomWidth + 3; x++) LevelBuildKit.PlaceFloor(new Vector2Int(x, ExitRow), "floor_worn_metal");
    }

    private static void BuildWalls()
    {
        int east = RoomWidth;
        int north = RoomHeight;
        int corridorEnd = RoomWidth + 4;
        var runs = new List<RectInt>
        {
            LevelBuildKit.Run(-1, -1, east, -1),
            LevelBuildKit.Run(-1, north, east, north),
            LevelBuildKit.Run(-1, 0, -1, ExitRow - 1),
            LevelBuildKit.Run(-1, ExitRow + 1, -1, north - 1),
            LevelBuildKit.Run(east, 0, east, ExitRow - 1),
            LevelBuildKit.Run(east, ExitRow + 1, east, north - 1),
            LevelBuildKit.Run(east + 1, ExitRow - 1, corridorEnd, ExitRow - 1),
            LevelBuildKit.Run(east + 1, ExitRow + 1, corridorEnd, ExitRow + 1),
            LevelBuildKit.Run(corridorEnd, ExitRow, corridorEnd, ExitRow)
        };
        LevelBuildKit.BuildWalls(runs);
    }

    private static void BuildPuzzles()
    {
        LevelBuildKit.CreateSlidingDoor("Entrance Door", new Vector2Int(-1, ExitRow));

        int topRow = RoomHeight - 1;
        for (int i = 0; i < LaserColumns.Length; i++)
        {
            LevelBuildKit.CreateLaserBeam("Laser " + (i + 1), LaserColumns[i], topRow, 0);
        }

        LevelBuildKit.CreatePushableBox("Crate", new Vector2Int(2, topRow));
    }

    private static void BuildDecor()
    {
        float wallBaseY = RoomHeight * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        for (int i = 0; i < LaserColumns.Length; i++)
        {
            var position = new Vector2(LaserColumns[i] * LevelBuildKit.CellSize, wallBaseY + 0.5f);
            GameObject emitter = LevelBuildKit.PlaceWallDecor("Laser Emitter " + (i + 1), "wall_control_panel", position, 0.7f, wallBaseY);
            emitter.GetComponent<SpriteRenderer>().color = EmitterColor;
        }

        LevelBuildKit.PlaceProp("Barrel 1", "barrel", new Vector2(0.3f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel 2", "barrel", new Vector2(1.4f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Computer Terminal", "computer_terminal", new Vector2(6.4f, -0.8f), 1.5f);
        LevelBuildKit.PlaceProp("Warning Laser 1", "warning_sign", new Vector2(11.8f, 0.2f), 0.7f);
        LevelBuildKit.PlaceProp("Crate Stack 1", "crate", new Vector2(14.2f, -0.8f), 1.5f);
        LevelBuildKit.PlaceProp("Warning Laser 2", "warning_sign", new Vector2(19.8f, 0.2f), 0.7f);
        LevelBuildKit.PlaceProp("Crate Stack 2", "crate", new Vector2(22.2f, -0.8f), 1.5f);
        LevelBuildKit.PlaceProp("Warning Laser 3", "warning_sign", new Vector2(27.8f, 0.2f), 0.7f);
        LevelBuildKit.PlaceProp("Barrel 3", "barrel", new Vector2(29.4f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel 4", "barrel", new Vector2(30.4f, -0.8f), 0.8f);
    }

    private static void BuildTriggers()
    {
        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(1, 3)), new Vector2(6f, 6f),
            "Lazerleri aş: Robota kutuyu ışının önüne ittir (Q)", "Kutu ışını keserse altından geçebilirim.");
        LevelBuildKit.CreateLevelExit(new Vector2Int(RoomWidth + 3, ExitRow), "Level03", "Bölüm 2 tamamlandı!");
    }
}
