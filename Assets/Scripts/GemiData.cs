using System;


public  class GemiData
{
    private String _gemiAdi;
    private float _gemiHizi;
    private float _donusHizi;
    private float _anlikcan;

    private float _maxsimumCan;

    private float _hasar;

    public GemiData(String gemiAdi, float gemiHizi, float donusHizi, float anlikCan, float maxsimumCan, float hasar)
    {
        this._gemiAdi = gemiAdi;
        this._gemiHizi = gemiHizi;
        this._donusHizi = donusHizi;
        this._anlikcan = anlikCan;
        this._maxsimumCan = maxsimumCan;
        this._hasar = hasar;
    }
    public String GetGemiAdı() {  return _gemiAdi; }

    public float GetGemiHizi() { return _gemiHizi; }

    public float GetDonusHizi() { return _donusHizi; }

    public float GetCan() { return _anlikcan; }
    public float GetmaxsimumCan() { return _maxsimumCan; }

    public float GetHasar() { return _hasar; }

}
