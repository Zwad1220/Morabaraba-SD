using UnityEngine;

// This script acts like a traffic cop. It ensures that the Placement script and Movement script
// are never trying to fight for control of the mouse clicks at the same time.
public class PhaseState : MonoBehaviour
{
    public Placement placementPhase; // Reference to the Phase 1 script
    public Movement movementPhase;   // Reference to the Phase 2 script

    void Start()
    {
        // When the game starts, players are putting pieces down, not moving them.
        // So, we turn Placement ON and Movement OFF.
        placementPhase.enabled = true;
        movementPhase.enabled = false;
    }

    // Called by the GameManager once the piecesPlaced counter hits 24
    public void SwitchToMovementPhase()
    {

        // Swap the active scripts
        placementPhase.enabled = false; // Stop placing new pieces
        movementPhase.enabled = true;   // Allow clicking and dragging

        // Tell the GameManager to update the UI text at the top of the screen
        GameManager.instance.UpdatePhaseUI("Movement Phase");
    }
}