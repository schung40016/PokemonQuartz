using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialogue);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;

        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialogue);

        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

           string playerName = player.GetComponent<PlayerController>().Name;
           yield return DialogManager.Instance.ShowDialogText($"{playerName} received {Base.RequiredItem.Name}.");
        }
    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
            {
                return false;
            }
        }

        return true;
    }
}

public enum QuestStatus {  None, Started, Completed }
