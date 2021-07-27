using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetControlMessage : INetMessage
{

    string message;
    
    public NetControlMessage(string message,ControlMessageType type)
    {
        this.message = NetControlMessage.headers[(int)type]+message;
    }
    public NetControlMessage(string message)
    {
        if (NetControlMessage.IsControlMessage(message))
        {
            this.message = message;
        }
        else
        {
            throw new System.Exception("Can't add non control message to NetControlMessage object.");
        }
    }

    
    public static List<string> headers = new List<string>()
    {
        "//?}[]||.,1:",
        ":1,.||[]}?//",
        "-!?><:[]_=!|",
        "||}{<>;[]=!-"
    };
    
    
    public string GetMessage(out ControlMessageType type)
    {
        int num = 0;
        foreach(string header in headers)
        {
            if (this.message.Contains(header))
            {
                type = (ControlMessageType)num;
                return this.message.Replace(header, "");
            }
            num++;
        }
        type = ControlMessageType.NONE;
        return this.message;

    }
    public string GetMessage()
    {
        int num = 0;
        foreach (string header in headers)
        {
            if (this.message.Contains(header))
            {
                
                return this.message.Replace(header, "");
            }
            num++;
        }
        return this.message;
    }
    public string GetRawMessage()
    {
        return this.message;
    }


    public static bool IsControlMessage(string message)
    {
        foreach(string header in NetControlMessage.headers)
        {
            if (message.Contains(header))
            {
                return true;
            }
        }
        return false;
    }
}
public enum ControlMessageType
{
    INITIALIZATIONREQUEST = 0,
    INITIALIZATIONASSIGNMENT = 1,
    ONLINECHECK = 2,
    NONE = 3

}
