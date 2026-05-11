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

        // Connect the "Click" action to OnClick method
        controls.Gameplay.Click.performed += ctx => OnClick();
    }

    //  ensure the script only listens for clicks when it is enabled
    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void OnClick()
    {
            if (GameManager.instance.gameOver) return;

            // Get the position directly from the action context
            Vector2 inputPos = Pointer.current.position.ReadValue();

            //  Convert to world space
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(inputPos);

            // Raycast
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        

        if (hit.collider != null)
        {
            Node node = hit.collider.GetComponent<Node>();
            if (node == null) return; // If we didn't hit a node object, return

            // Capturing
            // If the GameManager is in "Capture Mode", clicks steal pieces instead of placing them
            if (GameManager.instance.IsCapturing())
            {
                GameManager.instance.TryCapture(node);
            }

            // Placing 
            // checks the node is empty, to place a piece
            else if (!node.isOccupied)
            {
                //  fetchs the current player and their specific color from the GameManager
                int player = GameManager.instance.currentPlayer;

                //  Sync colors
                Color cowColor = (player == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;

                UndoRedoManager.instance.SaveState();

                //Update the Node's visual color and data (who owns it)
                node.OnClicked(player, cowColor);

                // Tells the GameManager a piece was placed so it can check for mills and swap turns
                GameManager.instance.OnPiecePlaced(node);
            }
        }
    }
}