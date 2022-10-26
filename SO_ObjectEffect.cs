using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect",  menuName = "ScriptableObjects/Effects/Effect", order = 1)]
public class SO_ObjectEffect : ScriptableObject
{
    public string effectName;
    public GameObject effectPrefab;
    public Animator[] animEffect;
    public int indexEffect;

    public string EffectName
    {
        get { return effectName; }
    }

    public GameObject EffectPrefab
    {
        get { return effectPrefab; }    
    }

    public Animator[] AnimEffect
    {
        get { return animEffect; }  
    }

    public int IndexEffect
    {
        get { return indexEffect; }
    }
}
