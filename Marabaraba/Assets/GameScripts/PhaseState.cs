using UnityEngine;

public class PhaseState : MonoBehaviour
{
    public Placement placementPhase; // Reference to the Phase 1 script
    public Movement movementPhase;   // Reference to the Phase 2 script
    public GameManager gm;

    void Start()
    {
     
        movementPhase.enabled = false;//disable movemnt script at the start of the game, only enable placement script
    }

    // Called by the GameManager once the piecesPlaced counter hits 24
    public void SwitchToMovementPhase()
    {

        // Swap the active scripts
        placementPhase.enabled = false; // Stop placing new pieces
        movementPhase.enabled = true;   // Allow moving existing pieces

        // Tell the GameManager to update the UI text 

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