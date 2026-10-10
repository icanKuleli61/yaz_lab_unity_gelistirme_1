using UnityEngine;

public abstract class Gemi 
{
    private GemiData gemiVerileri;

    protected Gemi(GemiData gemiVerileri)
    {
        this.gemiVerileri = gemiVerileri;
    }

    public abstract void AtesEt();

    public abstract void HasarAl(float hasar);

    public abstract void Iyiles();

    public GemiData getGemiVerileri()
    {
        return gemiVerileri;
    }
}





