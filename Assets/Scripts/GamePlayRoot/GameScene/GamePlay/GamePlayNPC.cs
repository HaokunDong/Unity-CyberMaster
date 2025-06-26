using Cysharp.Text;
using NodeCanvas.DialogueTrees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayNPC : GamePlayAIEntity, IInteractable
{
    public Transform Transform => gameObject.transform;
    public bool canInteract => true;

    private DialogueTreeController dialogueTreeController = null;

    private DialogueTreeController DialogueTreeController
    {
        get
        {
            if (dialogueTreeController == null)
            {
                dialogueTreeController = GetComponent<DialogueTreeController>();
            }
            return dialogueTreeController;
        }
    }

    public void OnInteract()
    {
        DialogueTreeController?.StartDialogue();
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat(GamePlayId, isGen ? " G " : " ", TableId);
        color = GamePlayId <= 0 ? Color.red : Color.cyan;
        return true;
    }
#endif
}
