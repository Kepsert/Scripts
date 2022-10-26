using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [SerializeField] List<Sprite> images;
    [SerializeField] List<string> names;
    [TextArea(3, 10)]
    [SerializeField] List<string> lines;
    [SerializeField] bool hasPlayed = false;

    public List<Sprite> Images
    {
        get { return images; }  
    }
    public List<string> Names
    {
        get { return names; }
    }

    public List<string> Lines
    {
        get { return lines; }
    }

    public bool HasPlayed
    {
        get { return hasPlayed; }
    }

    // Set has played to true if dialogue has been played so it won't replay again on player death/level restart
    public void SetHasPlayed(bool hasplayed)
    {
        hasPlayed = hasplayed;
    }
}
