using NUnit.Framework;
using UnityEngine;

public class CaptureTests
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

            gm.allNodes[i] = n;
        }

        node = gm.allNodes[0];

        GameManager.instance = gm;
    }

    [Test]
    public void test_capture_removes_piece()
    {
        // Arrange
        gm.currentPlayer = 1;

        node.OnClicked(2, Color.blue);

        // Act
        gm.TryCapture(node);

        // Assert
        Assert.IsFalse(node.isOccupied);
    }

    [Test]
    public void test_capture_reduces_piece_count()
    {
        // Arrange
        gm.currentPlayer = 1;

        gm.p2PiecesLeft = 5;

        node.OnClicked(2, Color.blue);

        // Act
        gm.TryCapture(node);

        // Assert
        Assert.AreEqual(4, gm.p2PiecesLeft);
    }

    // INVALID CASE

    [Test]
    public void test_player_cannot_capture_own_piece()
    {
        // Arrange
        gm.currentPlayer = 1;

        node.OnClicked(1, Color.red);

        // Act
        gm.TryCapture(node);

        // Assert
        Assert.IsTrue(node.isOccupied);
    }

    // INVALID CASE

    [Test]
    public void test_player_cannot_capture_empty_node()
    {
        // Arrange
        gm.currentPlayer = 1;

        // Act
        gm.TryCapture(node);

        // Assert
        Assert.AreEqual(0, node.owner);
    }
}