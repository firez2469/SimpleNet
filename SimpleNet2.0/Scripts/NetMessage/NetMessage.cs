using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class stores one or more messages together for use in Server-Client ApolloNet.
/// </summary>
public class NetMessage:INetMessage
{
    /// <summary>
    /// Initializes the net message by assigning the message string.
    /// </summary>
    /// <param name="message">The message being sent.</param>
    public NetMessage(string message)
    {
        this.message = message;
    }


    public string message;
    /// <summary>
    /// Splits the message using the NetWorkParser.parser string as a divider.
    /// </summary>
    /// <returns>List of strings split by the divider.</returns>
    public string[] Split()
    {
        return NetworkParser.Parse(message);
    }

    /// <summary>
    /// Joins this message with the NetMessage added using the NetworkParser.parser as divider.
    /// </summary>
    /// <param name="m">The NetMessage being added.</param>
    public void AddMessage(NetMessage m)
    {
        this.message = NetParser.JoinForParse(m.message);
    }

    /// <summary>
    /// Joins multiple messages together using the NetworkParser.parser as divider.
    /// </summary>
    /// <param name="ms">The messages being joined.</param>
    public void AddMessages(params NetMessage[] ms)
    {
        foreach(NetMessage mess in ms)
        {
            this.message = NetParser.JoinForParse(mess.message);
        }
    }

    public string GetMessage()
    {
        return this.message;
    }
    public string GetRawMessage()
    {
        return this.message;
    }


}
