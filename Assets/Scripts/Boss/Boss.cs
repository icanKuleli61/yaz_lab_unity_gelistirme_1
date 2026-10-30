using UnityEngine;

public abstract class  Boss : MonoBehaviour
{
    public event System.Action HasarAlindi;
    protected BossOzellikleri boss_ozellikleri;

    protected Boss(BossOzellikleri boss_ozellikleri) 
    {
        this.boss_ozellikleri = boss_ozellikleri;
    }


    public BossOzellikleri Ozellikler => boss_ozellikleri;

    public abstract void saldir();

    public  void hasarAl(float hasar)
    {
        float yeniCan = boss_ozellikleri.getAnlik_boss_can() - hasar;
        yeniCan = Mathf.Max(0, yeniCan);
        boss_ozellikleri.setAnlik_boss_can(yeniCan);
        HasarAlindi?.Invoke();
    }
}
