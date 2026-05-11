using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class FlyingPhaseTests
{
    private GameManager gm;
    private FlyingPhase flying;
    private GameObject gameObj;

    [SetUp]
    public void Setup()
    {
        // Arrange: Create test environment
        gameObj = new GameObject();

        gm = gameObj.AddComponent<GameManager>();
        flying = gameObj.AddComponent<FlyingPhase>();

        // Static instances
        GameManager.instance = gm;
        FlyingPhase.instance = flying;

        // FULL board setup
        gm.allNodes = new Node[24];

        for (int i = 0; i < 24; i++)
        {
            GameObject nObj = new GameObject();

            Node node = nObj.AddComponent<Node>();
            node.nodeID = i;

            // Needed for renderer/material logic
            var renderer = nObj.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            gm.allNodes[i] = node;
        }

        gm.currentPlayer = 1;
    }
    [TearDown]
    public void Teardown() => Object.DestroyImmediate(gameObj);

    // NORMAL CASE: Moving to a neighbor when not flying
    [Test]
    public void CanMove_ToNeighbor_ReturnsTrue()
    {
        gm.piecesPlaced = 24; // Movement phase
        gm.p1PiecesLeft = 5;  // Not flying
        Node start = gm.allNodes[0];
        Node target = gm.allNodes[1];
        start.neighbours.Add(target);

        bool result = flying.CanMove(start, target);
        Assert.IsTrue(result);
    }

    // BOUNDARY CASE: Exactly 3 pieces allows flying (anywhere)
    [Test]
    public void IsFlyingActive_WithExactlyThreePieces_ReturnsTrue()
    {
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 3;

        Assert.IsTrue(gm.IsFlying(1), "Player with 3 pieces should be allowed to fly.");
    }

    // BOUNDARY CASE: Exactly 4 pieces prevents flying
    [Test]
    public void IsFlyingActive_WithFourPieces_ReturnsFalse()
    {
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 4;

        Assert.IsFalse(gm.IsFlying(1), "Player with 4 pieces should NOT be allowed to fly.");
    }

    // INVALID CASE: Moving to a node already occupied
    [Test]
    public void CanMove_ToOccupiedNode_ReturnsFalse()
    {
        Node start = gm.allNodes[0];
        Node target = gm.allNodes[1];
        target.isOccupied = true;

        Assert.IsFalse(flying.CanMove(start, target), "Cannot move to a node that is already occupied.");
    }

    [Test]// SPECIAL CASE: Flying allows movement to any empty node, not just neighbors
    public void CanMove_WhenFlyingIsActive_AllowsMovementToNonNeighbors()
    {
        // Arrange
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 3;

        Node start = gm.allNodes[0];
        Node target = gm.allNodes[10]; // Distant node

        // Act
        bool result = FlyingPhase.instance.CanMove(start, target);

        // Assert
        Assert.IsTrue(result, "Flying should allow movement to any empty node regardless of proximity.");
    }

    [Test]// SPECIAL CASE: Player with no valid moves (all neighbors occupied) cannot move
    public void PlayerWithNoValidMoves_CannotMoveAnywhere()
    {
        // Arrange: Set up a scenario where the player has no valid moves (all neighbors occupied)
        gm.piecesPlaced = 24;

        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 4; // NOT flying

        Node playerNode = gm.allNodes[0];
        playerNode.owner = 1;
        playerNode.isOccupied = true;

        Node blocked1 = gm.allNodes[1];
        blocked1.owner = 2;
        blocked1.isOccupied = true;

        Node blocked2 = gm.allNodes[2];
        blocked2.owner = 2;
        blocked2.isOccupied = true;

        // Only neighbours available are occupied
        playerNode.neighbours.Add(blocked1);
        playerNode.neighbours.Add(blocked2);

        // Act: Attempt to move to either occupied neighbor
        bool move1 = flying.CanMove(playerNode, blocked1);
        bool move2 = flying.CanMove(playerNode, blocked2);

        // Assert: Both moves should be invalid
        Assert.IsFalse(move1);
        Assert.IsFalse(move2);
    }
}