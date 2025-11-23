
using Mirror;
using Mirror.Examples.Basic;

public enum TurnState
{
    NOT_IN_GAME,
    PLAYING,
    OPPONENT_TURN,
    OPPONENT_BLOCKING,
    BLOCKING,
    ATTACKING,
    PLAYING_AFTER_ATTACK,
    COMBAT_ANIMATIONS,
    MOVING_CREATURE,
}

public class TurnManager : NetworkBehaviour
{
    NetworkPlayers<NetworkGamePlayer> players;
    NetworkGamePlayer currentPlayer;

    TurnState returnStateFromMoving;


    // not sure if this works on the clients
    public void Init(NetworkPlayers<NetworkGamePlayer> players, NetworkGamePlayer firstPlayer)
    {
        this.players = players;

        currentPlayer = firstPlayer;
        currentPlayer.state = TurnState.PLAYING;
        currentPlayer.maxMana = 1;
        currentPlayer.mana = currentPlayer.maxMana;
        currentPlayer.RpcStartTurn(currentPlayer.netIdentity.connectionToClient);

        var otherPlayer = players.Other(currentPlayer);
        otherPlayer.state = TurnState.OPPONENT_TURN;
        otherPlayer.maxMana = 0;
        otherPlayer.mana = otherPlayer.maxMana;
        otherPlayer.RpcStartGameAsSecond(otherPlayer.netIdentity.connectionToClient);

    }


    public void EndTurn(NetworkGamePlayer player)
    {
        player.state = TurnState.OPPONENT_TURN;
        player.RpcEndTurn(player.netIdentity.connectionToClient);

        var otherPlayer = players.Other(player);
        otherPlayer.state = TurnState.PLAYING;
        otherPlayer.maxMana += 1;
        otherPlayer.mana = otherPlayer.maxMana;
        otherPlayer.RpcStartTurn(otherPlayer.netIdentity.connectionToClient);
        currentPlayer = otherPlayer;
    }

    public void ConfirmBlockers(NetworkGamePlayer player)
    {
        player.state = TurnState.OPPONENT_TURN;

        var otherPlayer = players.Other(player);
        otherPlayer.state = TurnState.PLAYING_AFTER_ATTACK;
    }

    public void ConfirmAttackers(NetworkGamePlayer player)
    {
        player.state = TurnState.OPPONENT_BLOCKING;

        var otherPlayer = players.Other(player);
        otherPlayer.state = TurnState.BLOCKING;
    }

    public void MovingCreature(NetworkGamePlayer player)
    {
        returnStateFromMoving = player.state;
        player.state = TurnState.MOVING_CREATURE;
    }

    public void EndMovingCreature(NetworkGamePlayer player)
    {
        player.state = returnStateFromMoving;
    }
}