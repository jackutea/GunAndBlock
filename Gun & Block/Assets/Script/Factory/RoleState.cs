﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleState{
    public string username;
    public bool isLeftAlly = true;
    public int life = 5;
    public int blockLife = 3;
    public int damage = 1;
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