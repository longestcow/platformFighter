using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class Manager : MonoBehaviour
{
    public GameObject bf; //reference of battlefield stage
    public GameObject fd; //reference of final destination stage
    public VideoPlayer clip; // this variables value will be set after a stage has been chosen
    public bool over=false; // i dont remember why this is here but im not going to remove it in case it's actually getting used elsewhere
    public int number = 0; // number of players
    // Start is called before the first frame update
    public void onPlayerJoined() {
        ++number; //increment index when a player joins
        print("Player Joined");
    }

    public void pickStage(int i) { //pick random stage
        bf.SetActive(i==1);
        fd.SetActive(i!=1);
        clip=GameObject.Find((i==1?"bgClipBF":"bgClipFD")).GetComponent<VideoPlayer>();
        
    }
}
