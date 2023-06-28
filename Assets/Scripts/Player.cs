using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using TMPro;

public class Player : MonoBehaviour
{
    public int stock = 3, dmg=0, dealDmg=1, angle=20;
    public float dealBKB=1.6f, dealVM, dealHM; // all of this stuff is to be changed using SetAttack() later on
    public GameObject fireBall; // reference of fireball prefab
    int id; // id of the player instance
    Animator tsAnim; // animator for the title screen
    public LayerMask floorLayerMask, groundLayer; 
    public bool pressedDown=false;
    //colliders and references
    public BoxCollider2D feet; 
    public BoxCollider2D standCol;
    public BoxCollider2D hitboxCol;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Animator anim;
    GameObject respawn;
    public Camera cam;
    public CinemachineImpulseSource camShake; // this is used for camera shakes
    GameObject titlescren; // reference of titlescreen object
    public Player opponent; //reference of the opponent player script
    
    Vector2 movement = new Vector2(0,0); // main movement object that will be used on eveyr frame
    bool a,b,x,y; // booleans to see if a certain button is pressed
    float ssb; //knockback value 
    int hsframes; //hitStun frames count

    Manager manager; //reference of manager script

    public int action;
     /* 0 = idle
     * 1 = run
     * 2 = jump
     * 3 = double jump
     * 4 = jab
     * 5 = ftilt
     * 6 = dtilt
     * 7 = utilt
     * 8 = aerial
     * 9 = uspec recovery
     * 10 = nspec fireball
     * 11 = dspec increase damage
     * 12 = fspec dash zoom
     * 13 = fall
     * I ended up not using almost all of these
     */
    public bool right = true; //true if the character is facing right
    bool stun; //automatically gets enabled if stuncounter > 0
    int stuncounter; //goes down by 1 every frame after which you're unstunned
    bool grounded, buttonUp = false, wasButtonUp=false; 
    bool fastFall=false; 
    int wasAction; // what action was last frame
    bool dead; // set to true when player dies (gets set to false after a few seconds)
    bool hitStun; 
    TextMeshProUGUI dmgText, stockText; // references of the texts
    BoxCollider2D hurtBox; // the collider which hurts other players

    //movement
    bool hasDoubleJump = true;
    public float moveSpeed=7f;
    public float longHop=4f;
    public float shortHop=5f;
    public float jumpVelocity = 10;
    public float fallOff = 3;
    CinemachineTargetGroup targetGroup;
    

    // Start is called before the first frame update
    void Start()
    {
        titlescren=GameObject.Find("Titlescren");
        tsAnim=GameObject.Find("Titlescren").GetComponent<Animator>();
        camShake=GameObject.Find("background").GetComponent<CinemachineImpulseSource>();
        respawn=GameObject.Find("respawn");
        action = 0;
        stun = false;
        stuncounter = 0;
        manager = GameObject.Find("PlayerManager").GetComponent<Manager>();
        id=manager.number; // set index
        sprite.color=(id==1)?new Vector4(1f,57f/255f,56f/255f,1f):(id==2)?new Vector4(48f/255f,138f/255f,1f,1f):new Vector4(41f/255f,181f/255f,72f/255f,1f); //set colour of sprite based on index (r,b,g)
        
        if(id!=1){
            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Player")) 
                if(!obj.Equals(gameObject))
                    Physics2D.IgnoreCollision(standCol, obj.GetComponent<Player>().standCol, true); //ignore collision with the other player so you can pass each other
                
        }

        dmgText=GameObject.Find(("dmgperc"+id.ToString())).GetComponent<TextMeshProUGUI>(); //HUD damage % text
        dmgText.text="<size=100%>"+dmg.ToString()+"<size=50%>%";
        stockText=GameObject.Find("stocknum"+id.ToString()).GetComponent<TextMeshProUGUI>();//HUD stock % text

        GameObject.Find("frame"+id.ToString()).GetComponent<SpriteRenderer>().color=sprite.color;

        gameObject.transform.position=(id==1)?new Vector3(-4.73f,-1.15f,1f):(id==2)?new Vector3(3.88f,-1.16f,1):new Vector3(-0.32f,-1.15f,1); // set position based on index
        if(id==2) {
            gameObject.transform.localScale=new Vector3(-0.75f, transform.localScale.y, transform.localScale.z); // turn 2nd player around
            right = false;
        }
        hurtBox=sprite.gameObject.GetComponent<BoxCollider2D>();

        
        
    }

