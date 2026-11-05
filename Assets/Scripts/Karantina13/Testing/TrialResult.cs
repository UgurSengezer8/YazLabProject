using System.Globalization;
using UnityEngine;

namespace Karantina13.Testing
{
    public readonly struct TrialResult
    {
        public TrialResult(int index, Vector2 from, Vector2 to, bool success, float duration)
        {
            Index = index;
            From = from;
            To = to;
            Success = success;
            Duration = duration;
        }

        public int Index { get; }
        public Vector2 From { get; }
        public Vector2 To { get; }
        public bool Success { get; }
        public float Duration { get; }
        public float StraightDistance => Vector2.Distance(From, To);

        public string Describe()
        {
            return string.Format(CultureInfo.InvariantCulture, "#{0} {1} {2:0.00} sn", Index, Success ? "bulundu" : "zaman aşımı", Duration);
        }

        public string ToLogLine()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "[RobotTest] #{0} {1} sure={2:0.00}sn mesafe={3:0.0} ({4:0.0},{5:0.0}) -> ({6:0.0},{7:0.0})",
                Index, Success ? "BASARILI" : "ZAMAN ASIMI", Duration, StraightDistance, From.x, From.y, To.x, To.y);
        }

        public string ToCsvLine()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1:0.00};{2:0.00};{3:0.00};{4:0.00};{5:0.00};{6:0.000};{7}",
                Index, From.x, From.y, To.x, To.y, StraightDistance, Duration, Success ? "basarili" : "zaman_asimi");
        }
    }
}
