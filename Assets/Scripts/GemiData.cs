using System;
using UnityEngine;


public  class GemiData
{
    private GemiTuru _gemiTuru;
    private float _gemiHizi;
    private float _donusHizi;
    private float _anlikcan;

    private float _maxsimumCan;

    private float _hasar;

    private float _tamirCani;

    public GemiData(GemiTuru gemiTuru, float gemiHizi, float donusHizi, float anlikCan, float maxsimumCan, float hasar, float tamirCani)
    {
        this._gemiTuru = gemiTuru;
        this._gemiHizi = gemiHizi;
        this._donusHizi = donusHizi;
        this._anlikcan = anlikCan;
        this._maxsimumCan = maxsimumCan;
        this._hasar = hasar;
        this._tamirCani = tamirCani;
    }
    public GemiTuru GetGemiAdı() {  return _gemiTuru; }

    public float GetGemiHizi() { return _gemiHizi; }

    public float GetDonusHizi() { return _donusHizi; }

    public float GetAnlıkCan() { return _anlikcan; }
    public float GetmaxsimumCan() { return _maxsimumCan; }

    public float GetHasar() { return _hasar; }

    public float GetTamirCani() { return _tamirCani; }


    public void HasarUygulama(float hasarAl)
    {
        _anlikcan = Mathf.Clamp(_anlikcan - hasarAl, 0f, _maxsimumCan);
    }

    public void TamirEt(float tamirCanı)
    {
        _anlikcan = Mathf.Clamp(_anlikcan + tamirCanı, 0f, _maxsimumCan);
    }
}
