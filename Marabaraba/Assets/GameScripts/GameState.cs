using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public int[] nodeOwners;// 0 for empty, 1 for player 1, 2 for player 2
    public bool[] nodeOccupied;// Parallel array to nodeOwners for quick checks on whether a node is occupied

    public int currentPlayer;// 1 or 2

    public int p1PiecesLeft;// checks how many pieces each player has on the board, used for determining flying phase and win conditions
    public int p2PiecesLeft;// checks how many pieces each player has on the board, used for determining flying phase and win conditions

    public int p1PiecesToPlace;// checks how many pieces each player has left to place on the board, used for determining placement phase
    public int p2PiecesToPlace;// checks how many pieces each player has left to place on the board, used for determining placement phase
    public int piecesPlaced;// tracks how many pieces have been placed on the board, used for determining placement phase

    public bool isCapturing;// tracks whether the current game state is in the middle of a capture, used for undoing captures properly

    public List<int> lastMill;// stores the last mill formed, used for undoing moves and captures
}