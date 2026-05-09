using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Camera & Controls")]
    public Camera mainCamera;
    private InputSystem_Actions controls;

    [Header("Selection State")]
    private Node selectedNode; // Stores the piece currently picked up/highlighted
    public GameManager gm;
    void Awake()
    {
        // Initializing the new Unity Input System
        controls = new InputSystem_Actions();
        controls.Gameplay.Click.performed += ctx => OnClick();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    
    // Triggered every time the player clicks. 
    // Handles Selection, Deselection, and Destination picking.
    void OnClick()
    {
        if (AIManager.instance.isAIActive && GameManager.instance.currentPlayer == AIManager.instance.aiPlayerNumber)
            return;
        // Raycast to see what was clicked
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Node clickedNode = hit.collider.GetComponent<Node>();
            if (clickedNode == null) return;

           
           
            if (GameManager.instance.IsCapturing())//checks if we are in capture mode, if so, try to capture the clicked node instead of moving
            {
                GameManager.instance.TryCapture(clickedNode);//calls capture logic in gamemanager, which checks if the capture is valid and updates the game state accordingly
                return;
            }

            // movement logic

            // checks if nothing is selected and tries to pick up a piece
            if (selectedNode == null)
            {
                // Only allow selecting a piece that belongs to the current player
                if (clickedNode.owner == GameManager.instance.currentPlayer)
                {
                    SelectPiece(clickedNode);
                }
            }
            // handles the next click if piece is already selected, either deselecting, switching selection, or trying to move
            else
            {
                // Clicked the same piece again- Deselect/Turn off highlight
                if (clickedNode == selectedNode)
                {
                    DeselectPiece();
                }
                // Clicked another of your own pieces - Switch the highlight to that one
                else if (clickedNode.owner == GameManager.instance.currentPlayer)
                {
                    DeselectPiece();
                    SelectPiece(clickedNode);
                }
                //Clicked an empty spot - Attempt to move there
                else if (!clickedNode.isOccupied)
                {
                    TryMove(clickedNode);
                }
            }
        }
    }

    
    // Changes the piece to Green and turns on the glow.
    void SelectPiece(Node node)
    {
        selectedNode = node;
        int player = GameManager.instance.currentPlayer;

        // Change the actual material color to Green
        Renderer r = selectedNode.GetComponent<Renderer>();

        if (r != null)
        {
            r.material.color = Color.green;
        }

        // glow for extra feedback
        Color highlightColor = (player == 1) ? GameManager.instance.p1GlowColor : GameManager.instance.p2GlowColor;
        selectedNode.SetGlow(true, highlightColor);
    }

  
    // Reverts the piece back to its original team color and turns off glow.
    void DeselectPiece()
    {
        if (selectedNode != null)
        {
            //  Determines original team color based on the owner of the selected node
            Color teamColor = (selectedNode.owner == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;

            //  Revert the material color
            Renderer r = selectedNode.GetComponent<Renderer>();

            if (r != null)
            {
                r.material.color = teamColor;
            }

            //  Turn off the glow
            selectedNode.SetGlow(false);
        }
        selectedNode = null;
    }

   
    // checks if moving to neighbour or in the flying phase and handles errors accordingly
    void TryMove(Node targetNode)
    {
        if (gm.gameOver) return;
        // calls flying phase logic
        if (FlyingPhase.instance.CanMove(selectedNode, targetNode))
        {
            ExecuteMove(targetNode);
        }
        else
        {
            gm.instructionText.text = "Invalid move!";
        }
    }

 
    // Moves the piece data and visual color, then resets the glow.
    void ExecuteMove(Node targetNode)
    {
        int player = GameManager.instance.currentPlayer;

        Color teamColor = (player == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;// Determine the team color based on the current player

        selectedNode.SetGlow(false);
        UndoRedoManager.instance.SaveState();// Save the state before making the move for undo functionality
        selectedNode.ClearNode();

        targetNode.OnClicked(player, teamColor);// Update the target node with the new piece

        selectedNode = null;

        
        GameManager.instance.CheckMillAndSwitchTurn(targetNode);// Check if the move formed a mill and switch turns
    }
}