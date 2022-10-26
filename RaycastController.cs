using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastController : MonoBehaviour
{
    [SerializeField] GameObject raycastOrigin;
    [SerializeField] float raycastRange;
    [SerializeField] LayerMask IgnoreTrigger;

    [Header("Detection Ray Casts"), Space(10)]

    [SerializeField] Vector2 LeftConeEdgeRayCastDirection;
    [SerializeField] Vector2 RightConeEdgeRayCastDirection;
    [SerializeField] Vector2 CenterConeEdgeRayCastDirection;

    RaycastHit2D LeftEdgeHit;
    RaycastHit2D RightEdgeHit;
    RaycastHit2D CenterEdgeHit;

    private bool playerSeen;

    private void Update()
    {
        //Create the raycast
        LeftEdgeHit = Physics2D.Raycast(transform.position, raycastOrigin.transform.TransformDirection(LeftConeEdgeRayCastDirection), raycastRange);
        RightEdgeHit = Physics2D.Raycast(transform.position, raycastOrigin.transform.TransformDirection(RightConeEdgeRayCastDirection), raycastRange);
        CenterEdgeHit = Physics2D.Raycast(transform.position, raycastOrigin.transform.TransformDirection(CenterConeEdgeRayCastDirection), raycastRange);

        //Draw the raycast in Scene view
        Debug.DrawRay(transform.position, raycastOrigin.transform.TransformDirection(LeftConeEdgeRayCastDirection) * raycastRange, Color.red);
        Debug.DrawRay(transform.position, raycastOrigin.transform.TransformDirection(RightConeEdgeRayCastDirection) * raycastRange, Color.red);
        Debug.DrawRay(transform.position, raycastOrigin.transform.TransformDirection(CenterConeEdgeRayCastDirection) * raycastRange, Color.red);

        // Well f, we'll try to find to do this more elgantly if there's time, it works this way but there can be optimizations //
        if (LeftEdgeHit.collider != null)
        {

            if (LeftEdgeHit.collider.tag == "Player")
            {
                if (PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Invisible &&
                PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Hidden)
                {
                    //Make sure this script only tries setting the playerseen bool once.
                    if (!playerSeen)
                    {
                        //MusicPlayer.Instance.PlaySoundEffectByName("UI_AI_Drone_Detected_Player");
                        transform.GetComponent<CameraAI>().SetPlayerSeen(true);
                    }

                    playerSeen = true;
                }
            }
            else
                playerSeen = false;
        }

        if (RightEdgeHit.collider != null)
        {

            if (RightEdgeHit.collider.tag == "Player")
            {
                if (PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Invisible &&
                PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Hidden)
                {
                    //Make sure this script only tries setting the playerseen bool once.
                    if (!playerSeen)
                    {
                        //MusicPlayer.Instance.PlaySoundEffectByName("UI_AI_Drone_Detected_Player");
                        transform.GetComponent<CameraAI>().SetPlayerSeen(true);
                    }

                    playerSeen = true;
                }
            }
            else
                playerSeen = false;
        }


        if (CenterEdgeHit.collider != null)
        {

            if (CenterEdgeHit.collider.tag == "Player")
            {
                if (PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Invisible &&
                PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Hidden)
                {
                    //Make sure this script only tries setting the playerseen bool once.
                    if (!playerSeen)
                    {
                        //MusicPlayer.Instance.PlaySoundEffectByName("UI_AI_Drone_Detected_Player");
                        transform.GetComponent<CameraAI>().SetPlayerSeen(true);
                    }

                    playerSeen = true;
                }
            }
            else
                playerSeen = false;
        }
    }

    //Comment out the Update part if you use this code
    public void CheckIfPlayerSeen()
    {
        // Create the raycast
        LeftEdgeHit = Physics2D.Raycast(transform.position, raycastOrigin.transform.TransformDirection(LeftConeEdgeRayCastDirection), raycastRange, ~IgnoreTrigger);
        RightEdgeHit = Physics2D.Raycast(transform.position, raycastOrigin.transform.TransformDirection(RightConeEdgeRayCastDirection), raycastRange, ~IgnoreTrigger);
        CenterEdgeHit = Physics2D.Raycast(transform.position, raycastOrigin.transform.TransformDirection(CenterConeEdgeRayCastDirection), raycastRange, ~IgnoreTrigger);

        // Draw the raycast in Scene view
        Debug.DrawRay(transform.position, raycastOrigin.transform.TransformDirection(LeftConeEdgeRayCastDirection).normalized * raycastRange, Color.red);
        Debug.DrawRay(transform.position, raycastOrigin.transform.TransformDirection(RightConeEdgeRayCastDirection).normalized * raycastRange, Color.red);
        Debug.DrawRay(transform.position, raycastOrigin.transform.TransformDirection(CenterConeEdgeRayCastDirection).normalized * raycastRange, Color.red);

        if (LeftEdgeHit.collider != null || RightEdgeHit.collider != null || CenterEdgeHit.collider != null)
        {
            if (LeftEdgeHit.collider.CompareTag("Player") || RightEdgeHit.collider.CompareTag("Player") || CenterEdgeHit.collider.CompareTag("Player"))
            {
                
                playerSeen = true;
            }
            else
            {
                playerSeen = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (PlayerAbilities.Instance.GetCurrentInvisibleState() != InvisibleAbilityState.Invisible)
        {
            if (collision.CompareTag("Player"))
            {
                CheckIfPlayerSeen();
                if (playerSeen)
                {
                    transform.GetComponentInParent<CameraAI>().SetPlayerSeen(true);
                }
            }
            else
            {
                playerSeen = false;
            }
        }
    }
}
