using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[TestFixture]
public class CaptureTests
{
    private GameManager gm;
    private GameObject gameObj;

    [SetUp]
    public void Setup()
    {
        // Initialize the core GameObjects and Script Instances
        gameObj = new GameObject("TestController");
        gm = gameObj.AddComponent<GameManager>();

        // Add logic dependencies
        var undo = gameObj.AddComponent<UndoRedoManager>();
        var ai = gameObj.AddComponent<AIManager>();

        // Assign static instances so the scripts can find each other
        GameManager.instance = gm;
        UndoRedoManager.instance = undo;
        AIManager.instance = ai;

        //  Mock ALL UI Text References to prevent NullReferenceExceptions
        // Each of these is referenced in GameManager's UI update methods
        gm.instructionText = new GameObject("InstructionText").AddComponent<TMPro.TextMeshProUGUI>();
        gm.phaseText = new GameObject("PhaseText").AddComponent<TMPro.TextMeshProUGUI>();
        gm.turnText = new GameObject("TurnText").AddComponent<TMPro.TextMeshProUGUI>();
        gm.p1PiecesText = new GameObject("P1PiecesText").AddComponent<TMPro.TextMeshProUGUI>();
        gm.p2PiecesText = new GameObject("P2PiecesText").AddComponent<TMPro.TextMeshProUGUI>();

        // Win Screen UI
        gm.winScreen = new GameObject("WinScreen");
        gm.winScreen.SetActive(false);
        gm.winText = gm.winScreen.AddComponent<TMPro.TextMeshProUGUI>();

        // Mock the PhaseState dependency
        // GameManager calls FindObjectOfType<PhaseState>(), so we must add it to the scene
        var phaseState = gameObj.AddComponent<PhaseState>();
        phaseState.gm = gm;

        // PhaseState itself needs its internal script references
        phaseState.placementPhase = gameObj.AddComponent<Placement>();
        phaseState.movementPhase = gameObj.AddComponent<Movement>();

        // Initialize the Board (Nodes)
        gm.allNodes = new Node[24];
        for (int i = 0; i < 24; i++)
        {
            GameObject nObj = new GameObject("Node_" + i);
            Node node = nObj.AddComponent<Node>();
            node.nodeID = i;

            // Node.ClearNode() requires a Renderer to avoid errors
            nObj.AddComponent<SpriteRenderer>();

            gm.allNodes[i] = node;
        }

        // Set Initial Game State for testing
        gm.p1PiecesToPlace = 12;
        gm.p2PiecesToPlace = 12;
        gm.currentPlayer = 1;
        gm.gameOver = false;
    }
    [TearDown]
    public void Teardown() => Object.DestroyImmediate(gameObj);

    // NORMAL CASE: Capturing a standard opponent piece
    [Test]
    public void TryCapture_OpponentPieceOutsideMill_DecrementsOpponentCount()
    {
        // Arrange: Set up the game state for a valid capture scenario
        gm.currentPlayer = 1;
        gm.p2PiecesLeft = 5;
        Node target = gm.allNodes[0];
        target.owner = 2; // Piece belongs to P2

        // Act: Attempt to capture the opponent's piece 
        gm.TryCapture(target);

        // Assert: Verify that P2's piece count decreases and the node is cleared
        Assert.AreEqual(4, gm.p2PiecesLeft, "P2 piece count should decrease after valid capture.");
        Assert.AreEqual(0, target.owner, "Node should be empty after capture.");
    }

    // INVALID CASE: Trying to capture your own piece
    [Test]
    public void TryCapture_OwnPiece_DoesNotRemovePiece()
    {
        // Arrange: Set up the game state where the current player tries to capture their own piece
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 5;
        Node target = gm.allNodes[0];
        target.owner = 1; // Own piece

        // Act: Attempt to capture own piece
        gm.TryCapture(target);

        // Assert: Verify that own piece count does not change and the node remains owned by the player
        Assert.AreEqual(5, gm.p1PiecesLeft, "Own piece count should not change.");
        Assert.AreEqual(1, target.owner, "Node owner should remain unchanged.");
    }

    // SPECIAL CASE: Rule check - Cannot capture piece in mill if others exist outside
    [Test]
    public void TryCapture_PieceInMillWhileOthersExist_FailsToCapture()
    {
        // Arrange: Set up a scenario where the opponent has a piece in a mill but also has pieces outside the mill
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

        // Assert: Verify that the capture fails because there is a piece outside the mill, and the piece in the mill remains
        Assert.AreEqual(4, gm.p2PiecesLeft, "Capture should be blocked because a piece exists outside the mill.");
        Assert.AreEqual(2, gm.allNodes[0].owner, "Mill piece should still exist.");
    }

    // BOUNDARY CASE: Capturing last piece to trigger win
    [Test]
    public void TryCapture_LastPossiblePiece_SetsGameOver()
    {
        // Arrange: Set up the game state where capturing one more piece would leave the opponent with only 2 pieces, triggering a win condition
        gm.piecesPlaced = 24; // Movement phase
        gm.currentPlayer = 1;
        gm.p2PiecesLeft = 3;
        gm.allNodes[0].owner = 2;
        gm.winScreen = new GameObject(); // Mock UI
        gm.winText = gm.winScreen.AddComponent<TMPro.TextMeshProUGUI>();

        // Act: Attempt to capture the last piece that would leave the opponent with only 2 pieces
        gm.TryCapture(gm.allNodes[0]);

        // Assert: Verify that the opponent's piece count decreases and the game is marked as over
        Assert.IsTrue(gm.gameOver, "Game should be over when opponent has only 2 pieces left.");
    }
}