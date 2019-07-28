using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SkillScript : MonoBehaviour {

    public string sid;
    public string bid;

    public int skillEnum;
    Skill skill;

    GameObject parentRole;
    RoleScript parentRoleScript;
    RoleState roleState;

    void Awake() {

    }

    void Start() {

        bid = getBid();

        parentRole = GameObject.Find(sid);

        parentRoleScript = parentRole.GetComponent<RoleScript>();

        roleState = parentRoleScript.roleState;

        skill = roleState.skillList[skillEnum];

        skill.buffLast = skill.buffLastOrigin;

        skill.cd = skill.cdOrigin;

        Debug.Log(roleState.skillList[skillEnum].buffLast + "/" + skill.buffLast);

    }

    void Update() {
        
        onFlying();

        onBuffing();

        onOut();

    }

    string getBid() {

        string originStr = "";

        originStr += DateTime.Now.Millisecond.ToString();

        originStr += sid;

        MD5 md5 = new MD5CryptoServiceProvider();

        byte[] s = Encoding.UTF8.GetBytes(originStr);

        byte[] c = md5.ComputeHash(s);

        return Convert.ToBase64String(c);

    }

    void onFlying() {

        if (skill.isBuff) return;

        int direct = (roleState.isLeftAlly) ? 1 : -1;

        gameObject.transform.Translate(Vector2.right * direct * skill.flySpeed * Time.deltaTime);

    }

    void onBuffing() {

        if (!skill.isBuff) return;

        if (skill.buffLast > 0) {

            skill.buffLast -= Time.deltaTime;

        } else {

            Destroy(this.gameObject);

        }
    }

    void onOut() {

    }

    // void OnTriggerEnter (Collider col) {

        // Debug.Log("out" + col.name);
        
    // }
}
