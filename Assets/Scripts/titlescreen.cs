using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class titlescreen : MonoBehaviour
{
    public GameObject p1c;
    public GameObject p2c;
    public TextMeshProUGUI pressA;
    Manager manager;
    public static bool started;
    public Cinemachine.CinemachineImpulseSource camimp;
    int pastno = 0;
    Coroutine startdelay;
    bool count, blink=false, joined=false;
    int framecount;
    // Start is called before the first frame update
    void Start()
    {
        p1c.SetActive(false);
        p2c.SetActive(false);
        manager = GameObject.Find("PlayerManager").GetComponent<Manager>();
        started = false;
        StartCoroutine(textBlink(0.5f, 400));
    }

    // Update is called once per frame
    void Update()
    {   
        if(joined) return;

        if (pastno != manager.number)
            camimp.GenerateImpulse(new Vector3(2, 0.1f, 0));
        if (manager.number == 1) p1c.SetActive(true);
        if (manager.number == 2)
        {
            p2c.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(textBlink(0.1f, 12));
            manager.pickStage(Random.value<0.5f?1:2);
            joined=true;
        }
        pastno = manager.number;

    }

    IEnumerator textBlink(float sec, int i){
        yield return new WaitForSeconds(sec);
        pressA.color=new Vector4(1,1,1,1f - ((blink)?0:0.5f));
        blink=!blink;
        if(i==0) {
            started = true;
            camimp.GenerateImpulse(new Vector3(1, 0.1f, 0));
            manager.clip.SetDirectAudioMute(0,false);
            hideObject();
            yield break;
        }
        StartCoroutine(textBlink(sec, (i==400?i:i-1)));
    }

    void hideObject(){
        for(int ij = 0; ij<transform.childCount; ij++){
            transform.GetChild(ij).gameObject.SetActive(false);
            
        }
        Destroy(pressA);
        StopAllCoroutines();

    }

    public void Continue(){
        StartCoroutine(textBlink(0.5f, 400));
    }

}
    