    void AllConnected() // this function is called when both players have joined the game and are connected
    {
        targetGroup = GameObject.Find("targetGroup").GetComponent<CinemachineTargetGroup>(); // add both players to target group so that the camera follows both players and keeps them in view
        Cinemachine.CinemachineTargetGroup.Target target;
        target.target = gameObject.transform;
        target.weight = 1;
        target.radius = 5;

        for (int i = 0; i < targetGroup.m_Targets.Length; i++) {
            if (targetGroup.m_Targets[i].target == null) {
                targetGroup.m_Targets.SetValue(target, i); // look for the first null spot and replace it with this gameObject
                break;
            }
        }

        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Player")){
            if(!obj.Equals(gameObject))
                opponent=obj.GetComponent<Player>(); // set reference of opponent
        }

    }

    void Update() {
        if(!buttonUp)
            buttonUp=((wasButtonUp && (!x && !y))); // this is a workAround for the buggy ButtonDown in FixedUpdate()
        if (titlescreen.started) AllConnected();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(dead||hitStun) return; // you shouldnt be able to do anything if youre either dead or hitstunned


        grounded = ((feet.IsTouchingLayers(floorLayerMask)) || feet.IsTouchingLayers(groundLayer)); //checks if the feet collider is touching a platform
        if(grounded && anim.GetCurrentAnimatorStateInfo(0).IsName("fall")){
            anim.SetTrigger("idle"); action=0; //idle state
            if(hurtBox.enabled) hurtBox.enabled=false;
        }
        else if(!grounded && (action!=8) && !anim.GetCurrentAnimatorStateInfo(0).IsName("jump") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime>0.1){
            anim.SetTrigger("fall"); action=13; // in air and thus in falling state
            if(hurtBox.enabled) hurtBox.enabled=false;
        }

        //check direction and change rotation
        if (movement.x > 0 && !right) {
            gameObject.transform.localScale=new Vector3(0.75f, transform.localScale.y, transform.localScale.z);
            right = true;  
        }
        else if (movement.x < 0 && right) {
            gameObject.transform.localScale=new Vector3(-0.75f, transform.localScale.y, transform.localScale.z);
            right = false;
        }

        if(anim.GetCurrentAnimatorStateInfo(0).IsName("jump") && grounded && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1){
            anim.SetTrigger("idle"); stopped moving after jump
        }


        //check stun

        if (stun) {
            if(action==8)
                rb.velocity = new Vector2(movement.x*(moveSpeed/1.5f), rb.velocity.y); // if youre doing the aerial attack then you can move around in half the speed
            else
                rb.velocity = new Vector2(0,0);
            stuncounter--;
            if (stuncounter <= 0) stun = false;
            return;
        }
        else if (stuncounter > 0) stun = true;

        //action = 0;
        if (grounded && movement.x != 0) action = 1;
        else if (grounded) action = 0;
        

        // movement ifs
        if (b) { // if b is pressed
            if(grounded)
                jab();
            else 
                aerial();
        }
        else if(a){ //if a is pressed
            projectile(); // this doesnt work properly (doesnt rotate well, and isnt detected by opponents hitbox)
        }

        if (grounded && !hasDoubleJump) hasDoubleJump = true;

        if(canJump()){
            rb.velocity=Vector2.up*jumpVelocity; //jump which is basically just adding velocity on the y axis
            anim.SetTrigger("jump"); //start jump anim
        }
        endJump(); // decrease velocity if the player has reached a certain height

        //running
        rb.velocity = new Vector2(movement.x*moveSpeed, rb.velocity.y);

        //----------run anim
        if(movement.x!=0 && grounded){
            if(action==1 && wasAction!=1)
                anim.SetTrigger("run");
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("run")){
            if(!grounded)
                anim.SetTrigger("fall"); 
            else          
                anim.SetTrigger("idle");
        }
        
        //if the stick is pushed down
        if (movement.y < -0.8) {
            pressedDown=true;
            if(!feet.IsTouchingLayers(groundLayer)){
                feet.enabled=false;
                feet.enabled=true;
            }
            fastFall=true;
        }
        if(fastFall) { //pressedDown
            rb.AddForce(-Vector2.up * 2f, ForceMode2D.Impulse);
            if(grounded || action==3)
                fastFall=false;
        }
        else pressedDown=false;


        wasAction=action; //what action was last frame
        wasButtonUp=(x||y); //was up button clicked last frame
    }

    //checks to see if the player *wants* to jump, and if they *can* jump and returns true if both are true
    bool canJump() {
        if ((x || y) && buttonUp && (hasDoubleJump || anim.GetCurrentAnimatorStateInfo(0).IsName("idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("run"))) {
            buttonUp=false;
            if ((grounded || hasDoubleJump)) {
                action = 2;
                if(!grounded){hasDoubleJump=false; action=3; }
                return true; 
            }
            return false;
        }
        return false;
    }

    void endJump(){
        if(rb.velocity.y < fallOff) { //jump end big
            rb.velocity += (Vector2.up * Physics2D.gravity.y * longHop  * Time.deltaTime); //bring player down
        }
        else if(rb.velocity.y > 0 && !(x || y)) { //jump end small
             rb.velocity += (Vector2.up * Physics2D.gravity.y * shortHop * Time.deltaTime); // this was supposed to be a shorthop but it was kinda annoying while playing around. I'll assign this to another button later on
        }
    }

    void jab() { // set attack values (the variables at the top) to work with punch, aerial, and projectile (which doesnt work)
        action = 4;
        setAttack(2, 2f, 20, 20, 5);
        anim.SetTrigger("punch");
        stuncounter = 17;
    }

    void aerial() { 
        action=8;
        setAttack(2,1,20,1.1f,0f);
        anim.SetTrigger("aerial");
        stuncounter=30;
    }

    void projectile(){
        action = 10;
        setAttack(6,0.1f,20,3,0);
        anim.SetTrigger("projectile");
        stuncounter=10;
        StartCoroutine(spawnFireball());
    }



    //these functions are called when input is received
    public void onMove(InputAction.CallbackContext ctx) => movement=ctx.ReadValue<Vector2>();
    public void onA(InputAction.CallbackContext ctx)=> a=ctx.ReadValueAsButton();
    public void onB(InputAction.CallbackContext ctx)=> b=ctx.ReadValueAsButton();
    public void onX(InputAction.CallbackContext ctx)=> x=ctx.ReadValueAsButton();
    public void onY(InputAction.CallbackContext ctx)=> y=ctx.ReadValueAsButton();


    public void kill(){ // function is called when player collides with a death boundary aka an explosion zone
        //cam shake        
        stock--;
        stockText.text=stock.ToString();
        camShake.GenerateImpulse(new Vector3(2, 0.1f, 0)); //cam shake

        dead=true; // turn player invis and slowly bring them back to stage so that the camera doesnt snap
        rb.gravityScale=0;
        rb.velocity=Vector2.zero;
        feet.enabled=false;
        standCol.enabled=false;
        hitboxCol.enabled=false;
        sprite.enabled=false;

        if(stock==0) { // this means that the opponent won
            endGame();
            return;
        }
        dmg=0;
        dmgText.text="<size=100%>"+dmg.ToString()+"<size=50%>%";
        StartCoroutine(transition(0));
    }

    IEnumerator transition(int i) { // bring player back to stage slowly so that the camera doesnt snap
        if(i==0)
            yield return new WaitForSeconds(0.2f);
        transform.position=Vector3.MoveTowards(transform.position, respawn.transform.position, 65*Time.deltaTime);
        yield return new WaitForSeconds(0.01f);
        if(transform.position==respawn.transform.position){
            yield return new WaitForSeconds(0.3f);
            rb.gravityScale=1;
            rb.velocity=Vector2.zero;
            dead=false;
            feet.enabled=true;
            hitboxCol.enabled=true;
            standCol.enabled=true;
            sprite.enabled=true;
        }
        else
            StartCoroutine(transition(i+1));
    }

    public void hurt(int dmgDealt, float bkbDealt, float hmDealt, float vmDealt, float xang, bool side){ //this function is called when another players hurtBox collides with this players hitbox, hurting the current player
        hurtBox.enabled=false;
        dmg+=dmgDealt;
        gameObject.transform.localScale=new Vector3(0.75f * ((side)?-1:1), transform.localScale.y, transform.localScale.z);
        right=!side;
        anim.SetTrigger("hurt");
        dmgText.text="<size=100%>"+dmg.ToString()+"<size=50%>%";
        ssb=getSSBKnockback(dmg, dmgDealt, 100, 1, bkbDealt, 1); // get knockback value based on damage %, damage dealth, knockback dealth, etc. 
        hitStun=true;
        StartCoroutine(disableStun(hsframes=(int)(Mathf.Floor(ssb*0.4f)*1.5f))); //decide the number of frames the player will be hitstunned for
        float hAngle = Mathf.Cos(angle * Mathf.Deg2Rad); // this just converts degrees to radians and gets the x value of the angle 
        float vAngle = Mathf.Sin(angle * Mathf.Deg2Rad); 
        float hVelocity = ssb*0.04f*hmDealt*(hAngle * ((side)?1:-1)); hVelocity= Mathf.Round(hVelocity*100_000)/100_000; //calculate the horizontal velocity and round it to an integer
        float vVelocity = ssb*0.04f*vmDealt*vAngle; vVelocity= Mathf.Round(vVelocity*100_000)/100_000; //calculate vertical velocity and round it to an integer
        rb.velocity=(new Vector2(hVelocity, vVelocity)); // apply the new velocity
        
    } 

    IEnumerator disableStun(int i){ // this disables the hitstun after the specified number of frames have passed
        if(i!=0){
            yield return new WaitForFixedUpdate();
            Vector3 velocity = rb.velocity;
            velocity.x = velocity.x / (0.5f + 1f); 
            velocity.y = velocity.y - (0.2f);
            rb.velocity = velocity;

            StartCoroutine(disableStun(i-1));
        }
        else{
            hitStun=false;
            anim.SetTrigger("idle");
        }
    }

    public float getSSBKnockback(float p, float d, float w, float s, float b, float r){ //found this formula on the SSB wiki
        return (float) ((((((((p/10)+((p*d)/20))*(200/(w+100)))*1.4)+18)*s)+b)*r);
    }


    void setAttack(int dd, float dkb, int ang, float hm, float vm){ //sets attack values
        dealDmg=dd;
        dealBKB=dkb;
        angle=ang;
        dealHM=hm;
        dealVM=vm;
    }
    
    void endGame(){ // stop the bg video, play the win screen aniamtion after 0.5s, shake camera
        manager.over=true;
        manager.clip.playbackSpeed=0;        
        manager.clip.SetDirectAudioMute(0,true);
        StartCoroutine(playerWinScreen());
        camShake.GenerateImpulse(new Vector3(5.5f, 0.5f, 0)); //cam shake

    }

    IEnumerator playerWinScreen() { // plays the win animation
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i<titlescren.transform.childCount; i++){
            titlescren.transform.GetChild(i).gameObject.SetActive(true);
        }
        GameObject.Find("player wins").GetComponent<TextMeshProUGUI>().text = "PLAYER " +((id==1)?"2 ":"1 ") + "WINS";

        if(id==1)
            tsAnim.SetTrigger("player2");
        else if(id==2)
            tsAnim.SetTrigger("player1");
    }

    IEnumerator spawnFireball(){ // this is buggy and non-functional but it is supposed to spawn a fireball prefab which goes forward towards whiever direction youre facing
        yield return new WaitForSeconds(0.38f);
        GameObject ball = Instantiate(fireBall, transform.position+new Vector3(0.5f,-0.1f,0), Quaternion.Euler(new Vector3(0,0,(right?0:180))));
        ball.GetComponentInChildren<SpriteRenderer>().color=sprite.color;
    }


}
