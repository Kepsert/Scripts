using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Objectpoolmanager for optimization
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] GameObject go_ObjectPoolGroup;

    // Add all effects in the inspector
    [Header("Effects Animation")]
    [SerializeField] List<SO_ObjectEffect> effectList;
    // Create a list of names in the same order as the effect list to easily find index of an effect based on name.
    List<string> effectNameList;

    int i_poolSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance);
        }

        // Initialize list
        effectNameList = new List<string>();
    }

    private void Start()
    {
        i_poolSize = 20;

        // Go through all of the effects in the list of effects, add the name to the name list, and instantiate an initial object of the effect.
        foreach (SO_ObjectEffect effect in effectList)
        {
            effectNameList.Add(effect.name);

            if (effect.effectPrefab != null)
            {
                effect.animEffect = new Animator[i_poolSize];
                for (int i = 0; i < i_poolSize; i++)
                {
                    effect.animEffect[i] = Instantiate(effect.effectPrefab, Vector3.down * 10000, Quaternion.identity, go_ObjectPoolGroup.transform).GetComponent<Animator>();
                }
                effect.indexEffect = 0;
            }
        }
    }


    // Call this method through the singleton pattern to play effect of name, at position v_pos in direction i_dir
    public void PlayEffect(string name, Vector3 v_pos, int i_dir)
    {
        // See if the effectname is in the effectList
        int index = effectNameList.IndexOf(name);

        SO_ObjectEffect temp = effectList[index];

        // See if this scriptable object has a prefab applied to it.
        if (temp.effectPrefab == null) return;

        // Double check to see if the requested name and IndexOf correspond
        if (name == temp.effectName)
        {
            temp.indexEffect++;
            if (temp.indexEffect >= i_poolSize)
            {
                temp.indexEffect = 0;
            }

            temp.animEffect[temp.indexEffect].SetTrigger("Play");

            if (i_dir > 0)
            {
                temp.animEffect[temp.indexEffect].transform.localScale = new Vector3(1, 1, 1);
                temp.animEffect[temp.indexEffect].transform.position = v_pos;
            }
            if (i_dir < 0)
            {
                temp.animEffect[temp.indexEffect].transform.localScale = new Vector3(-1, 1, 1);
                temp.animEffect[temp.indexEffect].transform.position = v_pos;
            }
        }
        else
        {
            Debug.Log("The requested index's name does not match the requested effect name.");
        }
    }
}
