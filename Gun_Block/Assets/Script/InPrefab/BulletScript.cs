using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    public RectTransform battlePanelRect;
    public BulletSkill bulletInfo;
    public sbyte direct;

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

        gameObject.transform.Translate(Vector3.right * direct * bulletInfo.flySpeed * Time.deltaTime);

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
