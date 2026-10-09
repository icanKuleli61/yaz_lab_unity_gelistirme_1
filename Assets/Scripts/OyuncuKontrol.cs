using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class OyuncuKontrol : MonoBehaviour
{

    [SerializeField]
    private GemiHareket gemiHareket;

    [SerializeField]
    private Camera camera;

    // Update is called once per frame
    void Update()
    {
        

        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 ekranTiklamaKonumu = Mouse.current.position.ReadValue();
            Debug.Log(ekranTiklamaKonumu);
            Ray isin = camera.ScreenPointToRay(ekranTiklamaKonumu);


            Plane hareketDuzlemi = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            if (hareketDuzlemi.Raycast(isin, out float uzaklik))
            {
                Vector3 hedefKonum = isin.GetPoint(uzaklik);
                gemiHareket.HareketEt(hedefKonum);
            }
        }


   

    }
}
