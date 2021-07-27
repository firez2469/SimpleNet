using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
using System;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Derived class handles all functions related with setting up and running a URP network.
/// The class allows for messages to be sent and recieved, as well as having the server assign
/// netId's to each client. The class can be used for both server or client objects.
/// </summary>
public class NetBehavior : MonoBehaviour
{
    private UdpClient udpClient;
    public string ip = "127.0.0.1";
    public int port = 8052;
    private IPEndPoint RemoteIpEndPoint;
    [HideInInspector]
    public List<IPAddress> connectedIps = new List<IPAddress>();
    public bool isServer = false;

    [HideInInspector]
    public bool debug = false;
    [HideInInspector]
    public int netId = 0;
    
    [HideInInspector]
    public int startingNetId = 1000;
    private int tempNetId;
    [HideInInspector]
    public bool initializedOnNetwork = false;
    private int initializationRequestInterval = 3;
    /// <summary>
    /// Only a Server based field.
    /// </summary>
    [HideInInspector]
    private Dictionary<string, int> addressToNetId = new Dictionary<string, int>();

    #region UDPInitialization
    /// <summary>
    /// Initializes the UDP listener by setting up connections, initializing listeners, and setting up netIds
    /// on network.
    /// </summary>
    public void InitializeUdp()
    {
        tempNetId = UnityEngine.Random.Range(0, 100000);
        try
        {
            if (isServer)
                udpClient = new UdpClient(port);
            else
                udpClient = new UdpClient(ip, port);

            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, this.port);
            Thread t = new Thread(new ThreadStart(UDPListenForData));
            t.IsBackground = true;
            t.Start();
            StartCoroutine(InitializeOnNetwork());
        }
        catch (Exception e)
        {
            print("Exception:" + e.ToString());
        }
    }

    private void UDPListenForData()
    {
        try
        {
            while (true)
            {
                Byte[] data = udpClient.Receive(ref this.RemoteIpEndPoint);
                string returnData = Encoding.UTF8.GetString(data);
                if(debug)
                    print("recieved from " + RemoteIpEndPoint.Address.ToString() + ": " + returnData);
                
                if (!connectedIps.Contains(RemoteIpEndPoint.Address)&&isServer)
                {
                    IPAddress address1 = RemoteIpEndPoint.Address;
                    connectedIps.Add(address1);
                    
                    this.addressToNetId.Add(address1.ToString(), -1);
                   
                }
                
                if (NetControlMessage.IsControlMessage(returnData))
                {
                    this.ProcessControlMessage(returnData,RemoteIpEndPoint.Address);
                }
                else
                {
                    this.OnMessageReceive(returnData);
                }

                

            }


        }
        catch (Exception e)
        {
            print(e.Message);
        }

    }

    private void UDPSendData(IPAddress address, int port, string message)
    {
        udpClient.Connect(address, port);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(message);
        udpClient.Send(sendBytes, sendBytes.Length);
        
    }

    /// <summary>
    /// Sends data over the network.
    /// </summary>
    /// <param name="message">The message being sent.</param>
    public void SendNetData(string message)
    {
        if (this.isServer)
        {
            foreach(IPAddress address in this.connectedIps)
            {
                try
                {
                    this.UDPSendData(address, this.port, message);
                }catch(Exception e)
                {
                    //if it gets an exception that means a user disconnected.
                    this.OnUserDisconnect(this.addressToNetId[address.ToString()]);
                }
                
            }
        }
        else
        {
            this.UDPSendData(IPAddress.Parse(this.ip), this.port, message);
        }

    }

    /// <summary>
    /// Sends data over the network.
    /// </summary>
    /// <param name="message">The INetMessage object being sent.</param>
    public void SendNetData(INetMessage message)
    {
        this.SendNetData(message.GetRawMessage());

    }

    /// <summary>
    /// Runs when NetBehavior object recieves a message.
    /// </summary>
    /// <param name="message">The message being received</param>
    public virtual void OnMessageReceive(string message)
    {

    }

    /// <summary>
    /// [Server ONLY] method which is called whenever a user disconnects.
    /// </summary>
    /// <param name="netId">Net id of user disconnecting</param>
    public virtual void OnUserDisconnect(int netId)
    {

    }

    private void ProcessControlMessage(string message,IPAddress address)
    {
        NetControlMessage controlMessage = new NetControlMessage(message);
        ControlMessageType type;
        string cM = controlMessage.GetMessage(out type);
        
        switch (type)
        {
            case ControlMessageType.INITIALIZATIONASSIGNMENT:
                int newId;
                string[] splitMsg = cM.Split(':');
                int testTempId;
                if (splitMsg.Length == 2&&!isServer)
                {
                    print("parsing:" + splitMsg[0]);
                    if(int.TryParse(splitMsg[0],out testTempId))
                    {
                        print(testTempId + " compared to " + this.tempNetId);
                        if (testTempId == this.tempNetId)
                        {
                            if (int.TryParse(splitMsg[1], out newId))
                            {
                                this.netId = newId;
                                this.initializedOnNetwork = true;
                            }
                        }
                    }
                }
                
                break;
            case ControlMessageType.INITIALIZATIONREQUEST:
                NetControlMessage responseMsg = new NetControlMessage(
                    cM+":"+
                    this.startingNetId.ToString(),
                    ControlMessageType.INITIALIZATIONASSIGNMENT);
                this.SendNetData(responseMsg);
                this.addressToNetId[address.ToString()] = this.startingNetId;
                this.startingNetId++;
                break;
            case ControlMessageType.ONLINECHECK:
                break;
            case ControlMessageType.NONE:
                break;

        }
    }
    
    private IEnumerator InitializeOnNetwork()
    { 
        if (!isServer)
        {
            if (!this.initializedOnNetwork)
            {
                
                NetControlMessage cm = new NetControlMessage(
                    this.tempNetId.ToString(), 
                    ControlMessageType.INITIALIZATIONREQUEST);
                if(debug)
                    print("Sending:" + cm.GetRawMessage());
                SendNetData(cm);
            }

            yield return new WaitForSeconds(this.initializationRequestInterval);
            if (!this.initializedOnNetwork)
            { 
                StartCoroutine(InitializeOnNetwork());
            }

        }
        yield return new WaitForSeconds(0);
    }


    #endregion

}
