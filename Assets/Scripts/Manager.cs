using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class Manager : MonoBehaviour
{
    public GameObject bf;
    public GameObject fd;
    public VideoPlayer clip;
    public bool over=false;
    public int number = 0;
    // Start is called before the first frame update
    public void onPlayerJoined() {
        ++number;
        print("Player Joined");
    }

    public void pickStage(int i) {
        bf.SetActive(i==1);
        fd.SetActive(i!=1);
        clip=GameObject.Find((i==1?"bgClipBF":"bgClipFD")).GetComponent<VideoPlayer>();
        
    }
}
