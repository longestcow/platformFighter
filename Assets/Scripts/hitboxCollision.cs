using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class hitboxCollision : MonoBehaviour
{
    Player player, opp;
    Vector2 direction;
    public BoxCollider2D attack;
    // Start is called before the first frame update
    void Start() {
        player = GetComponentInParent<Player>(); //get player script reference
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.name.Length>=5 && other.gameObject.name.Substring(0, 5)=="death"){
            player.kill(); // if player collides with explosion zone, kill player
        }
        try{       
        if(!other.transform.parent.gameObject.Equals(transform.parent.gameObject) && (other.gameObject.name=="sprite" || other.gameObject.name.StartsWith("fire"))){ // if this returns true, then something that is meant to hurt the player has collided with it
            float ang = (Vector2.Angle(other.transform.position, player.gameObject.transform.position)*180)/Mathf.PI;
            direction=direction.normalized;
            
            if(other.gameObject.name.StartsWith("fire")){//doesnt detect collision with fireball for some reason
                opp=player.opponent;
                Destroy(other.gameObject);
            }
            else
                opp=other.GetComponentInParent<Player>();

            player.hurt(opp.dealDmg, opp.dealBKB, opp.dealHM, opp.dealVM, ang, opp.right); //call the hurt function on the player script
        } 
        } catch(Exception e){e.GetBaseException();}
    }
}
