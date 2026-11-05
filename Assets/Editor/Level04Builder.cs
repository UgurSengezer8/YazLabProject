using System.Collections.Generic;
using Karantina13.Level;
using Karantina13.Puzzle;
using Karantina13.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level04Builder
{
    public const string ScenePath = "Assets/Scenes/Level04.unity";

    private const int WestRoomEnd = 5;
    private const int SplitRow = 4;
    private const int MiddleWall = 6;
    private const int EastWall = 14;
    private const int NorthWall = 8;
    private const int ExitRow = 1;
    private const int CorridorEnd = 19;
    private const int BoxLane = 6;
    private const int LaserColumn = 10;
    private const float HoldTolerance = 1f;
    private const float EmitterFeet = -1.15f;
    private const float CompanionRadius = 6f;

    private static readonly Vector2Int GateDoor = new Vector2Int(MiddleWall, 1);
    private static readonly Vector2Int VaultDoor = new Vector2Int(MiddleWall, BoxLane);
    private static readonly Vector2Int ExitDoor = new Vector2Int(EastWall, ExitRow);
    private static readonly Vector2Int FenceCell = new Vector2Int(2, SplitRow);
    private static readonly Vector2Int BoxHome = new Vector2Int(1, BoxLane);
    private static readonly Vector2Int StartPlate = new Vector2Int(1, 1);
    private static readonly Vector2Int VaultPlate = new Vector2Int(4, BoxLane);
    private static readonly Vector2Int HallPlate = new Vector2Int(8, 3);
    private static readonly Vector2Int RobotPlate = new Vector2Int(12, 2);
    private static readonly Vector2Int CratePlate = new Vector2Int(13, BoxLane);
    private static readonly Vector2Int StartResetCell = new Vector2Int(0, SplitRow - 1);
    private static readonly Vector2Int HallResetCell = new Vector2Int(11, 0);
    private static readonly Vector2Int VaultResetCell = new Vector2Int(MiddleWall + 1, NorthWall - 1);
    private static readonly Vector2Int LeverCell = new Vector2Int(EastWall + 2, ExitRow);

    [MenuItem("Karantina 13/Bolum 4 Olustur")]
    public static void BuildFromMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Build();
    }

    public static void Build()
    {
        Scene scene = LevelBuildKit.BeginScene(UsedSpritePaths());
        BuildFloors();
        BuildWalls();
        GameObject box = BuildPuzzles();
        BuildDecor();

        GameObject player = LevelBuildKit.SpawnPlayer(new Vector2Int(2, 2));
        GameObject robot = LevelBuildKit.SpawnRobot(new Vector2Int(4, 1));
        if (player.GetComponent<PlateWeight>() == null) player.AddComponent<PlateWeight>();
        if (robot.GetComponent<PlateWeight>() == null) robot.AddComponent<PlateWeight>();
        BuildTriggers(box, robot.transform);

        float right = CorridorEnd * LevelBuildKit.CellSize + 1f;
        float top = NorthWall * LevelBuildKit.CellSize + 1.6f;
        const float bottom = -2.2f;
        Camera camera = LevelBuildKit.CreateCamera(player.transform, Rect.MinMaxRect(-3f, bottom, right, top));
        camera.orthographicSize = (top - bottom) * 0.5f;
        GameObject systems = LevelBuildKit.CreateSystems(player.transform);
        systems.GetComponent<CompanionCommandMenu>().SetManualPushTargets(true);

        LevelBuildKit.FinishScene(scene, ScenePath, BuildIndexAfter(Level03Builder.ScenePath));
        Debug.Log("Level04 olusturuldu");
    }

    private static int BuildIndexAfter(string previousScene)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        int index = 0;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == ScenePath) continue;
            if (scenes[i].path == previousScene) return index + 1;
            index++;
        }
        return index;
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
            "crate", "barrel", "computer_terminal", "warning_sign",
            "wall_control_panel", "switch_lever", "laser_emitter", "laser_receiver"
        };
        foreach (string tile in tiles) yield return LevelBuildKit.TilePath(tile);
        foreach (string prop in props) yield return LevelBuildKit.PropPath(prop);
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.BarrierFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.DoorFrame)) yield return frame;
        foreach (string frame in LevelBuildKit.FramePaths(LevelBuildKit.RayFrame)) yield return frame;
        yield return LevelBuildKit.RayImpact;
    }

    private static void BuildFloors()
    {
        LevelBuildKit.FillRoomFloor(0, WestRoomEnd, 0, SplitRow - 1);
        LevelBuildKit.FillRoomFloor(0, WestRoomEnd, SplitRow + 1, NorthWall - 1);
        LevelBuildKit.FillRoomFloor(MiddleWall + 1, EastWall - 1, 0, NorthWall - 1);

        var doorCells = new List<Vector2Int> { FenceCell };
        doorCells.AddRange(LevelBuildKit.DoorCells(GateDoor, true));
        doorCells.AddRange(LevelBuildKit.DoorCells(VaultDoor, true));
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
            Run(-1, -1, CorridorEnd, -1),
            Run(-1, NorthWall, EastWall, NorthWall),
            Run(-1, 0, -1, NorthWall - 1),
            Run(0, SplitRow, FenceCell.x - 1, SplitRow),
            Run(FenceCell.x + 1, SplitRow, WestRoomEnd, SplitRow),
            Run(MiddleWall, GateDoor.y + 2, MiddleWall, VaultDoor.y - 2),
            Run(EastWall, ExitRow + 2, EastWall, NorthWall - 1),
            Run(EastWall + 1, ExitRow + 2, CorridorEnd, ExitRow + 2),
            Run(CorridorEnd, ExitRow - 1, CorridorEnd, ExitRow + 1)
        };
        LevelBuildKit.BuildWalls(runs);
    }

    private static RectInt Run(int fromX, int fromY, int toX, int toY) => LevelBuildKit.Run(fromX, fromY, toX, toY);

    private static GameObject BuildPuzzles()
    {
        SlidingDoor gate = LevelBuildKit.CreateSlidingDoor("Gate Door", GateDoor, true);
        SlidingDoor vault = LevelBuildKit.CreateSlidingDoor("Vault Door", VaultDoor, true);
        SlidingDoor exit = LevelBuildKit.CreateSlidingDoor("Exit Door", ExitDoor, true);
        gate.SetLatch(false);
        vault.SetLatch(false);
        exit.SetLatch(false);

        PressurePlate startPlate = HoldPlate("Plate Start", StartPlate, new MonoBehaviour[0]);
        PressurePlate vaultPlate = HoldPlate("Plate Vault", VaultPlate, new MonoBehaviour[0]);
        HoldPlate("Plate Hall", HallPlate, new MonoBehaviour[] { vault });
        PressurePlate robotPlate = HoldPlate("Plate East A", RobotPlate, new MonoBehaviour[0]);
        PressurePlate cratePlate = HoldPlate("Plate East B", CratePlate, new MonoBehaviour[0]);

        CreateGate("Gate Logic", new[] { startPlate, vaultPlate }, gate);
        CreateGate("Exit Logic", new[] { robotPlate, cratePlate }, exit);

        LevelBuildKit.CreateElectricFence(FenceCell.x, FenceCell.y, FenceCell.y);

        float wallFace = NorthWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float floorFace = -LevelBuildKit.CellSize * 0.5f + 0.7f;
        var origin = new Vector2(LaserColumn * LevelBuildKit.CellSize, wallFace);
        float goal = wallFace - BoxLane * LevelBuildKit.CellSize;
        LevelBuildKit.CreateLaserRay("Laser", origin, Vector2.down, wallFace - floorFace, goal);

        GameObject box = LevelBuildKit.CreatePushableBox("Crate", BoxHome);

        float corridorTop = (ExitRow + 2) * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float leverX = LeverCell.x * LevelBuildKit.CellSize;
        GameObject leverVisual = LevelBuildKit.PlaceWallDecor("Lever Visual", "switch_lever", new Vector2(leverX, corridorTop + 0.25f), 0.8f, corridorTop);
        GameObject leverZone = LevelBuildKit.CreateTrigger("Exit Lever", LevelBuildKit.CellCenter(LeverCell), new Vector2(1.6f, LevelBuildKit.CellSize * LevelBuildKit.DoorSpan));
        leverZone.AddComponent<WallLever>().Configure(
            new[] { exit },
            new Behaviour[0],
            leverVisual.GetComponent<SpriteRenderer>(),
            "Kapı kilitlendi. Robotu yanına çağır.");

        return box;
    }

    private static PressurePlate HoldPlate(string name, Vector2Int cell, MonoBehaviour[] targets)
    {
        PressurePlate plate = LevelBuildKit.CreatePressurePlate(name, cell, targets);
        plate.ConfigureHold(HoldTolerance);
        return plate;
    }

    private static void CreateGate(string name, PressurePlate[] inputs, SlidingDoor door)
    {
        var go = new GameObject(name);
        go.transform.SetParent(door.transform.parent, false);
        go.transform.position = door.transform.position;
        go.AddComponent<PlateGate>().Configure(inputs, new MonoBehaviour[] { door });
    }

    private static void BuildDecor()
    {
        float northBase = NorthWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        float laserX = LaserColumn * LevelBuildKit.CellSize;
        LevelBuildKit.PlaceWallDecor("Laser Emitter", "laser_emitter", new Vector2(laserX, northBase + 0.1f), 1f, northBase);
        LevelBuildKit.PlaceDecal("Laser Receiver", "laser_receiver", new Vector2(laserX, EmitterFeet), 0.62f);

        float splitBase = SplitRow * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        LevelBuildKit.PlaceWallDecor("Fence Warning", "warning_sign", new Vector2(6.6f, splitBase + 0.2f), 0.6f, splitBase);
        LevelBuildKit.PlaceWallDecor("Vault Panel", "wall_control_panel", new Vector2(6f, northBase + 0.3f), 0.7f, northBase);
        LevelBuildKit.PlaceProp("Barrel 1", "barrel", new Vector2(24.6f, 13.3f), 0.8f);
        LevelBuildKit.PlaceProp("Barrel 2", "barrel", new Vector2(23.6f, 13.5f), 0.8f);
    }

    private static void BuildTriggers(GameObject box, Transform robot)
    {
        var body = box.GetComponent<Rigidbody2D>();
        float splitBase = SplitRow * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;

        Vector2 startReset = LevelBuildKit.CellCenter(StartResetCell);
        LevelBuildKit.PlaceWallDecor("Reset Panel Start", "wall_control_panel", new Vector2(startReset.x, splitBase + 0.3f), 0.7f, splitBase);
        CreateResetZone("Box Reset Start", startReset, body, LevelBuildKit.CellCenter(BoxHome));

        float northBase = NorthWall * LevelBuildKit.CellSize - LevelBuildKit.CellSize * 0.5f;
        Vector2 vaultReset = LevelBuildKit.CellCenter(VaultResetCell);
        LevelBuildKit.PlaceWallDecor("Reset Panel Hall", "wall_control_panel", new Vector2(vaultReset.x, northBase + 0.3f), 0.7f, northBase);
        CreateResetZone("Box Reset Vault", vaultReset, body, LevelBuildKit.CellCenter(BoxHome));

        Vector2 hallReset = LevelBuildKit.CellCenter(HallResetCell);
        LevelBuildKit.PlaceDecal("Reset Terminal Hall", "computer_terminal", new Vector2(hallReset.x, EmitterFeet + 0.2f), 1.3f);
        CreateResetZone("Box Reset Hall", hallReset, body, LevelBuildKit.CellCenter(new Vector2Int(LaserColumn, BoxLane)));

        LevelBuildKit.CreateHintZone("Hint Start", LevelBuildKit.CellCenter(new Vector2Int(2, 2)), new Vector2(4f, 4f),
            "Çıkışa ulaş. Kutu tek, ağırlık üç.",
            "Bu kapı iki plakayı birden istiyor.");
        LevelBuildKit.CreateHintZone("Hint Reset", startReset, new Vector2(2f, 2f), null,
            "Kutu sıkışırsa panele dokun.");

        LevelExit levelExit = LevelBuildKit.CreateLevelExit(new Vector2Int(CorridorEnd - 1, ExitRow), "", "Bölüm 4 tamamlandı!",
            new Vector2(LevelBuildKit.CellSize, LevelBuildKit.CellSize * LevelBuildKit.DoorSpan));
        levelExit.RequireCompanion(robot, CompanionRadius, "Robotu burada bırakamam.");
    }

    private static void CreateResetZone(string name, Vector2 center, Rigidbody2D body, Vector2 home)
    {
        GameObject zone = LevelBuildKit.CreateTrigger(name, center, new Vector2(1.4f, 1.4f));
        zone.AddComponent<BoxResetSwitch>().Configure(body, home);
    }
}
