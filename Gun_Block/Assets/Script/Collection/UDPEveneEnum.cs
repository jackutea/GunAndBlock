using System;

[Serializable]
public enum HallEventEnum {

    Login,

    Register,

    ShowServer,

    ShowRoles,

    CreateRole,

    DeleteRole,

    EnterGame,

    ShowRoom,
    
}

[Serializable]
public enum CompareEventEnum {

    Compare,
    
}

[Serializable]
public enum BattleEventEnum {

    CastSkill,

    Dead,

    ReflectBullet,

    ImmuneBullet,

    KillBullet,

    BlockBullet,

    BeAttacked,
}