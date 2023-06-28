using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallThrough : MonoBehaviour
{
    public Player script;
    void OnTriggerEnter2D(Collider2D other)
    {        
        if(other.gameObject.name!="detect")
            return;
        if((other.gameObject.transform.parent.gameObject.name=="Ground") || !script.pressedDown) 
            script.standCol.enabled=true;
        
    
            
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.name!="detect")
            return;
        script.standCol.enabled=false;
        
    }
}
