using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance { get; private set; }

    [SerializeField] GameObject dialogueBox;
    [SerializeField] GameObject dialogueBoxNextArrow;
    [SerializeField] CutsceneActor[] cutsceneActors;
    [SerializeField] Animator canvasFadeAnim;

    [SerializeField] GameObject subtitleBox;
    [SerializeField] GameObject subtitleBoxNextArrow;

    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI dialogueText;
    public TMPro.TextMeshProUGUI subtitleText;
    //public Image portrait;
    [SerializeField] Animator portraitAnimator;
    [SerializeField] Animator portraitAnimatorMouth;
    [SerializeField] Animator portraitAnimatorBlinks;

    public Animator anim;

    //private List<string> sentences;
    private int currentLine;
    private CutsceneTrigger.CutsceneAction[] cutsceneActions;
    private bool isCutsceneActive;

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    [SerializeField] GameObject DialoguePanel;

    float actionTimer;

    bool loadNewScene;
    int sceneID;

    bool hasSFXPlayed;

    public void Awake()
    {
        //Initialize singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(this);
        }

        dialogueBoxNextArrow.SetActive(false);
        subtitleBoxNextArrow.SetActive(false);
    }

    private void Update()
    {
        //Check to see if a cutscene is currently playing
        if (isCutsceneActive)
        {
            actionTimer += Time.deltaTime;

            if (cutsceneActions[currentLine].doMove)
            {
                if (actionTimer > cutsceneActions[currentLine].moveDuration)
                    cutsceneActors[(int)cutsceneActions[currentLine].speaker].SetVelocity(0);
            }

            //SFX - Uncomment when implementing SFX
            if (currentLine < cutsceneActions.Length)
                if (cutsceneActions[currentLine].playSFX)
                {
                    if (actionTimer > cutsceneActions[currentLine].sfxStartTime && !hasSFXPlayed)
                    {
                        Debug.Log("Do I call: " + cutsceneActions[currentLine].sfxName);
                        MusicPlayer.Instance.PlaySoundEffectByName(cutsceneActions[currentLine].sfxName);
                        hasSFXPlayed = true;
                    }
                }

            //Handle actions that have no text
            string dialogueLine = cutsceneActions[currentLine].dialogue;
            if (dialogueLine == "" && actionTimer > cutsceneActions[currentLine].minimumActionDuration
                && actionTimer > cutsceneActions[currentLine].moveDuration && actionTimer > cutsceneActions[currentLine].waitTime)
                ProgressCutscene();

            //mouth flaps
            if (cutsceneActions[currentLine].dialogue != dialogueText.text)
            {
                if (portraitAnimatorMouth != null)
                    portraitAnimatorMouth.SetBool("IsTalking", true);
                dialogueBoxNextArrow.SetActive(false);
                subtitleBoxNextArrow.SetActive(false);
            }
            // Move on to the next line if the current line has completed
            else
            {
                if (portraitAnimatorMouth != null)
                    portraitAnimatorMouth.SetBool("IsTalking", false);
                dialogueBoxNextArrow.SetActive(true);
                subtitleBoxNextArrow.SetActive(true);
            }
        }
    }

    public void ContinueCall()
    {
        if (GameController.Instance.GetGameState() == GameState.Dialogue && isCutsceneActive)
        {
            // Can only go to next line once actions are complete - if a cutscene actor is moving during a line of dialogue for example, wait for the actor to finish moving
            if (actionTimer < cutsceneActions[currentLine].minimumActionDuration
                    || actionTimer < cutsceneActions[currentLine].moveDuration) return;


            if (isCutsceneActive
                    && !cutsceneActions[currentLine].quitGame)// && !dialogue.HasPlayed)
            {
                // Immediately put all text of current dialogue line in the textbox
                if (cutsceneActions[currentLine].dialogue != dialogueText.text)
                {
                    StopAllCoroutines();
                    dialogueText.text = cutsceneActions[currentLine].dialogue;
                    subtitleText.text = cutsceneActions[currentLine].dialogue;

                    // stop animations from playing
                    if (portraitAnimator != null)
                    {
                        //portraitAnimator.SetLayerWeight(1, 0);
                    }
                }
                // Move on to the next line if the current line has completed
                else
                {
                    MusicPlayer.Instance.PlaySoundEffectByName("UI_Confirm"); 
                    ProgressCutscene();
                }
            }
        }
    }

    // Player input to go through dialogue
    public void Continue(InputAction.CallbackContext context)
    {
        if (GameController.Instance.GetGameState() == GameState.Dialogue && isCutsceneActive)
        {
            // Can only go to next line once actions are complete - if a cutscene actor is moving during a line of dialogue for example, wait for the actor to finish moving
            if (actionTimer < cutsceneActions[currentLine].minimumActionDuration
                    || actionTimer < cutsceneActions[currentLine].moveDuration) return;


            if (context.performed && isCutsceneActive
                    && !cutsceneActions[currentLine].quitGame)// && !dialogue.HasPlayed)
            {
                // Immediately put all text of current dialogue line in the textbox
                if (cutsceneActions[currentLine].dialogue != dialogueText.text)
                {
                    StopAllCoroutines();
                    dialogueText.text = cutsceneActions[currentLine].dialogue;
                    subtitleText.text = cutsceneActions[currentLine].dialogue;

                    // Stop the portraits from animating
                    if (portraitAnimator != null)
                    {
                        portraitAnimator.SetLayerWeight(1, 0);
                    }
                }
                // Move on to the next line if the current line has completed
                else
                {
                    MusicPlayer.Instance.PlaySoundEffectByName("UI_Confirm");
                    ProgressCutscene();
                }
            }
        }
    }

    public void UpdateCutsceneActors(CutsceneActor[] cutsceneActors)
    {
        this.cutsceneActors = cutsceneActors;
    }

    public void UpdateCanvasFadeAnimator(Animator anim)
    {
        canvasFadeAnim = anim;
    }

    private void ProgressCutscene()
    {
        // Move on to the next line/action, if there are no more actions in this dialogue, either go to the next scene or simply end the cutscene.
        currentLine++;
        if (currentLine < cutsceneActions.Length)//dialogue.Lines.Count)
        {
            InitiateNextAction();
        }
        else
        {
            isCutsceneActive = false;

            // If we are loading a new scene, handle that
            if (loadNewScene)
            {
                // Fade out
                canvasFadeAnim.SetTrigger("FadeOut");

                // Invoke scene load so fade out can happen
                Invoke("LoadNewScene", 1.2f);
            }
            else
            {
                // Otherwise end scene and go to gameplay
                CloseDialogueBox();
                // Delay the cutscene end to give time for the dialogue to hide and prevent jumping on close
                Invoke("EndDialogue", 0.5f);
                //var HUDAnim = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Animator>();
                //HUDAnim.SetBool("isOpen", true);
                currentLine = 0;
            }
        }
    }

    public IEnumerator StartCutscene(CutsceneTrigger.CutsceneAction[] actions, CutsceneActor[] actors, bool loadNewScene_i, int sceneID_i)
    {
        yield return new WaitForEndOfFrame();

        loadNewScene = loadNewScene_i;
        sceneID = sceneID_i;

        //cutsceneActors = CutsceneActorRetrieval.Instance.CutsceneActors;
        cutsceneActors = actors;

        if (actions != null)
        {
            if (actions.Length > 0)
            {
                GameController.Instance.SetGameState(GameState.Dialogue);

                cutsceneActions = actions;
                currentLine = 0;
                InitiateNextAction();

                isCutsceneActive = true;
            }
        }

        //stop actors from moving
        cutsceneActors[0].SetVelocity(0);
    }

    public IEnumerator TypeDialogue(string dialogue)
    {
        dialogueText.text = "";
        subtitleText.text = "";
        // Type dialogueline into the dialogue box one character at a time
        foreach (var letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            subtitleText.text += letter;

            // Different delay for certain characters to make it feel less static
            if (letter == '.' || letter == '!' || letter == '?')
                yield return new WaitForSeconds(0.4f);
            if (letter == '-' || letter == '–')
                yield return new WaitForSeconds(0.3f);
            if (letter == ',')
                yield return new WaitForSeconds(0.2f);
            else
                yield return new WaitForSeconds(0.02f);
        }

        // Stop portrait from animating
        if (dialogueText.text == dialogue)
        {
            if (portraitAnimator != null)
            {
                portraitAnimator.SetLayerWeight(1, 0);
            }
        }
    }

    void InitiateNextAction()
    {
        // Reset timer
        actionTimer = 0;

        // Depending on whether there's text in the current cutscene/dialogue line, open or close the dialoguebox
        string dialogueLine = cutsceneActions[currentLine].dialogue;
        if (dialogueLine == "" || cutsceneActions[currentLine].speaker.ToString() == "Subtitle")
            anim.SetBool("IsOpen", false);
        else
            anim.SetBool("IsOpen", true);

        if (cutsceneActions[currentLine].speaker.ToString() == "Subtitle") subtitleBox.SetActive(true);
        else subtitleBox.SetActive(false);

        // Add portrait/expression
        if (dialogueLine != "")
        {
            nameText.text = cutsceneActions[currentLine].speaker.ToString();
            if (currentLine > 0) // Cancel previous trigger in case player mashes dialogue and two triggers fire at a similar time
            {
                if (portraitAnimator != null)
                {
                    portraitAnimator.ResetTrigger(nameText.text + cutsceneActions[currentLine - 1].expression.ToString());
                }
                if (portraitAnimatorBlinks != null)
                {
                    portraitAnimatorBlinks.SetTrigger(nameText.text + cutsceneActions[currentLine - 1].expression.ToString());
                }
            }
            if (portraitAnimator != null)
            {
                portraitAnimator.SetTrigger(nameText.text + cutsceneActions[currentLine].expression.ToString());
            }
            if (portraitAnimatorBlinks != null)
            {
                portraitAnimatorBlinks.SetTrigger(nameText.text + cutsceneActions[currentLine].expression.ToString());
            }
            StartCoroutine(TypeDialogue(dialogueLine));
            /// Animate portrait
            if (portraitAnimator != null)
            {
                //portraitAnimator.SetLayerWeight(1, 1);
            }
        }

        // Next sound
        /*if (currentLine > 0)
        {
            if (cutsceneActions[currentLine - 1].dialogue != "")
                MusicPlayer.Instance.PlaySoundEffectByName("DialogueNext");
        }*/

        // Animations
        if (cutsceneActions[currentLine].triggerAnimation && cutsceneActions.Length != 0)
        {
            cutsceneActors[(int)cutsceneActions[currentLine].speaker].TriggerAnimation(cutsceneActions[currentLine].animationName);
        }

        // Movement - in case the cutscene line requires an actor to move
        if (cutsceneActions[currentLine].doMove)
        {
            cutsceneActors[(int)cutsceneActions[currentLine].speaker].SetVelocity(cutsceneActions[currentLine].moveSpeed);
        }

        if (cutsceneActions[currentLine].doFlip)
        {
            cutsceneActors[(int)cutsceneActions[currentLine].speaker].SetScale();
        }

        // Toggle object activity if there is one
        if (cutsceneActions[currentLine].toggleActivityObject != null)
        {
            cutsceneActions[currentLine].toggleActivityObject.SetActive(!cutsceneActions[currentLine].toggleActivityObject.activeSelf);
        }

        // Music - change soundtrack if required
        if (cutsceneActions[currentLine].doChangeMusic)
        {
            Debug.Log("Song name: " + cutsceneActions[currentLine].songName);
            MusicPlayer.Instance.ChangeBgMusic(cutsceneActions[currentLine].songName, true);
        }

        // SFX
        hasSFXPlayed = false;
    }

    void CloseDialogueBox()
    {
        anim.SetBool("IsOpen", false);
        subtitleBox.SetActive(false);
    }

    void EndDialogue()
    {
        GameController.Instance.SetGameState(GameState.Play);
    }

    void LoadNewScene()
    {
        GameController.Instance.LoadNextScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void DialogueEnded()
    {
        OnCloseDialogue.Invoke();
    }
}
