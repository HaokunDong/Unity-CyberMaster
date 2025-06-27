using NodeCanvas.Framework;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using ParadoxNotion.Design;
using System.Xml.Linq;
using UnityEngine;

[Category("GamePlay")]
public class SetSelfAddPositionAction : ActionTask
{
    public BBParameter<Vector3> dtPos;
    public string posName;

    protected override void OnExecute()
    {
        base.OnExecute();
        if (agent != null && blackboard != null)
        {
            blackboard.SetVariableValue(posName, agent.transform.position + dtPos.value);
        }
        EndAction(true);
    }
}
