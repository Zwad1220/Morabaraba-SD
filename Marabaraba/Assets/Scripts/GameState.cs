using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public int[] nodeOwners;
    public bool[] nodeOccupied;

    public int currentPlayer;

    public int p1PiecesLeft;
    public int p2PiecesLeft;

    public int piecesPlaced;

    public bool isCapturing;

    public List<int> lastMill;
}