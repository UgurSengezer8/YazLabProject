using System;

namespace Karantina13.Dialogue
{
    public interface IOutsideVoiceChannel
    {
        event Action<string> MessageReceived;
        event Action<string> RequestFailed;
        void Send(string playerMessage);
    }
}
