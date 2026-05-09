using UnityEngine;

// This script acts like a traffic cop. It ensures that the Placement script and Movement script
// are never trying to fight for control of the mouse clicks at the same time.
public class PhaseState : MonoBehaviour
{
    public Placement placementPhase; // Reference to the Phase 1 script
    public Movement movementPhase;   // Reference to the Phase 2 script
    public GameManager gm;

    void Start()
    {
        // When the game starts, players are putting pieces down, not moving them.
        // So, we turn Placement ON and Movement OFF.
    
        movementPhase.enabled = false;
    }

    // Called by the GameManager once the piecesPlaced counter hits 24
    public void SwitchToMovementPhase()
    {

        // Swap the active scripts
        placementPhase.enabled = false; // Stop placing new pieces
        movementPhase.enabled = true;   // Allow clicking and dragging

        // Tell the GameManager to update the UI text at the top of the screen
        
        GameManager.instance.UpdatePieceUI();
        //gm.instructionText.text = "Move a piece to any adjacent empty slot.";
        if (gm.currentPlayer == 1 && !gm.p1FlyingPhase)
        {
            gm.instructionText.text = "Move a piece to any adjacent empty slot.";
            GameManager.instance.UpdatePhaseUI("Movement Phase");
        }
        if (gm.currentPlayer == 2 && !gm.p2FlyingPhase)
        {
            gm.instructionText.text = "Move a piece to any adjacent empty slot.";
            GameManager.instance.UpdatePhaseUI("Movement Phase");
        }
    }
}