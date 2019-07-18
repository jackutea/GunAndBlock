class RoleState {
    
    constructor() {

        this.username = "";

        this.roleName = "";

        this.inServerId = -1;

        this.level = 1;

        this.exp = 0;

        this.rank = 0;

        this.score = 0;

        this.isLeftAlly = true;

        this.isComparing = false;
        
        this.life = 5.0;
        
        this.blockLife = 3.0;
        
        this.damage = 1.0;
        
        this.shootGapOrigin = 1.0;
        
        this.shootGap = 0.0;
        
        this.blockGapOrigin = 0.5;
        
        this.blockGap = 0.0;
        
        this.perfectBlockGapOrigin = 0.2;
        
        this.perfectBlockGap = 0.0;
        
        this.moveSpeedOrigin = 2.0;
        
        this.moveSpeed = 2.0;
        
        this.shootSpeed = 4.0;
        
        this.isBlocking = false;
        
        this.isPerfectBlocking = false;
        
        this.isMoving = false;
        
        this.isReloading = false;
        
        // this.isDead = false; 

    }
}

module.exports = RoleState;