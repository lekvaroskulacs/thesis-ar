
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Timeline;

public class MovingCreature : Creature
{
    protected List<CreatureField> creatureFields = new List<CreatureField>();

    private int targetField;

    virtual protected int TargetField()
    {
        return targetField;
    }

    protected void MoveToTargetField()
    {
        networkManager.serverBoard.MoveCreature(this, TargetField());
        networkManager.turnManager.EndMovingCreature(owningPlayer);
    }


    [Command][LogReplay]
    public void CmdMoveCreature()
    {
        ReplayHelper.LogCommandAuto(nameof(CmdMoveCreature), this);
        MoveToTargetField();
    }

    public override void HookBlockToggled(bool oldValue, bool newValue)
    {
        base.HookAttackToggled(oldValue, newValue);

        if (blocking == true)
        {
            List<UnityEngine.Events.UnityAction> lambdas = new List<UnityEngine.Events.UnityAction>();
            for (int i = 0; i < 8; ++i)
            {
                int fieldIndex = i;
                lambdas.Add(() =>
                {
                    targetField = fieldIndex;
                    CmdMoveCreature();
                });
            }

            creatureFields = owningPlayer.board.battlefield.allFields;
            foreach (var field in creatureFields)
            {
                field.fieldSelector.onClick.AddListener(lambdas[creatureFields.IndexOf(field)]);
            }            
        }
        else
        {
            creatureFields = owningPlayer.board.battlefield.allFields;
            foreach (var field in creatureFields)
            {
                field.fieldSelector.onClick.RemoveAllListeners();
            } 
        }

        CmdMovingCreatureState();
    }

    [Command][LogReplay]
    public void CmdMovingCreatureState()
    {
        ReplayHelper.LogCommandAuto(nameof(CmdMovingCreatureState), this);
        if (blocking == true)
        {
            networkManager.turnManager.MovingCreature(owningPlayer);
        }
        else
        {
            networkManager.turnManager.EndMovingCreature(owningPlayer);
        }
    }

    public override void HookBlockConfirmed(bool oldValue, bool newValue)
    {
        networkManager.turnManager.EndMovingCreature(owningPlayer);
    }

}