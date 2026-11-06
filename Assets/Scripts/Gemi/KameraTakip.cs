using UnityEngine;

public class KameraTakip : MonoBehaviour
{
    [SerializeField] private Transform takipEdilecekGemi;
    [SerializeField]
    private Vector3 kameraOfseti =
        new Vector3(0f, 12f, -8f);

    void LateUpdate()
    {
        if (takipEdilecekGemi == null)
            return;

        transform.position =
            takipEdilecekGemi.position + kameraOfseti;
    }
}