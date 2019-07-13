class UserState {

    constructor() {

        this.username;

        this.ip;

        this.port;

    }
}

class RoleState extends UserState {
    
    constructor() {

        super();

        this.isLeftAlly = true;
        
        this.life = 5;
        
        this.blockLife = 3;
        
        this.damage = 1;
        
        this.shootGapOrigin = 1;
        
        this.shootGap = 0;
        
        this.blockGapOrigin = 0.5;
        
        this.blockGap = 0;
        
        this.perfectBlockGapOrigin = 0.2;
        
        this.perfectBlockGap = 0;
        
        this.moveSpeedOrigin = 2;
        
        this.moveSpeed = 2;
        
        this.shootSpeed = 4;
        
        this.isBlocking = false;
        
        this.isPerfectBlocking = false;
        
        this.isMoving = false;
        
        this.isReloading = false;
        
        this.isDead = false;

    }
}

module.exports = RoleState;