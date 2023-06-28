using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallThrough : MonoBehaviour
{
    public Player script; //get reference of player script
    void OnTriggerEnter2D(Collider2D other)
    {        
        if(other.gameObject.name!="detect") //this is the name of the invisible collider i have above every platform
            return;
        if((other.gameObject.transform.parent.gameObject.name=="Ground") || !script.pressedDown) //avoid falling through the main ground platform
            script.standCol.enabled=true;
        
    
            
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.name!="detect")
            return;
        script.standCol.enabled=false; //turn collider off if not standing on any platform
        
    }
}
