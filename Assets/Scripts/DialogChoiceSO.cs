using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog Choice", menuName = "Dialog System/Dialog Chocie")]

public class DialogChoiceSO : ScriptableObject
{
    public string text;
    public int nextId;
}
