using System;
using System.Collections.Generic;

[Serializable]
public class RoleListRecvInfo {

    public Dictionary<string, RoleState> roleJson = new Dictionary<string, RoleState>();
    
}