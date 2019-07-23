using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    public RectTransform battlePanelRect;
    public BulletInfo bulletInfo;
    // public float shootSpeed;
    // public float direct = 1;
    // public float dmg;

    void Awake() {

        // bulletInfo = new BulletInfo(PlayerDataScript.sid);

    }

    void Start() {

        battlePanelRect = transform.parent.GetComponent<RectTransform>();

    }

    

    void Update() {
        
        onFlying();

        onOut();

    }

    void onFlying() {

        gameObject.transform.Translate(Vector3.right * bulletInfo.direct * bulletInfo.shootSpeed * Time.deltaTime);

    }

    void onOut() {

        if (transform.localPosition.x > battlePanelRect.rect.width / 2 || transform.localPosition.x < - battlePanelRect.rect.width / 2) {

            Destroy(this.gameObject);

        }
    }

    // void OnTriggerEnter (Collider col) {

        // Debug.Log("out" + col.name);
        
    // }
}
