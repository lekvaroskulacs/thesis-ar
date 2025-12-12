using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Mirror;
using Newtonsoft.Json.Linq;
using UnityEngine;

class ReplayProcessor : NetworkBehaviour
{
    public AutoResetEvent waitEvent = new AutoResetEvent(false);
    public SyncDictionary<uint, NetworkIdentity> replayIdToNetObj = new SyncDictionary<uint, NetworkIdentity>();
    public NetworkIdentity lastPlayed;

    public bool canExecuteNext = true;

    public IEnumerator WaitForCreature(ServerCreatureField field)
    {
        while (field.creature == null)
        {
            yield return new WaitForSeconds(0.01f);
        }
        lastPlayed = field.creature.netIdentity;
        canExecuteNext = true;
    }

    public void ProcessCommand(ReplayEvent evt, bool isReplay = false)
    {   
        NetworkGamePlayer player;
        if (Convert.ToInt32(evt.connectionId) >= 0)
        {
            var players = (NetworkManager.singleton as NetworkManagerImpl).gamePlayers;
            if (isReplay == false)
            {
                player = players.data.Find((p) => p.connectionToClient.connectionId == Convert.ToInt32(evt.connectionId));
            }
            else
            {
                player = players.data[Convert.ToInt32(evt.connectionId)];
            }
        }
        else
        {
            player = null;
        }

        if (evt.commandName == "CmdPlayCreature")
        {
            player.PlayCreature(
                evt.parameters["creatureIdentifier"].ToString(),
                Convert.ToInt32(evt.parameters["manaCost"]),
                Convert.ToInt32(evt.parameters["creatureSlot"])
            );

            //var field = player.isHost ?
            //    player.board.battlefield.hostFields.ElementAt(Convert.ToInt32(evt.parameters["creatureSlot"])) :
            //    player.board.battlefield.guestFields.ElementAt(Convert.ToInt32(evt.parameters["creatureSlot"]));

            var field = player.isHost ?
                (NetworkManager.singleton as NetworkManagerImpl).serverBoard.battlefield.hostFields.ElementAt(Convert.ToInt32(evt.parameters["creatureSlot"])) :
                (NetworkManager.singleton as NetworkManagerImpl).serverBoard.battlefield.guestFields.ElementAt(Convert.ToInt32(evt.parameters["creatureSlot"]));
            
            canExecuteNext = false;
            StartCoroutine(WaitForCreature(field));
        }

        if (evt.commandName == "LogCreaturePlayed")
        {
            replayIdToNetObj[Convert.ToUInt32(evt.parameters["netId"])] = lastPlayed;
            lastPlayed = null;
        }
        
        if (evt.commandName == "CmdEndTurn")
        {
            player.EndTurn();
        }

        if (evt.commandName == "CmdChangeState")
        {
            player.ChangeState(
                (TurnState) Convert.ToInt32(evt.parameters["state"])
            );
        }

        if (evt.commandName == "CmdCommenceAttack")
        {
            var ids = (evt.parameters["creatureNetIds"] as JArray).ToObject<List<uint>>();
            List<uint> actualIds = new List<uint>();
            foreach (var id in ids)
            {
                actualIds.Add(replayIdToNetObj[id].netId);
            }

            player.CommenceAttack(
                actualIds
            );
        }

        if (evt.commandName == "CmdCommenceBlock")
        {
            var ids = (evt.parameters["creatureNetIds"] as JArray).ToObject<List<uint>>();
            List<uint> actualIds = new List<uint>();
            foreach (var id in ids)
            {
                actualIds.Add(replayIdToNetObj[id].netId);
            }

            player.ConfirmBlock(
                actualIds
            );
        }

        if (evt.commandName == "CmdResolveCombat")
        {
           player.ResolveCombat();
        }


        Creature cr = null;
        if  (replayIdToNetObj.ContainsKey(evt.netId))
        {
           cr = replayIdToNetObj[evt.netId].GetComponent<Creature>();
        }

        if (evt.commandName == "CmdToggleCanAttack")
        {
            cr.canAttack = (
                (bool) evt.parameters["canAttack"]
            );
        }

        if (evt.commandName == "CmdToggleAttack")
        {
            cr.attacking = !cr.attacking;
        }

        if (evt.commandName == "CmdToggleBlock")
        {
            cr.blocking = !cr.blocking;
        }

        if (evt.commandName == "CmdConfirmAttack")
        {
            cr.attackConfirmed = true;
        }

        if (evt.commandName == "CmdConfirmBlock")
        {
            cr.blockConfirmed = true;
        }

        if (evt.commandName == "CmdResetCombatState")
        {
            cr.ResetCombatState();
        }

        if (evt.commandName == "CmdMovingCreatureState")
        {
            var moving = (MovingCreature) cr;
            moving.MovingCreatureState();
        }

        if (evt.commandName == "MoveToField")
        {
            var moving = (MovingCreature) cr;
            moving.targetField = Convert.ToInt32(evt.parameters["field"]);
        }

        if (evt.commandName == "CmdMoveCreature")
        {
            var moving = (MovingCreature) cr;
            moving.MoveToTargetField();
        }
    }

}