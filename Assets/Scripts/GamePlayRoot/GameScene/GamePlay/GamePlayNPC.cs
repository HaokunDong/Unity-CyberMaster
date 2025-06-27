using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Everlasting.Extend;
using Managers;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;

public class GamePlayNPC : GamePlayAIEntity, IInteractable
{
    public Transform Transform => gameObject.transform;
    public bool canInteract => (dialogueTreeController != null && dialogueTreeController.behaviour != null);

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

    public override void AfterAIInit(Blackboard blackboard)
    {
        base.AfterAIInit(blackboard);
        var data = NPCTable.GetTableData(TableId);

        if(data != null)
        {
            if(!data.DialoguePath.IsNullOrEmpty())
            {
                TrySetDialogue(data.DialoguePath).Forget();
            }
        }
    }

    public async UniTask TrySetDialogue(string path)
    {
        var tree = await ResourceManager.LoadAssetAsync<DialogueTree>(path, ResType.Dialogue);
        if(tree != null)
        {
            if(dialogueTreeController == null)
            {
                dialogueTreeController = gameObject.AddComponent<DialogueTreeController>();
            }
            dialogueTreeController.behaviour = tree;
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
