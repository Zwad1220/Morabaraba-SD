using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class UndoRedoTests
{
    private GameManager gm;
    private UndoRedoManager undo;
    private GameObject gameObj;

    [SetUp]
    public void Setup()
    {
        // Arrange: Initialize GameManager and UndoRedoManager instances
        gameObj = new GameObject();
        gm = gameObj.AddComponent<GameManager>();
        undo = gameObj.AddComponent<UndoRedoManager>();

        //  Set instances
        GameManager.instance = gm;
        UndoRedoManager.instance = undo;

        gm.allNodes = new Node[1];
        GameObject nObj = new GameObject();
        gm.allNodes[0] = nObj.AddComponent<Node>();
        gm.allNodes[0].nodeID = 0;
    }

    [TearDown]
    public void Teardown() => Object.DestroyImmediate(gameObj);

    // NORMAL CASE: Save and Undo
    [Test]
    public void Undo_AfterSave_RestoresPreviousPlayer()
    {
        gm.currentPlayer = 1;
        undo.SaveState();

        gm.currentPlayer = 2; // Move happens
        undo.Undo();

        Assert.AreEqual(1, gm.currentPlayer, "Undo should restore the player turn to 1.");
    }

    // INVALID/ERROR CASE: Undo when no state is saved
    [Test]
    public void Undo_WithNoHistory_DoesNotCrash()
    {
        // No SaveState() called
        Assert.DoesNotThrow(() => undo.Undo());
    }

    // SPECIAL CASE: Redo after Undo
    [Test]
    public void Redo_AfterUndo_RestoresTheUndoneMove()
    {
        gm.currentPlayer = 1;
        undo.SaveState();

        gm.currentPlayer = 2;
        undo.Undo(); // Back to 1
        undo.Redo(); // Back to 2

        Assert.AreEqual(2, gm.currentPlayer, "Redo should restore the player turn to 2.");
    }
}