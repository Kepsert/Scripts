using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePropActor : CutsceneActor
{
    [SerializeField] Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void TriggerAnimation(string trigger)
    {
        Debug.Log(gameObject.name);
        anim.SetTrigger(trigger);
    }
}
