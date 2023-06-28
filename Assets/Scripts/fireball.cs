using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireball : MonoBehaviour
{
    // Start is called before the first frame update
    int a = 1;
    void Start()
    {
        if(transform.rotation.z==-180){
            a=-1;
            print("a");
        }
    }
    void Update() {
        if(a==1)
            transform.position += new Vector3(0.1f,0,0);
        else
            transform.position -= new Vector3(0.1f,0,0);



    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name.StartsWith("death") || other.name.StartsWith("Floor"))
            Destroy(gameObject);
    }


}
