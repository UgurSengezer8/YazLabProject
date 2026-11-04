using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level02Builder
{
    public const string ScenePath = "Assets/Scenes/Level02.unity";

    private const int RoomWidth = 11;
    private const int RoomHeight = 5;
    private const int GlassRow = 2;
    private const int BoxRow = 1;
    private const int DoorRow = 3;
    private const int CorridorLength = 3;
    private const float EmitterFeet = -1.15f;
    private const float RayOrigin = -0.3f;
    private static readonly int[] LaserColumns = { 2, 4, 6, 8 };

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

        GameObject player = LevelBuildKit.SpawnPlayer(new Vector2Int(0, DoorRow));
        LevelBuildKit.SpawnRobot(new Vector2Int(0, BoxRow));
        float right = (RoomWidth + CorridorLength) * LevelBuildKit.CellSize + 1f;
        float top = RoomHeight * LevelBuildKit.CellSize + 2.2f;
        LevelBuildKit.CreateCamera(player.transform, Rect.MinMaxRect(-3f, -3f, right, top));
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
            "floor_clean_metal", "floor_worn_metal", "glow_light_tile",
            "wall_straight", "wall_side", "wall_bottom", "glass_wall", "glass_wall_slot"
        };
        string[] props = { "barrel", "wall_control_panel", "laser_emitter", "laser_receiver" };
        foreach (string tile in tiles) yield return LevelBuildKit.TilePath(tile);
        foreach (string prop in props) yield return LevelBuildKit.PropPath(prop);
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.DoorFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.RayFrame)) yield return frame;
        yield return LevelBuildKit.RayImpact;
    }

    private static void BuildFloors()
    {
        LevelBuildKit.FillRoomFloor(0, RoomWidth - 1, 0, RoomHeight - 1);
        foreach (Vector2Int cell in LevelBuildKit.DoorCells(new Vector2Int(-1, DoorRow), true))
        {
            LevelBuildKit.PlaceFloor(cell, "floor_worn_metal");
        }
        for (int x = RoomWidth; x < RoomWidth + CorridorLength; x++)
        {
            LevelBuildKit.PlaceFloor(new Vector2Int(x, DoorRow), "floor_worn_metal");
        }
    }

    private static void BuildWalls()
    {
        int east = RoomWidth;
        int north = RoomHeight;
        int corridorEnd = RoomWidth + CorridorLength;
        var runs = new List<RectInt>
        {
            LevelBuildKit.Run(-1, -1, east, -1),
            LevelBuildKit.Run(-1, north, east, north),
            LevelBuildKit.Run(-1, 0, -1, DoorRow - 2),
            LevelBuildKit.Run(east, 0, east, DoorRow - 1),
            LevelBuildKit.Run(east, DoorRow + 1, east, north - 1),
            LevelBuildKit.Run(east + 1, DoorRow - 1, corridorEnd, DoorRow - 1),
            LevelBuildKit.Run(east + 1, DoorRow + 1, corridorEnd, DoorRow + 1),
            LevelBuildKit.Run(corridorEnd, DoorRow, corridorEnd, DoorRow)
        };
        LevelBuildKit.BuildWalls(runs);
        LevelBuildKit.CreateGlassWall(0, RoomWidth - 1, GlassRow, LaserColumns);
    }

    private static void BuildPuzzles()
    {
        LevelBuildKit.CreateSlidingDoor("Entrance Door", new Vector2Int(-1, DoorRow), true);

        float wallFace = RoomHeight * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float length = wallFace - RayOrigin;
        float goal = BoxRow * LevelBuildKit.CellSize - RayOrigin;
        for (int i = 0; i < LaserColumns.Length; i++)
        {
            var origin = new Vector2(LaserColumns[i] * LevelBuildKit.CellSize, RayOrigin);
            LevelBuildKit.CreateLaserRay("Laser " + (i + 1), origin, Vector2.up, length, goal);
        }

        LevelBuildKit.CreatePushableBox("Crate", new Vector2Int(1, BoxRow));
    }

    private static void BuildDecor()
    {
        float wallBaseY = RoomHeight * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        for (int i = 0; i < LaserColumns.Length; i++)
        {
            float x = LaserColumns[i] * LevelBuildKit.CellSize;
            LevelBuildKit.PlaceDecal("Laser Emitter " + (i + 1), "laser_emitter", new Vector2(x, EmitterFeet), 1.2f);
            LevelBuildKit.PlaceWallDecor("Laser Receiver " + (i + 1), "laser_receiver", new Vector2(x, wallBaseY + 0.35f), 0.62f, wallBaseY);
        }

        LevelBuildKit.PlaceWallDecor("Control Panel 1", "wall_control_panel", new Vector2(1f, wallBaseY + 0.3f), 0.7f, wallBaseY);
        LevelBuildKit.PlaceWallDecor("Control Panel 2", "wall_control_panel", new Vector2(19f, wallBaseY + 0.3f), 0.7f, wallBaseY);
        LevelBuildKit.PlaceProp("Barrel 1", "barrel", new Vector2(19.6f, -0.8f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel 2", "barrel", new Vector2(20.5f, -0.6f), 0.8f);
    }

    private static void BuildTriggers()
    {
        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(0, DoorRow)), new Vector2(4f, 4f),
            "Lazerleri aş: Robota kutuyu ışının önüne ittir (Q)",
            "Işınlar camdan geçiyor. Robot kutuyu lazerin önüne koyarsa geçebilirim.");
        LevelBuildKit.CreateLevelExit(new Vector2Int(RoomWidth + CorridorLength - 1, DoorRow), "Level03", "Bölüm 2 tamamlandı!");
    }
}
