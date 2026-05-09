using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[TestFixture]
public class CaptureLogicTests
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

    // NORMAL CASE: Capturing a standard opponent piece
    [Test]
    public void TryCapture_OpponentPieceOutsideMill_DecrementsOpponentCount()
    {
        // Arrange
        gm.currentPlayer = 1;
        gm.p2PiecesLeft = 5;
        Node target = gm.allNodes[0];
        target.owner = 2; // Piece belongs to P2

        // Act
        gm.TryCapture(target);

        // Assert
        Assert.AreEqual(4, gm.p2PiecesLeft, "P2 piece count should decrease after valid capture.");
        Assert.AreEqual(0, target.owner, "Node should be empty after capture.");
    }

    // INVALID CASE: Trying to capture your own piece
    [Test]
    public void TryCapture_OwnPiece_DoesNotRemovePiece()
    {
        // Arrange
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 5;
        Node target = gm.allNodes[0];
        target.owner = 1; // Own piece

        // Act
        gm.TryCapture(target);

        // Assert
        Assert.AreEqual(5, gm.p1PiecesLeft, "Own piece count should not change.");
        Assert.AreEqual(1, target.owner, "Node owner should remain unchanged.");
    }

    // SPECIAL CASE: Rule check - Cannot capture piece in mill if others exist outside
    [Test]
    public void TryCapture_PieceInMillWhileOthersExist_FailsToCapture()
    {
        // Arrange
        gm.currentPlayer = 1;
        gm.p2PiecesLeft = 4;

        // Piece in mill
        gm.allNodes[0].owner = 2;
        gm.allNodes[1].owner = 2;
        gm.allNodes[2].owner = 2;

        // Piece NOT in mill
        gm.allNodes[3].owner = 2;

        // Act: Try to capture the one in the mill
        gm.TryCapture(gm.allNodes[0]);

        // Assert
        Assert.AreEqual(4, gm.p2PiecesLeft, "Capture should be blocked because a piece exists outside the mill.");
        Assert.AreEqual(2, gm.allNodes[0].owner, "Mill piece should still exist.");
    }

    // BOUNDARY CASE: Capturing last piece to trigger win
    [Test]
    public void TryCapture_LastPossiblePiece_SetsGameOver()
    {
        // Arrange
        gm.piecesPlaced = 24; // Movement phase
        gm.currentPlayer = 1;
        gm.p2PiecesLeft = 3;
        gm.allNodes[0].owner = 2;
        gm.winScreen = new GameObject(); // Mock UI
        gm.winText = gm.winScreen.AddComponent<TMPro.TextMeshProUGUI>();

        // Act
        gm.TryCapture(gm.allNodes[0]);

        // Assert
        Assert.IsTrue(gm.gameOver, "Game should be over when opponent has only 2 pieces left.");
    }
}