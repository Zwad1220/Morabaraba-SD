using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager instance;

    private Stack<GameState> undoStack = new Stack<GameState>();
    private Stack<GameState> redoStack = new Stack<GameState>();
    public GameManager gm;

    void Awake()
    {
        instance = this;
    }

    //Saves the current state of the game
    public void SaveState()
    {
        undoStack.Push(CaptureState());

        //New move clears the history
        redoStack.Clear();

        Debug.Log("State Saved");
    }

    GameState CaptureState()
    {
        GameState state = new GameState();

        Node[] nodes = GameManager.instance.allNodes;

        state.nodeOwners = new int[nodes.Length];
        state.nodeOccupied = new bool[nodes.Length];

        for (int i = 0; i < nodes.Length; i++)
        {
            state.nodeOwners[i] = nodes[i].owner;
            state.nodeOccupied[i] = nodes[i].isOccupied;
        }

        state.currentPlayer = GameManager.instance.currentPlayer;
        state.p1PiecesLeft = GameManager.instance.p1PiecesLeft;
        state.p2PiecesLeft = GameManager.instance.p2PiecesLeft;
        state.piecesPlaced = GameManager.instance.piecesPlaced;

        return state;
    }

    void LoadState(GameState state)
    {
        Node[] nodes = GameManager.instance.allNodes;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (state.nodeOccupied[i])
            {
                int owner = state.nodeOwners[i];

                Color color =
                    (owner == 1)
                    ? GameManager.instance.p1BaseColor
                    : GameManager.instance.p2BaseColor;

                nodes[i].OnClicked(owner, color);
            }
            else
            {
                nodes[i].ClearNode();
            }
        }

        GameManager.instance.currentPlayer = state.currentPlayer;
        GameManager.instance.p1PiecesLeft = state.p1PiecesLeft;
        GameManager.instance.p2PiecesLeft = state.p2PiecesLeft;
        GameManager.instance.piecesPlaced = state.piecesPlaced;

        GameManager.instance.UpdateTurnUI();
    }

    //Undo Function
    public void Undo()
    {
        if (undoStack.Count == 0 || gm.gameOver) return;

        GameState current = CaptureState();
        redoStack.Push(current);

        GameState previous = undoStack.Pop();

        LoadState(previous);

        Debug.Log("Undo");
    }

    //Redo Function
    public void Redo()
    {
        if (redoStack.Count == 0 || gm.gameOver) return;

        GameState current = CaptureState();
        undoStack.Push(current);

        GameState redo = redoStack.Pop();

        LoadState(redo);

        Debug.Log("Redo");
    }
}