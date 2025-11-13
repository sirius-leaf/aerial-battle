using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealisitcExplosion55 : MonoBehaviour
{
    private Transform camTransform;
    private CameraControl55 camCtrl;

    void Start() { camCtrl = GameObject.FindGameObjectWithTag("CameraControl").GetComponent<CameraControl55>(); }

    void Update()
    {
        camTransform = camCtrl.cam[camCtrl.camMode].gameObject.transform;
        transform.LookAt(camTransform);
    }
    
    public void DestroySelf() { Destroy(gameObject); }
}
