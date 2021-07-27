using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface representing all NetMessage objects that can be sent accross the network.
/// </summary>
public interface INetMessage
{
    string GetMessage();
    string GetRawMessage();
}
