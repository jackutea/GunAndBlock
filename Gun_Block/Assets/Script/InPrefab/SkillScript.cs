using System;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SkillScript : MonoBehaviour {

    public string sid;
    public string bid;
    public int direct;
    public string oppoSid = null;

    public int skillEnum;
    public string timeSample;
    public SkillState skill;

    GameObject parentRole = null;
    RoleScript parentRoleScript;
    RoleState roleState;

    bool isHit = false;

    void Awake() {

    }

    void Start() {

        initSkill();

    }

    void Update() {

        if (parentRole == null) return;
        
        onFlying();

        onBuffing();

        enermyLiveCheck();

    }

    void initSkill() {

        gameObject.name = bid;

        parentRole = GameObject.Find(sid);

        parentRoleScript = parentRole.GetComponent<RoleScript>();

        roleState = parentRoleScript.roleState;

        skill = roleState.skillList[skillEnum];

        skill.buffLast = skill.buffLastOrigin;

        skill.cd = skill.cdOrigin;

        direct = (roleState.isLeftAlly) ? 1 : -1;

        initPosition();

    }

    void initPosition() {

        Vector2 skillPo = gameObject.transform.localPosition;

        Vector2 newPo = Vector2.zero;

        if (skillEnum == (int)SkillEnum.block) {

            newPo.x = 13;

            newPo.y = -20;

        } else if (skillEnum == (int)SkillEnum.normalBullet) {

            newPo.x = 15;

            newPo.y = -13;

        } else if (skillEnum == (int)SkillEnum.slowBullet) {

            newPo.x = 15;

            newPo.y = -13;

        } else if (skillEnum == (int)SkillEnum.fastBullet) {

            newPo.x = 15;

            newPo.y = -13;

        } else if (skillEnum == (int)SkillEnum.rayLight) {

            newPo.x = 15;

            newPo.y = -13;

        } else if (skillEnum == (int)SkillEnum.blockWall) {

            newPo.x = 60;

            newPo.y = 0;

        } else if (skillEnum == (int)SkillEnum.shadow) {

            newPo.x = 1150;

            newPo.y = 0;

        } else if (skillEnum == (int)SkillEnum.shield) {

            newPo.x = 0;

            newPo.y = -11;

        } 

        if (!roleState.isLeftAlly) {

            newPo.x = -newPo.x;

        }

        gameObject.transform.localPosition = new Vector2(skillPo.x + newPo.x, newPo.y);

    }

    public string getBid(string timeSample) {

        string originStr = "";

        originStr += timeSample;

        originStr += sid;

        MD5 md5 = new MD5CryptoServiceProvider();

        byte[] s = Encoding.UTF8.GetBytes(originStr);

        byte[] c = md5.ComputeHash(s);

        return Convert.ToBase64String(c);

    }

    // 客户端检测飞行与碰撞信息
    void onFlying() {

        if (skill.isBuff) return;

        gameObject.transform.Translate(Vector2.right * direct * skill.flySpeed * Time.deltaTime);

        RaycastHit rayHit;

        Vector3 skillPo = gameObject.transform.localPosition;

        Vector3 dir = (roleState.isLeftAlly) ? Vector3.right - skillPo : Vector3.left - skillPo;

        bool t = Physics.Raycast(skillPo, dir, out rayHit, 1920);

        if (t && isHit == false) {

            if (rayHit.collider.name != sid && rayHit.collider.tag == "Player") {

                GameObject oppoRole = rayHit.collider.gameObject;

                RoleScript rs = oppoRole.GetComponent<RoleScript>();

                if (rs.roleState.isDead) return;

                if (Vector3.Distance(rayHit.collider.transform.localPosition, skillPo) < 45f) {

                    isHit = true;

                    gameObject.transform.localPosition = new Vector3(99999, 99999, 99999);

                    if (rs.isMe) {

                        if (rs.roleState.isReflect == true) {

                            CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.ReflectBullet.ToString(), bid);

                            return;

                        }

                        if (rs.roleState.isImmune == true) {

                            CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.ImmuneBullet.ToString(), bid);

                            return;

                        }

                        if (rs.roleState.blockCount > 0) {

                            CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.KillBullet.ToString(), bid);

                            return;

                        }

                        if (rs.roleState.isBlocking == true) {

                            CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.BlockBullet.ToString(), bid);

                            return;

                        } else {

                            CuteUDPManager.cuteUDP.emitServer(BattleEventEnum.BeAttacked.ToString(), bid);

                            return;

                        }
                    }
                }

            } else if (rayHit.collider.name != sid && rayHit.collider.tag != "Player") {

                // 击中非玩家

            }
        }
    }

    // 服务器回传碰撞信息
    public void onCol(string _sid, BattleEventEnum colTypeEnum) {

        GameObject whoBeCol = GameObject.Find(_sid);

        if (whoBeCol == null) return;

        RoleScript rs = whoBeCol.GetComponent<RoleScript>();

        switch(colTypeEnum) {

            case BattleEventEnum.ReflectBullet:

                rs.castSkill(skillEnum, bid);

                // direct *= -1;

                // sid = _sid;

                // oppoSid = rs.oppoSid;

                // gameObject.tag = (tag == "RightSkill") ? "LeftSkill" : "RightSkill";

                Destroy(gameObject);

                break;

            case BattleEventEnum.ImmuneBullet:

                Destroy(gameObject);

                break;

            case BattleEventEnum.KillBullet:

                if (rs.roleState.blockCount > 0) {

                    rs.roleState.blockCount -= 1;

                } else {

                    rs.roleState.life -= skill.dmg;

                }

                Destroy(gameObject);

                break;

            case BattleEventEnum.BlockBullet:

                if (rs.roleState.blockLife > 0) {

                    rs.roleState.blockLife -= skill.dmg;

                } else {

                    rs.roleState.life -= skill.dmg;

                }

                Destroy(gameObject);

                break;

            case BattleEventEnum.BeAttacked:

                rs.roleState.life -= skill.dmg;

                Destroy(gameObject);

                break;

            default:

                break;
        }

    }

    void onBuffing() {

        if (!skill.isBuff) return;

        if (skill.buffLast > 0) {

            skill.buffLast -= Time.deltaTime;

            if (skill.id == (int)SkillEnum.block) {

                roleState.isBlocking = true;

                roleState.isReflect = true;

            }

            if (skill.id == (int)SkillEnum.shield) {

                roleState.isImmune = true;

            }

        } else {

            if (skill.id == (int)SkillEnum.block) {

                roleState.isReflect = false;

            }

            if (skill.id == (int)SkillEnum.shield) {

                roleState.isImmune = false;

            }

            Destroy(this.gameObject);

        }
    }

    void enermyLiveCheck() {

        GameObject enermyObj = GameObject.Find(oppoSid);

        if (enermyObj == null || oppoSid == "") return;

        RoleScript rs = enermyObj.GetComponent<RoleScript>();

        if (rs.roleState.life <= 0) {

            if (gameObject.transform.localPosition.x < -2560 || gameObject.transform.localPosition.x > 2560) {

                Destroy(gameObject);

            }
        }
    }

    // void OnTriggerEnter (Collider col) {

        // Debug.Log("out" + col.name);
        
    // }
}
