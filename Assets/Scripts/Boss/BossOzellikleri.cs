public class BossOzellikleri
{
    private float _anlik_boss_can;
    private float _maxsimum_can;
    private float _hareket_hizi;

    private float _takip_mesafesi;

    private float _saldiri_mesafesi;

    private float _hasar;


    public BossOzellikleri(float anlik_boss_can, float maxsimum_can, float hareket_hizi, float takip_mesafesi,
        float saldiri_mesafesi, float hasar
        )
    {
        this._anlik_boss_can = anlik_boss_can;
        this._maxsimum_can = maxsimum_can;
        this._hareket_hizi = hareket_hizi;
        this._takip_mesafesi = takip_mesafesi;
        this._saldiri_mesafesi= saldiri_mesafesi;
        this._hasar = hasar;

    }

    public float getAnlik_boss_can()
    {
        return _anlik_boss_can;
    }

    public void setAnlik_boss_can(float anlik_boss_can)
    {
        _anlik_boss_can = anlik_boss_can;
    }

    public float getMaxsimum_can() {
        return _maxsimum_can;
    }

    public void setMaxsimum_can(float maxsimum_can)
    {
        _maxsimum_can = maxsimum_can;
    }

    public float getHareket_hizi()
    {
        return _hareket_hizi;
    }

    public void setHareket_hizi(float hareket_hizi)
    {
        _hareket_hizi = hareket_hizi;
    }

    public float getTakip_mesafesi()
    {
        return _takip_mesafesi;
    }

    public void setTakip_mesafesi(float takip_mesafesi)
    {
        _takip_mesafesi = takip_mesafesi;
    }

    public float getSaldiri_mesafesi()
    {
        return _saldiri_mesafesi;
    }

    public void setSaldiri_mesafesi(float saldiri_mesafesi)
    {
        _saldiri_mesafesi = saldiri_mesafesi;

    }

    public float getHasar()
    {
        return _hasar;
    }


    public void setHasar(float hasar)
    {
        _hasar = hasar;
    }
}