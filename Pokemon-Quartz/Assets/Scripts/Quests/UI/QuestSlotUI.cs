using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text completionText;

    RectTransform rectTransform;

    private void Awake()
    {
    }

    public Text NameText => nameText;
    public Text CompletionText => completionText;

    public float Height => rectTransform.rect.height;

    public void SetQuestData(Quest questSlot)
    {
        rectTransform = GetComponent<RectTransform>();
        nameText.text = questSlot.Base.Name;
        completionText.text = $"X {questSlot.Status}";
    }
}
