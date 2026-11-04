using System.Collections.Generic;
using Karantina13.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level03Builder
{
    public const string ScenePath = "Assets/Scenes/Level03.unity";

    private const int MiddleWall = 6;
    private const int CenterWall = 4;
    private const int EastWall = 13;
    private const int NorthWall = 8;
    private const int ExitRow = 6;
    private const int CorridorEnd = 16;
    private const int LaserColumn = 10;
    private const int BoxRow = 1;
    private const float EmitterFeet = -1.15f;
    private const float RayOrigin = -0.3f;

    private static readonly Vector2Int DoorA = new Vector2Int(MiddleWall, BoxRow);
    private static readonly Vector2Int DoorB = new Vector2Int(LaserColumn, CenterWall);
    private static readonly Vector2Int ExitDoor = new Vector2Int(EastWall, ExitRow);
    private static readonly Vector2Int FenceCell = new Vector2Int(2, CenterWall);

    [MenuItem("Karantina 13/Bolum 3 Olustur")]
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
        LevelBuildKit.SpawnRobot(new Vector2Int(0, BoxRow));
        // Harita 2x2 oda oldugu icin kamera tum yuksekligi gorecek kadar uzak durur (ust odalar hep gorunur).
        float right = (CorridorEnd + 1) * LevelBuildKit.CellSize;
        float top = NorthWall * LevelBuildKit.CellSize + 1.6f;
        const float bottom = -2.2f;
        Camera camera = LevelBuildKit.CreateCamera(player.transform, Rect.MinMaxRect(-3f, bottom, right, top));
        camera.orthographicSize = (top - bottom) * 0.5f;
        LevelBuildKit.CreateSystems(player.transform);

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Level01Builder.ScenePath) != null)
        {
            LevelBuildKit.PutSceneInBuild(Level01Builder.ScenePath, 0);
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Level02Builder.ScenePath) != null)
        {
            LevelBuildKit.PutSceneInBuild(Level02Builder.ScenePath, 1);
        }
        LevelBuildKit.FinishScene(scene, ScenePath, 2);
        Debug.Log("Level03 olusturuldu");
    }

    private static IEnumerable<string> UsedSpritePaths()
    {
        string[] tiles =
        {
            "floor_clean_metal", "floor_worn_metal", "floor_caution", "floor_electric_hazard",
            "glow_light_tile", "wall_straight", "wall_side", "wall_bottom"
        };
        string[] props =
        {
            "crate", "barrel", "lab_table", "computer_terminal", "warning_sign",
            "wall_control_panel", "laser_emitter"
        };
        foreach (string tile in tiles) yield return LevelBuildKit.TilePath(tile);
        foreach (string prop in props) yield return LevelBuildKit.PropPath(prop);
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.BarrierFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.DoorFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.RayFrame)) yield return frame;
        yield return LevelBuildKit.RayImpact;
    }

    // A: baslangic (sol alt), B: lazer odasi (sag alt), C: kutu kafesi (sol ust), D: cikis odasi (sag ust)
    private static void BuildFloors()
    {
        LevelBuildKit.FillRoomFloor(0, MiddleWall - 1, 0, CenterWall - 1);
        LevelBuildKit.FillRoomFloor(MiddleWall + 1, EastWall - 1, 0, CenterWall - 1);
        LevelBuildKit.FillRoomFloor(0, MiddleWall - 1, CenterWall + 1, NorthWall - 1);
        LevelBuildKit.FillRoomFloor(MiddleWall + 1, EastWall - 1, CenterWall + 1, NorthWall - 1);

        foreach (Vector2Int cell in new[] { DoorA, DoorB, ExitDoor, FenceCell })
        {
            LevelBuildKit.PlaceFloor(cell, "floor_worn_metal");
        }
        for (int x = EastWall + 1; x < CorridorEnd; x++)
        {
            LevelBuildKit.PlaceFloor(new Vector2Int(x, ExitRow), "floor_worn_metal");
        }
    }

    private static void BuildWalls()
    {
        var runs = new List<RectInt>
        {
            Run(-1, -1, EastWall, -1),
            Run(-1, NorthWall, EastWall, NorthWall),
            Run(-1, 0, -1, NorthWall - 1),

            Run(MiddleWall, 0, MiddleWall, DoorA.y - 1),
            Run(MiddleWall, DoorA.y + 1, MiddleWall, NorthWall - 1),

            Run(0, CenterWall, FenceCell.x - 1, CenterWall),
            Run(FenceCell.x + 1, CenterWall, MiddleWall - 1, CenterWall),
            Run(MiddleWall + 1, CenterWall, DoorB.x - 1, CenterWall),
            Run(DoorB.x + 1, CenterWall, EastWall - 1, CenterWall),

            Run(EastWall, 0, EastWall, ExitRow - 1),
            Run(EastWall, ExitRow + 1, EastWall, NorthWall - 1),

            Run(EastWall + 1, ExitRow - 1, CorridorEnd, ExitRow - 1),
            Run(EastWall + 1, ExitRow + 1, CorridorEnd, ExitRow + 1),
            Run(CorridorEnd, ExitRow, CorridorEnd, ExitRow)
        };
        LevelBuildKit.BuildWalls(runs);
    }

    private static RectInt Run(int fromX, int fromY, int toX, int toY) => LevelBuildKit.Run(fromX, fromY, toX, toY);

    private static void BuildPuzzles()
    {
        SlidingDoor doorA = LevelBuildKit.CreateSlidingDoor("Door D1", DoorA);
        SlidingDoor doorB = LevelBuildKit.CreateSlidingDoor("Door D2", DoorB);
        SlidingDoor exitDoor = LevelBuildKit.CreateSlidingDoor("Exit Door", ExitDoor);

        // Tel hicbir butona bagli degil: hep acik kalir. Robot gecer, oyuncu ve kutular gecemez.
        LevelBuildKit.CreateElectricFence(FenceCell.x, FenceCell.y, FenceCell.y);

        // Lazer D2 kapisinin onunu keser. Kutu noktasi BoxRow uzerinde.
        float wallFace = CenterWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float length = wallFace - RayOrigin;
        float goal = BoxRow * LevelBuildKit.CellSize - RayOrigin;
        var origin = new Vector2(LaserColumn * LevelBuildKit.CellSize, RayOrigin);
        LevelBuildKit.CreateLaserRay("Laser L", origin, Vector2.up, length, goal);

        LevelBuildKit.CreatePushableBox("Crate X", new Vector2Int(1, BoxRow));
        LevelBuildKit.CreatePushableBox("Crate Y", new Vector2Int(1, ExitRow));

        LevelBuildKit.CreatePressurePlate("Plate P1", new Vector2Int(3, BoxRow), new MonoBehaviour[] { doorA });
        LevelBuildKit.CreatePressurePlate("Plate P2", new Vector2Int(MiddleWall - 1, ExitRow), new MonoBehaviour[] { doorB });
        LevelBuildKit.CreatePressurePlate("Plate P3", new Vector2Int(LaserColumn, ExitRow), new MonoBehaviour[] { exitDoor });
    }

    private static void BuildDecor()
    {
        float centerBase = CenterWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float northBase = NorthWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float laserX = LaserColumn * LevelBuildKit.CellSize;

        LevelBuildKit.PlaceDecal("Laser Emitter", "laser_emitter", new Vector2(laserX, EmitterFeet), 1.2f);

        LevelBuildKit.PlaceWallDecor("Control Panel A", "wall_control_panel", new Vector2(9f, centerBase + 0.3f), 0.7f, centerBase);
        LevelBuildKit.PlaceWallDecor("Control Panel D", "wall_control_panel", new Vector2(23f, northBase + 0.3f), 0.7f, northBase);
        LevelBuildKit.PlaceProp("Lab Table", "lab_table", new Vector2(8.6f, 13.2f), 2.5f);
        LevelBuildKit.PlaceProp("Computer Terminal", "computer_terminal", new Vector2(15f, 13.3f), 1.4f);
        LevelBuildKit.PlaceProp("Warning Fence", "warning_sign", new Vector2(6.4f, 6.3f), 0.7f);
        LevelBuildKit.PlaceProp("Barrel 1", "barrel", new Vector2(23.5f, -0.6f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel 2", "barrel", new Vector2(24.4f, -0.8f), 0.8f);
    }

    private static void BuildTriggers()
    {
        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(1, 2)), new Vector2(4f, 4f),
            "Çıkışa ulaş: Kutuları butonlara ve lazere ittir (Q)",
            "Tellerden kutu geçmez ama robot yalıtımlı, o geçebilir.");
        LevelBuildKit.CreateHintZone("Hint Laser Room", LevelBuildKit.CellCenter(new Vector2Int(MiddleWall + 1, 2)), new Vector2(2f, 6f),
            "Lazeri kes ve D2'den geç",
            "Butonlar bir kez basılınca kapı açık kalıyor. Kutuyu butondan alsak da olur.");
        LevelBuildKit.CreateLevelExit(new Vector2Int(CorridorEnd - 1, ExitRow), "Level04", "Bölüm 3 tamamlandı!");
    }
}
