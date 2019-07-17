using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoleState{
    public string username = "";
    public string roleName = "";
    public int inServerId = -1;
    public int level = 1;
    public int exp = 0;
    public int rank = 0;
    public int score = 0;
    public bool isLeftAlly = true;
    public float life = 5;
    public float blockLife = 3;
    public float damage = 1;
    public float shootGapOrigin = 1f;
    public float shootGap = 0;
    public float blockGapOrigin = 0.5f;
    public float blockGap = 0;
    public float perfectBlockGapOrigin = 0.2f;
    public float perfectBlockGap = 0;
    public float moveSpeedOrigin = 2f;
    public float moveSpeed = 2f;
    public float shootSpeed = 4f;
    public bool isBlocking = false;
    public bool isPerfectBlocking = false;
    public bool isMoving = false;
    public bool isReloading = false;
    public bool isDead = false;
}