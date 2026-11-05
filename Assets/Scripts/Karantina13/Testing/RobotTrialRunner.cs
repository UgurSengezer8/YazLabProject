using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Karantina13.Companion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Karantina13.Testing
{
    public sealed class RobotTrialRunner : MonoBehaviour
    {
        private enum Phase { Starting, Running, Cooldown, Finished }

        [SerializeField] private Transform robot;
        [SerializeField] private Transform target;
        [SerializeField] private List<Vector2> spawnPoints = new List<Vector2>();
        [SerializeField] private float reachDistance = 1.3f;
        [SerializeField] private float minSpawnDistance = 6f;
        [SerializeField] private float timeout = 30f;
        [SerializeField] private float cooldown = 0.6f;
        [SerializeField] private int maxTrials = 20;
        [SerializeField] private bool randomizeRobotStart;
        [SerializeField] private int seed;
        [SerializeField] private string csvFileName = "robot_test_sonuclari.csv";

        private readonly List<TrialResult> results = new List<TrialResult>();
        private ICompanionCommandReceiver receiver;
        private Rigidbody2D robotBody;
        private System.Random random;
        private Phase phase = Phase.Starting;
        private float phaseTimer;
        private float trialStart;
        private Vector2 trialFrom;
        private int lastSpawnIndex = -1;
        private string csvPath;
        private GUIStyle panelStyle;
        private GUIStyle textStyle;
        private GUIStyle titleStyle;

        public IReadOnlyList<TrialResult> Results => results;
        public int SuccessCount { get; private set; }
        public float SuccessRate => results.Count == 0 ? 0f : (float)SuccessCount / results.Count;
        public float AverageSuccessTime { get; private set; }

        public void Configure(Transform robot, Transform target, IEnumerable<Vector2> spawnPoints)
        {
            this.robot = robot;
            this.target = target;
            this.spawnPoints = new List<Vector2>(spawnPoints);
        }

        private void Awake()
        {
            random = seed == 0 ? new System.Random() : new System.Random(seed);
            csvPath = Path.Combine(Application.persistentDataPath, csvFileName);
        }

        private void Start()
        {
            if (robot != null)
            {
                robotBody = robot.GetComponent<Rigidbody2D>();
                receiver = FindReceiver(robot);
            }
            WriteCsvHeader();
            phaseTimer = 0.5f;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Restart();
            if (robot == null || target == null || spawnPoints.Count == 0) return;

            switch (phase)
            {
                case Phase.Starting:
                case Phase.Cooldown:
                    phaseTimer -= Time.deltaTime;
                    if (phaseTimer <= 0f) BeginTrial();
                    break;
                case Phase.Running:
                    TickTrial();
                    break;
            }
        }

        private void BeginTrial()
        {
            if (maxTrials > 0 && results.Count >= maxTrials)
            {
                phase = Phase.Finished;
                Debug.Log(Summary() + "\nCSV: " + csvPath);
                return;
            }

            if (randomizeRobotStart) TeleportRobot(PickSpawn(target.position));
            target.position = PickSpawn(robot.position);
            trialFrom = robot.position;
            trialStart = Time.time;
            phase = Phase.Running;
            receiver?.Execute(CompanionCommand.MoveTo(target.position));
        }

        private void TickTrial()
        {
            float elapsed = Time.time - trialStart;
            if (Vector2.Distance(robot.position, target.position) <= reachDistance)
            {
                EndTrial(true, elapsed);
            }
            else if (timeout > 0f && elapsed >= timeout)
            {
                EndTrial(false, elapsed);
            }
        }

        private void EndTrial(bool success, float elapsed)
        {
            var result = new TrialResult(results.Count + 1, trialFrom, target.position, success, elapsed);
            results.Add(result);
            if (success)
            {
                SuccessCount++;
                AverageSuccessTime += (elapsed - AverageSuccessTime) / SuccessCount;
            }
            AppendCsv(result);
            Debug.Log(result.ToLogLine());

            if (!success) receiver?.Execute(CompanionCommand.Wait());
            phase = Phase.Cooldown;
            phaseTimer = cooldown;
        }

        private void Restart()
        {
            results.Clear();
            SuccessCount = 0;
            AverageSuccessTime = 0f;
            lastSpawnIndex = -1;
            WriteCsvHeader();
            receiver?.Execute(CompanionCommand.Wait());
            phase = Phase.Starting;
            phaseTimer = 0.5f;
        }

        private Vector2 PickSpawn(Vector2 awayFrom)
        {
            var candidates = new List<int>();
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                if (i == lastSpawnIndex) continue;
                if (Vector2.Distance(spawnPoints[i], awayFrom) >= minSpawnDistance) candidates.Add(i);
            }
            if (candidates.Count == 0)
            {
                for (int i = 0; i < spawnPoints.Count; i++) candidates.Add(i);
            }
            int index = candidates[random.Next(candidates.Count)];
            lastSpawnIndex = index;
            return spawnPoints[index];
        }

        private void TeleportRobot(Vector2 position)
        {
            if (robotBody != null)
            {
                robotBody.position = position;
                robotBody.linearVelocity = Vector2.zero;
            }
            robot.position = position;
        }

        private static ICompanionCommandReceiver FindReceiver(Transform robot)
        {
            foreach (MonoBehaviour behaviour in robot.GetComponentsInChildren<MonoBehaviour>())
            {
                if (behaviour is ICompanionCommandReceiver found) return found;
            }
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is ICompanionCommandReceiver found) return found;
            }
            return null;
        }

        private void WriteCsvHeader()
        {
            TryWrite(() => File.WriteAllText(csvPath, "deneme;baslangic_x;baslangic_y;hedef_x;hedef_y;kus_ucusu_mesafe;sure_sn;sonuc\n", Encoding.UTF8));
        }

        private void AppendCsv(TrialResult result)
        {
            TryWrite(() => File.AppendAllText(csvPath, result.ToCsvLine() + "\n", Encoding.UTF8));
        }

        private void TryWrite(System.Action write)
        {
            try
            {
                write();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("CSV yazilamadi: " + exception.Message);
            }
        }

        private string Summary()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Robot testi: {0}/{1} basarili (%{2:0.0}), ortalama sure {3:0.00} sn",
                SuccessCount, results.Count, SuccessRate * 100f, AverageSuccessTime);
        }

        private void OnGUI()
        {
            EnsureStyles();
            float width = 360f;
            float height = 196f;
            GUI.Box(new Rect(16, 16, width, height), GUIContent.none, panelStyle);
            GUI.Label(new Rect(30, 24, width - 28, 26), "ROBOT YOL BULMA TESTİ", titleStyle);

            float elapsed = phase == Phase.Running ? Time.time - trialStart : 0f;
            string trialText = maxTrials > 0 ? (results.Count + (phase == Phase.Running ? 1 : 0)) + " / " + maxTrials : (results.Count + 1).ToString();
            string last = results.Count > 0 ? results[results.Count - 1].Describe() : "-";
            string state = receiver == null ? "Komut alıcısı yok (ok tuşlarıyla sür)" : StateText();

            var lines = new StringBuilder();
            lines.AppendLine("Deneme: " + trialText);
            lines.AppendLine(string.Format(CultureInfo.InvariantCulture, "Geçen süre: {0:0.00} sn", elapsed));
            lines.AppendLine(string.Format(CultureInfo.InvariantCulture, "Başarı: {0}/{1}  (%{2:0.0})", SuccessCount, results.Count, SuccessRate * 100f));
            lines.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ortalama süre: {0:0.00} sn", AverageSuccessTime));
            lines.AppendLine("Son: " + last);
            lines.AppendLine("Durum: " + state);
            lines.Append("R: Testi sıfırla");
            GUI.Label(new Rect(30, 54, width - 28, height - 44), lines.ToString(), textStyle);
        }

        private string StateText()
        {
            switch (phase)
            {
                case Phase.Running: return "Kutuya gidiyor";
                case Phase.Finished: return "Bitti";
                default: return "Kutu yerleşiyor";
            }
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.09f, 0.88f));
            background.Apply();
            panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = background } };
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, normal = { textColor = new Color32(0xF2, 0xB5, 0x44, 0xFF) } };
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(0.9f, 0.92f, 0.94f) } };
        }
    }
}
