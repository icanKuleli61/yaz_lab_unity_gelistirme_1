using UnityEngine;

public class Devolarium : Boss
{
    public Devolarium()
        : base(new BossOzellikleri(
            1000f, 
            1000f, 
            10f,   
            5f,    
            2f,    
            50f   
        ))
    {

    }

    public override void saldir()
    {
        // Devolarium saldırı mekanizması
    }
}