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

        // Add logic dependencies
        var undo = gameObj.AddComponent<UndoRedoManager>();
        var ai = gameObj.AddComponent<AIManager>();

        // Assign static instances
        GameManager.instance = gm;
        UndoRedoManager.instance = undo;
        AIManager.instance = ai;

        // MOCK UI: Prevent the NullReferenceException
        gm.instructionText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        gm.turnText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        gm.phaseText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        gm.p1PiecesText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        gm.p2PiecesText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();

        // MOCK PHASESTATE: GameManager calls FindObjectOfType<PhaseState>()
        var ps = gameObj.AddComponent<PhaseState>();
        ps.gm = gm;
        ps.placementPhase = gameObj.AddComponent<Placement>();
        ps.movementPhase = gameObj.AddComponent<Movement>();

        gm.allNodes = new Node[24];
        for (int i = 0; i < 24; i++)
        {
            GameObject nObj = new GameObject();
            gm.allNodes[i] = nObj.AddComponent<Node>();
            gm.allNodes[i].nodeID = i;

            // Give each node a material so sharedMaterial doesn't fail
            var renderer = nObj.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        gm.p1PiecesToPlace = 12;
        gm.p2PiecesToPlace = 12;
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
    // Verifies the core mill detection algorithm
    [Test]
    public void CheckForMill_WhenThreePiecesFormLine_IdentifiesValidMill()
    {
        // Arrange
        gm.allNodes[0].owner = 1;
        gm.allNodes[1].owner = 1;
        gm.allNodes[2].owner = 1;

        // Act
        bool result = gm.CheckForMill(gm.allNodes[0], 1);

        // Assert
        Assert.IsTrue(result, "The system should identify three pieces in a line as a mill.");
    }

    // Tests the rule regarding breaking and reforming mills
    [Test]
    public void CheckForMill_WhenMillIsReformed_ValidatesCorrectly()
    {
        // Arrange: Set up two pieces, leaving the third slot empty
        gm.allNodes[1].owner = 1;
        gm.allNodes[2].owner = 1;
        gm.allNodes[0].owner = 0;

        // Act: Place a piece to reform the mill
        gm.allNodes[0].owner = 1;
        bool result = gm.CheckForMill(gm.allNodes[0], 1);

        // Assert
        Assert.IsTrue(result, "Reforming a mill must be detected as a new mill event.");
    }

    [Test]
    public void ResetGame_WhenCalled_RestoresInitialState()
    {
        // Arrange: Set up a modified game state (Guideline: Arrange - Act - Assert)
        gm.allNodes[0].owner = 1;
        gm.p1PiecesLeft = 5;
        gm.gameOver = true;

        // Act: Perform the reset operation
        gm.ResetGame();

        // Assert: Verify expected behavior (Guideline: Normal cases and Expected results)
        Assert.AreEqual(0, gm.allNodes[0].owner, "Nodes were not cleared.");
        Assert.AreEqual(12, gm.p1PiecesToPlace, "Placement count did not reset.");
        Assert.IsFalse(gm.gameOver, "Game over state was not cleared.");
    }

    [Test]// Tests the transition from placement to movement phase after all pieces are placed
    public void PlacementPhase_When24PiecesPlaced_SwitchesToMovementPhase()
    {
        // Arrange: Set up the game state to be one piece away from completing placement
        gm.piecesPlaced = 23;
        gm.currentPlayer = 1;

        PhaseState ps = Object.FindObjectOfType<PhaseState>();

        // Act: Place the last piece to trigger the phase transition
        gm.OnPiecePlaced(gm.allNodes[0]);

        // Assert: Verify that the pieces placed count is correct and phases are updated accordingly
        Assert.AreEqual(24, gm.piecesPlaced);

        Assert.IsFalse(ps.placementPhase.enabled,
            "Placement phase should be disabled.");

        Assert.IsTrue(ps.movementPhase.enabled,
            "Movement phase should be enabled.");
    }

    [Test]// Tests the flying phase condition when a player has only three pieces left
    public void PlayerWithThreePieces_EntersFlyingPhase()
    {
        // Arrange: Set up the game state to reflect a player having only three pieces left
        gm.piecesPlaced = 24;
        gm.p1PiecesLeft = 3;

        // Act: Check if the flying phase is active for the player with three pieces
        bool result = gm.IsFlying(1);

        // Assert: Verify that the flying phase is active for the player with three pieces
        Assert.IsTrue(result);
    }

    [Test]// Tests that the game reset functionality properly restores the initial game state after a game over scenario
    public void ResetGame_AfterGameOver_RestoresInitialState()
    {
        // Arrange: Set up a game over scenario with modified game state
        gm.gameOver = true;

        gm.currentPlayer = 2;

        gm.piecesPlaced = 24;

        gm.allNodes[0].owner = 1;
        gm.allNodes[0].isOccupied = true;

        // Act: Call the reset function to restore the game state
        gm.ResetGame();

        // Assert: Verify that the game state has been restored to its initial conditions
        Assert.IsFalse(gm.gameOver);

        Assert.AreEqual(1, gm.currentPlayer);

        Assert.AreEqual(0, gm.piecesPlaced);

        Assert.AreEqual(0, gm.allNodes[0].owner);

        Assert.IsFalse(gm.allNodes[0].isOccupied);
    }

}