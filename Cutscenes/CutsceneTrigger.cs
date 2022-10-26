using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] CutsceneAction[] cutsceneActions;
    [SerializeField] bool loadNewScene = false;
    [SerializeField] int sceneID = 0;
    [SerializeField] CutsceneActor[] cutsceneActors;

    private bool hasPlayed = false;

    [SerializeField] bool PlayOnStart = false;

    private void Awake()
    {
        //hasPlayed = false;
        //// If the dialogue and/or cutscene have to play upon opening the scene
        //if (PlayOnStart && GameController.Instance.GetGameType() != GameType.Speedrun)
        //{
        //    GameController.Instance.SetGameState(GameState.Dialogue);
        //    TriggerDialogue();
        //}
    }

    private void Start()
    {
        hasPlayed = false;
        // If the dialogue and/or cutscene have to play upon opening the scene
        if (PlayOnStart && GameController.Instance.GetGameType() != GameType.Speedrun)
        {
            GameController.Instance.SetGameState(GameState.Dialogue);
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (!hasPlayed && GameController.Instance.GetGameType() != GameType.Speedrun)
        {
            if (PlayOnStart)
            {
                if (PlayerPrefs.HasKey("Cutscene" + SceneManager.GetActiveScene().buildIndex))
                {
                    Debug.Log("Test");
                    return;
                }
                PlayerPrefs.SetInt("Cutscene" + SceneManager.GetActiveScene().buildIndex, 1);
                PlayerPrefs.Save();
            }
            hasPlayed = true;
            // Start dialogue and/or cutscene
            StartCoroutine(CutsceneController.Instance.StartCutscene(cutsceneActions, cutsceneActors, loadNewScene, sceneID));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !PlayOnStart)
            TriggerDialogue();
    }

    // Struct with all types of cutscene possibilities.
    [System.Serializable]
    public struct CutsceneAction
    {
        public enum Speaker { Samantha, Subtitle, Prop01 }
        public enum Expression { Default, Worried, Happy }
        public string name;
        public Speaker speaker;
        public Expression expression;
        public string dialogue;
        [Space(10)]
        public bool triggerAnimation;
        public string animationName;
        [Space(10)]
        public bool doMove;
        public float moveDuration;
        public float moveSpeed;
        [Space(10)]
        public bool doFlip;
        public float waitTime;
        [Space(10)]
        public bool doChangeMusic;
        public string songName;
        [Space(10)]
        public bool playSFX;
        public string sfxName;
        public float sfxStartTime;
        [Space(10)]
        public float minimumActionDuration;
        [Space(10)]
        public bool showGameOver;
        public bool hideGameOver;
        public bool hideGameOverText;
        public bool triggerRetry;
        [Space(10)]
        public GameObject toggleActivityObject;
        public bool quitGame;
    }
}
