using UnityEngine;

namespace Karantina13.Companion
{
    public interface ICompanionCommandReceiver
    {
        Transform Transform { get; }
        void Execute(CompanionCommand command);
    }
}
