using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[TestFixture]
public class GameManagerTests
{
    private GameManager gm;
    private GameObject gameObj;

    [SetUp]
    public void Setup()
    {
        gameObj = new GameObject();
        gm = gameObj.AddComponent<GameManager>();

        // Add dependencies
        var undo = gameObj.AddComponent<UndoRedoManager>();
        var ai = gameObj.AddComponent<AIManager>();

        // CRITICAL: Manual Instance Assignment
        GameManager.instance = gm;
        UndoRedoManager.instance = undo;
        AIManager.instance = ai;

        // FIX: Mock UI References to prevent NullReferenceException
        // We create an empty GameObject and attach a Text component so the script has something to "update"
        gm.winScreen = new GameObject();
        gm.winScreen.SetActive(false);

        // If you use TextMeshPro, add a dummy component
        gm.winText = gm.winScreen.AddComponent<TMPro.TextMeshProUGUI>();

        // Mock turn/piece UI if your script references them directly
        // gm.turnText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();

        // Initialize Nodes
        gm.allNodes = new Node[24];
        for (int i = 0; i < 24; i++)
        {
            GameObject nObj = new GameObject();
            gm.allNodes[i] = nObj.AddComponent<Node>();
            gm.allNodes[i].nodeID = i;

            // FIX: Ensure Node has a Renderer or SpriteRenderer if ClearNode() changes colors
            nObj.AddComponent<SpriteRenderer>();
        }

        gm.p1PiecesToPlace = 12;
        gm.p2PiecesToPlace = 12;
        gm.currentPlayer = 1;
    }

    [TearDown]
    public void Teardown() => Object.DestroyImmediate(gameObj);

    // NORMAL CASE: Standard turn switching
    [Test]
    public void SwitchTurn_FromPlayer1_ChangesCurrentPlayerToPlayer2()
    {
        gm.currentPlayer = 1;
        gm.SwitchTurn();
        Assert.AreEqual(2, gm.currentPlayer, "Turn should switch to Player 2.");
    }

    // BOUNDARY CASE: Last piece placed triggers phase change
    [Test]
    public void OnPiecePlaced_LastPiece_IncrementsPiecesPlacedToLimit()
    {
        gm.piecesPlaced = 23;
        gm.currentPlayer = 1;
        gm.OnPiecePlaced(gm.allNodes[0]);
        Assert.AreEqual(24, gm.piecesPlaced, "Pieces placed should reach 24.");
    }

    // INVALID/ERROR CASE: Capturing an empty node should not be possible
    [Test]
    public void TryCapture_OnEmptyNode_DoesNotDecrementPieceCount()
    {
        gm.p2PiecesLeft = 5;
        Node emptyNode = gm.allNodes[0];
        emptyNode.owner = 0; // Empty

        gm.TryCapture(emptyNode);

        Assert.AreEqual(5, gm.p2PiecesLeft, "Piece count should not decrease when capturing an empty node.");
    }

    // SPECIAL CASE: Capturing a piece in a mill when ALL pieces are in mills
    [Test]
    public void TryCapture_PieceInMill_AllowedIfNoOtherPiecesExist()
    {
        // Setup: P2 has only 3 pieces, all in a mill (0,1,2)
        gm.currentPlayer = 1;
        gm.allNodes[0].owner = 2;
        gm.allNodes[1].owner = 2;
        gm.allNodes[2].owner = 2;
        gm.p2PiecesLeft = 3;

        // Act: Try to capture one of the mill pieces
        gm.TryCapture(gm.allNodes[0]);

        Assert.AreEqual(2, gm.p2PiecesLeft, "Capture should be allowed if the opponent has no pieces outside of mills.");
    }
}