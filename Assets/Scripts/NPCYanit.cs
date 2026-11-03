
using System;

[Serializable]
public class NPCYanit
{
    public string action;
    public string response;
}

[Serializable]
public class NPCIslemSonucu
{
    
    public string action;

    
    public bool success;

    public string reasonCode;

    
    public float previousHp;
    public float currentHp;
    public float maxHp;

    public float previousDamage;
    public float currentDamage;
}
