using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletState {
    public bool isColRole = false;
    public bool isColBlock = false;
    public bool isColOut = false;
}
public class BulletScript : MonoBehaviour {

    public RectTransform battlePanelRect;
    public bool isShootToRight;
    public float shootSpeed;
    public BulletState bulletState;

    public float direct = 1;

    void Awake() {

        isShootToRight = true;

        shootSpeed = 1f;

        bulletState = new BulletState();

    }

    void Start() {

        battlePanelRect = transform.parent.GetComponent<RectTransform>();

    }

    void Update() {
        
        onFlying();

        onOut();

    }

    void onFlying() {

        gameObject.transform.Translate(Vector3.right * direct * shootSpeed * Time.deltaTime);

    }

    void onRole() {

    }

    void onBlock() {

    }

    void onOut() {

        if (transform.localPosition.x > battlePanelRect.rect.width / 2 || transform.localPosition.x < - battlePanelRect.rect.width / 2) {

            DestroyImmediate(this.gameObject);

        }
    }

    void OnTriggerEnter (Collider col) {

        // Debug.Log("out" + col.name);
        
    }
}
