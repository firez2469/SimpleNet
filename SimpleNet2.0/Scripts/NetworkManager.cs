using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : NetBehavior
{
    // Start is called before the first frame update
    void Start()
    {
        this.debug = this.isServer;
        this.InitializeUdp();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!isServer)
        {
            
            ///this.SendNetData("Hello");
            
        }
    }

    public override void OnMessageReceive(string message)
    {
        print("recieved:" + message);
    }
}
