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
    private const int EastWall = 15;
    private const int NorthWall = 8;
    private const int ExitRow = 6;
    private const int CorridorEnd = 18;
    private const int LaserColumn = 12;
    private const int BoxRow = 1;
    private const float EmitterFeet = -1.15f;
    private const float RayOrigin = -0.3f;

    private static readonly Vector2Int DoorA = new Vector2Int(MiddleWall, BoxRow);
    // Lazer D2'nin tam ortasindan gecer ve D odasinin ust duvarina kadar uzanir:
    // oyuncu kapinin yan karelerinden girse bile cikisa gitmek icin lazeri gecmek zorunda.
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
            "wall_control_panel", "laser_emitter", "laser_receiver"
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

        var doorCells = new List<Vector2Int> { FenceCell };
        doorCells.AddRange(LevelBuildKit.DoorCells(DoorA, true));
        doorCells.AddRange(LevelBuildKit.DoorCells(DoorB, false));
        doorCells.AddRange(LevelBuildKit.DoorCells(ExitDoor, true));
        foreach (Vector2Int cell in doorCells) LevelBuildKit.PlaceFloor(cell, "floor_worn_metal");

        for (int x = EastWall + 1; x < CorridorEnd; x++)
        {
            for (int y = ExitRow - 1; y <= ExitRow + 1; y++) LevelBuildKit.PlaceFloor(new Vector2Int(x, y), "floor_worn_metal");
        }
    }

    private static void BuildWalls()
    {
        var runs = new List<RectInt>
        {
            Run(-1, -1, EastWall, -1),
            Run(-1, NorthWall, EastWall, NorthWall),
            Run(-1, 0, -1, NorthWall - 1),

            // D1 (DoorA) satir 0-2'yi kaplar, duvar satir 3'ten baslar.
            Run(MiddleWall, DoorA.y + 2, MiddleWall, NorthWall - 1),

            Run(0, CenterWall, FenceCell.x - 1, CenterWall),
            Run(FenceCell.x + 1, CenterWall, MiddleWall - 1, CenterWall),
            Run(MiddleWall + 1, CenterWall, DoorB.x - 2, CenterWall),
            Run(DoorB.x + 2, CenterWall, EastWall - 1, CenterWall),

            // Cikis kapisi D odasinin dogu duvarinin tamamini (satir 5-7) kaplar.
            Run(EastWall, 0, EastWall, ExitRow - 2),

            Run(EastWall + 1, ExitRow - 2, CorridorEnd, ExitRow - 2),
            Run(EastWall + 1, ExitRow + 2, CorridorEnd, ExitRow + 2),
            Run(CorridorEnd, ExitRow - 1, CorridorEnd, ExitRow + 1)
        };
        LevelBuildKit.BuildWalls(runs);
    }

    private static RectInt Run(int fromX, int fromY, int toX, int toY) => LevelBuildKit.Run(fromX, fromY, toX, toY);

    private static void BuildPuzzles()
    {
        SlidingDoor doorA = LevelBuildKit.CreateSlidingDoor("Door D1", DoorA, true);
        SlidingDoor doorB = LevelBuildKit.CreateSlidingDoor("Door D2", DoorB, false);
        SlidingDoor exitDoor = LevelBuildKit.CreateSlidingDoor("Exit Door", ExitDoor, true);

        // Tel hicbir butona bagli degil: hep acik kalir. Robot gecer, oyuncu ve kutular gecemez.
        LevelBuildKit.CreateElectricFence(FenceCell.x, FenceCell.y, FenceCell.y);

        // Lazer B'nin altindan D'nin ust duvarina kadar uzanir. Kutu noktasi BoxRow uzerinde.
        float wallFace = NorthWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float length = wallFace - RayOrigin;
        float goal = BoxRow * LevelBuildKit.CellSize - RayOrigin;
        var origin = new Vector2(LaserColumn * LevelBuildKit.CellSize, RayOrigin);
        LevelBuildKit.CreateLaserRay("Laser L", origin, Vector2.up, length, goal);

        LevelBuildKit.CreatePushableBox("Crate X", new Vector2Int(1, BoxRow));
        LevelBuildKit.CreatePushableBox("Crate Y", new Vector2Int(1, ExitRow));

        LevelBuildKit.CreatePressurePlate("Plate P1", new Vector2Int(3, BoxRow), new MonoBehaviour[] { doorA });
        LevelBuildKit.CreatePressurePlate("Plate P2", new Vector2Int(MiddleWall - 1, ExitRow), new MonoBehaviour[] { doorB });
        // P3 lazerle ayni satirda: robot kutuyu lazerden alip dogrudan saga iter.
        LevelBuildKit.CreatePressurePlate("Plate P3", new Vector2Int(EastWall - 1, BoxRow), new MonoBehaviour[] { exitDoor });
    }

    private static void BuildDecor()
    {
        float centerBase = CenterWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float northBase = NorthWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float laserX = LaserColumn * LevelBuildKit.CellSize;

        LevelBuildKit.PlaceDecal("Laser Emitter", "laser_emitter", new Vector2(laserX, EmitterFeet), 1.2f);
        LevelBuildKit.PlaceWallDecor("Laser Receiver", "laser_receiver", new Vector2(laserX, northBase + 0.35f), 0.62f, northBase);

        LevelBuildKit.PlaceWallDecor("Control Panel A", "wall_control_panel", new Vector2(9f, centerBase + 0.3f), 0.7f, centerBase);
        LevelBuildKit.PlaceWallDecor("Control Panel D", "wall_control_panel", new Vector2(19f, northBase + 0.3f), 0.7f, northBase);
        LevelBuildKit.PlaceProp("Lab Table", "lab_table", new Vector2(8.6f, 13.2f), 2.5f);
        LevelBuildKit.PlaceProp("Computer Terminal", "computer_terminal", new Vector2(15f, 13.3f), 1.4f);
        LevelBuildKit.PlaceProp("Warning Fence", "warning_sign", new Vector2(6.4f, 6.3f), 0.7f);
        LevelBuildKit.PlaceProp("Barrel 1", "barrel", new Vector2(18.6f, -0.6f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel 2", "barrel", new Vector2(19.5f, -0.8f), 0.8f);
    }

    private static void BuildTriggers()
    {
        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(1, 2)), new Vector2(4f, 4f),
            "Çıkışa ulaş: Kutuları butonlara ve lazere ittir (Q)",
            "Tellerden kutu geçmez ama robot yalıtımlı, o geçebilir.");
        LevelBuildKit.CreateHintZone("Hint Laser Room", LevelBuildKit.CellCenter(new Vector2Int(MiddleWall + 1, 2)), new Vector2(2f, 6f),
            "Lazeri kes ve D2'den geç",
            "Butonlar bir kez basılınca kapı açık kalıyor. Kutuyu butondan alsak da olur.");
        LevelBuildKit.CreateLevelExit(new Vector2Int(CorridorEnd - 1, ExitRow), "Level04", "Bölüm 3 tamamlandı!",
            new Vector2(LevelBuildKit.CellSize, LevelBuildKit.CellSize * LevelBuildKit.DoorSpan));
    }
}
