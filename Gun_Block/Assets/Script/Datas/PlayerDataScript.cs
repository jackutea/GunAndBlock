using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataScript {

    public static string sid = "";
    public static string USER_NAME = "";
    public static string ROLE_NAME = "";
    public static Dictionary<string, RoleState> ROLES = new Dictionary<string, RoleState>();
    public static RoleState ROLE_STATE = null;
    public static FieldInfo FIELD_INFO = null;
    public static Dictionary<SkillEnum, Skill> SKILL_JSON = new Dictionary<SkillEnum, Skill>();
    
}
