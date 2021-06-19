using UnityEngine;

public interface INetworkObject
{
    void ReceiveCommand(NetworkCommand command, params object[] values);
}
