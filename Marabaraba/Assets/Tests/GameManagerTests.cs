using NUnit.Framework;
using UnityEngine;

public class GameManagerTests
{
    GameManager gm;
    Node node;

    [SetUp]
    public void Setup()
    {
        GameObject gmObj = new GameObject();
        gm = gmObj.AddComponent<GameManager>();

        gm.allNodes = new Node[24];

        for (int i = 0; i < 24; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Node n = obj.AddComponent<Node>();
            n.nodeID = i;

            gm.allNodes[i] = n;
        }

        node = gm.allNodes[0];

        GameManager.instance = gm;
    }

    [Test]
    public void test_switch_turn_changes_player()
    {
        // Arrange
        gm.currentPlayer = 1;

        // Act
        gm.SwitchTurn();

        // Assert
        Assert.AreEqual(2, gm.currentPlayer);
    }

    [Test]
    public void test_piece_count_increases_after_placement()
    {
        // Arrange
        gm.currentPlayer = 1;

        // Act
        gm.OnPiecePlaced(node);

        // Assert
        Assert.AreEqual(1, gm.p1PiecesLeft);
    }

    [Test]
    public void test_pieces_to_place_decreases_after_placement()
    {
        // Arrange
        gm.currentPlayer = 1;
        gm.p1PiecesToPlace = 12;

        // Act
        gm.OnPiecePlaced(node);

        // Assert
        Assert.AreEqual(11, gm.p1PiecesToPlace);
    }

    // INVALID CASE

    [Test]
    public void test_switch_turn_does_not_change_when_game_over()
    {
        // Arrange
        gm.gameOver = true;
        gm.currentPlayer = 1;

        // Act
        gm.SwitchTurn();

        // Assert
        Assert.AreEqual(1, gm.currentPlayer);
    }
}