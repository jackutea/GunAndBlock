using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class FieldState {

    public int fieldId;
    public string modeCode;
    public Dictionary<string, RoleState> sidJson;

}