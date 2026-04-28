using UnityEngine;
using UnityEngine.InputSystem;

public class Placement : MonoBehaviour
{
    public Camera mainCamera; // Used to translate screen clicks into the 3D/2D world

    private InputSystem_Actions controls; // Reference to the Unity Input System actions

    void Awake()
    {
        // Initialize the input system
        controls = new InputSystem_Actions();

        // Connect the "Click" action to our OnClick method
        controls.Gameplay.Click.performed += ctx => OnClick();
    }

    // These ensure the script only listens for clicks when it is active (enabled)
    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void OnClick()
    {
        // 1. Get the mouse position and convert it to a point in the game world
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        // 2. Fire a Raycast (an invisible laser) to see if we clicked on a Node collider
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Node node = hit.collider.GetComponent<Node>();
            if (node == null) return; // If we didn't hit a node object, stop here

            // --- CASE A: CAPTURING ---
            // If the GameManager says we are in "Capture Mode", clicks steal pieces instead of placing them
            if (GameManager.instance.IsCapturing())
            {
                GameManager.instance.TryCapture(node);
            }

            // --- CASE B: PLACING ---
            // If the node is empty, we can place a piece
            else if (!node.isOccupied)
            {
                // We fetch the current player and their specific color from the GameManager
                int player = GameManager.instance.currentPlayer;

                // 🔥 Sync colors: Pulling the Base Color we set in the GameManager script
                Color cowColor = (player == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;

                // 1. Update the Node's visual color and data (who owns it)
                node.OnClicked(player, cowColor);

                // 2. Tell the GameManager a piece was placed so it can check for mills and swap turns
                GameManager.instance.OnPiecePlaced(node);
            }
        }
    }
}