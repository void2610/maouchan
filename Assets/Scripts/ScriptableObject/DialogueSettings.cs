using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueEntry
{
    [TextArea(2, 4)] public string text;
    public CharacterAnimationType animationType;
    public Sprite characterSprite; // nullの場合はデフォルト画像を使用
}

[Serializable]
public class DialogueData
{
    public List<DialogueEntry> dialogues;
}

[CreateAssetMenu(fileName = "DialogueSettings", menuName = "Settings/DialogueSettings")]
public class DialogueSettings : ScriptableObject
{
    [SerializeField] private List<DialogueData> entries;

    public List<DialogueEntry> GetDialogues(int areaIndex) => entries[areaIndex].dialogues;
}
