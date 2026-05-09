using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Camera & Controls")]
    public Camera mainCamera; // To convert screen clicks to game world positions
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

    // <summary>
    // Triggered every time the player clicks. 
    // Handles Selection, Deselection, and Destination picking.
    // </summary>
    void OnClick()
    {
        if (AIManager.instance.isAIActive && GameManager.instance.currentPlayer == AIManager.instance.aiPlayerNumber)
            return;
        // 1. Raycast to see what was clicked
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Node clickedNode = hit.collider.GetComponent<Node>();
            if (clickedNode == null) return;

            // --- CAPTURE PHASE OVERRIDE ---
            // If the game is waiting for a capture, we skip movement and go to TryCapture
            if (GameManager.instance.IsCapturing())
            {
                GameManager.instance.TryCapture(clickedNode);
                return;
            }

            // --- MOVEMENT PHASE LOGIC ---

            // STEP 1: If nothing is selected yet, try to pick up a piece
            if (selectedNode == null)
            {
                // Only allow selecting a piece that belongs to the current player
                if (clickedNode.owner == GameManager.instance.currentPlayer)
                {
                    SelectPiece(clickedNode);
                }
            }
            // STEP 2: If a piece is already selected, handle the next click
            else
            {
                // A) Clicked the same piece again -> Deselect/Turn off highlight
                if (clickedNode == selectedNode)
                {
                    DeselectPiece();
                }
                // B) Clicked another of your own pieces -> Switch the highlight to that one
                else if (clickedNode.owner == GameManager.instance.currentPlayer)
                {
                    DeselectPiece();
                    SelectPiece(clickedNode);
                }
                // C) Clicked an empty spot -> Attempt to move there
                else if (!clickedNode.isOccupied)
                {
                    TryMove(clickedNode);
                }
            }
        }
    }

    // <summary>
    // Changes the piece to Green and turns on the glow.
    // </summary>
    void SelectPiece(Node node)
    {
        selectedNode = node;
        int player = GameManager.instance.currentPlayer;

        // 1. Change the actual material color to Green
        Renderer r = selectedNode.GetComponent<Renderer>();

        if (r != null)
        {
            r.material.color = Color.green;
        }

        // glow for extra feedback
        Color highlightColor = (player == 1) ? GameManager.instance.p1GlowColor : GameManager.instance.p2GlowColor;
        selectedNode.SetGlow(true, highlightColor);
    }

    // <summary>
    // Reverts the piece back to its original team color and turns off glow.
    // </summary>
    void DeselectPiece()
    {
        if (selectedNode != null)
        {
            //  Determine original team color based on the owner of the selected node
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

    // <summary>
    // Checks Morabaraba rules: Is it a neighbor, or are you in "Flying" mode?
    // </summary>
    void TryMove(Node targetNode)
    {
        if (gm.gameOver) return;
        // Use FlyingPhase instead of manual logic
        if (FlyingPhase.instance.CanMove(selectedNode, targetNode))
        {
            ExecuteMove(targetNode);
        }
        else
        {
            gm.instructionText.text = "Invalid move!";
        }
    }

    // <summary>
    // Moves the piece data and visual color, then resets the glow.
    // </summary>
    void ExecuteMove(Node targetNode)
    {
        int player = GameManager.instance.currentPlayer;

        Color teamColor = (player == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;

        selectedNode.SetGlow(false);
        UndoRedoManager.instance.SaveState();
        selectedNode.ClearNode();

        targetNode.OnClicked(player, teamColor);

        selectedNode = null;

        
        GameManager.instance.CheckMillAndSwitchTurn(targetNode);
    }
}