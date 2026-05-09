using NUnit.Framework;
using UnityEngine;

public class UndoRedoTests
{
    UndoRedoManager manager;
    GameManager gm;

    [SetUp]
    public void Setup()
    {
        GameObject gmObj = new GameObject();
        gm = gmObj.AddComponent<GameManager>();

        gm.allNodes = new Node[24];

        for (int i = 0; i < 24; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Node node = obj.AddComponent<Node>();

            gm.allNodes[i] = node;
        }

        GameManager.instance = gm;

        GameObject obj2 = new GameObject();
        manager = obj2.AddComponent<UndoRedoManager>();
    }

    [Test]
    public void test_save_state_does_not_throw_error()
    {
        // Assert
        Assert.DoesNotThrow(() => manager.SaveState());
    }

    // INVALID CASE

    [Test]
    public void test_undo_without_previous_state_does_not_crash()
    {
        // Assert
        Assert.DoesNotThrow(() => manager.Undo());
    }

    [Test]
    public void test_redo_without_redo_state_does_not_crash()
    {
        // Assert
        Assert.DoesNotThrow(() => manager.Redo());
    }
}