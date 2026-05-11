using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager instance;

    private GameState previousState;
    private GameState redoState;

    void Awake()
    {
        instance = this;
    }

    //Saves current game state
    public void SaveState()
    {
        previousState = CreateGameState();

        //Clear redo whenever a new move happens
        redoState = null;
    }

    public void ClearHistory()
    {
        previousState = null;
        redoState = null;
    }

    //Undo only last move
    public void Undo()
    {
        if (previousState == null)
        {
            Debug.Log("No Undo Available");
            return;
        }

        //Saves current state for redo
        redoState = CreateGameState();

        //Restores previous state
        RestoreGameState(previousState);
        FindObjectOfType<Movement>().ClearValidMoves();

        //Clear undo so it can only happen once
        previousState = null;

        Debug.Log("Undo Complete");
    }

    //Redo
    public void Redo()
    {
        if (redoState == null)
        {
            Debug.Log("No Redo Available");
            return;
        }

        //Saves current state back into undo
        previousState = CreateGameState();

        //Restores redo state
        RestoreGameState(redoState);
        FindObjectOfType<Movement>().ClearValidMoves();

        //Clears redo after use
        redoState = null;

        Debug.Log("Redo Complete");
    }


    //Create game state snapshot

    GameState CreateGameState()
    {
        GameState state = new GameState();

        state.nodeOwners = new int[GameManager.instance.allNodes.Length];

        for (int i = 0; i < GameManager.instance.allNodes.Length; i++)
        {
            state.nodeOwners[i] =
                GameManager.instance.allNodes[i].owner;
        }

        state.currentPlayer =
            GameManager.instance.currentPlayer;

        state.piecesPlaced =
            GameManager.instance.piecesPlaced;

        state.p1PiecesLeft =
            GameManager.instance.p1PiecesLeft;

        state.p2PiecesLeft =
            GameManager.instance.p2PiecesLeft;

        state.p1PiecesToPlace =
    GameManager.instance.p1PiecesToPlace;

        state.p2PiecesToPlace =
            GameManager.instance.p2PiecesToPlace;

        // Save capture state
        state.isCapturing =
            GameManager.instance.IsCapturing();
        // Save capture state
        state.isCapturing =
            GameManager.instance.isCapturing;

        return state;

    }


    //Restore game state
 

    void RestoreGameState(GameState state)
    {
        for (int i = 0; i < GameManager.instance.allNodes.Length; i++)
        {
            Node node = GameManager.instance.allNodes[i];

            node.ClearNode();

            int owner = state.nodeOwners[i];

            if (owner != 0)
            {
                Color color =
                    (owner == 1)
                    ? GameManager.instance.p1BaseColor
                    : GameManager.instance.p2BaseColor;

                node.OnClicked(owner, color);
            }
        }

        GameManager.instance.currentPlayer =
            state.currentPlayer;

        GameManager.instance.piecesPlaced =
            state.piecesPlaced;

        GameManager.instance.p1PiecesLeft =
            state.p1PiecesLeft;

        GameManager.instance.p2PiecesLeft =
            state.p2PiecesLeft;

        GameManager.instance.p1PiecesToPlace =
    state.p1PiecesToPlace;

        GameManager.instance.p2PiecesToPlace =
            state.p2PiecesToPlace;

        // Restore capture mode
        GameManager.instance.isCapturing =
            state.isCapturing;

        GameManager.instance.UpdateTurnUI();
        GameManager.instance.UpdatePieceUI();

        // Restore capture UI
        if (state.isCapturing)
        {
            GameManager.instance.instructionText.text =
                "Player " +
                GameManager.instance.currentPlayer +
                ": Capture a piece!";

            GameManager.instance.SetCaptureHighlights(true);
        }
        else
        {
            GameManager.instance.SetCaptureHighlights(false);
        }
    }
}