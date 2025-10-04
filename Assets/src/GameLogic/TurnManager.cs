
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
    PLAYING_AFTER_ATTACK
}

public class TurnManager : NetworkBehaviour
{
    NetworkPlayers<NetworkGamePlayer> players;
    NetworkGamePlayer currentPlayer;


    // not sure if this works on the clients
    public void Init(NetworkPlayers<NetworkGamePlayer> players, NetworkGamePlayer firstPlayer)
    {
        this.players = players;

        currentPlayer = firstPlayer;
        currentPlayer.state = TurnState.PLAYING;
        currentPlayer.RpcStartTurn(currentPlayer.netIdentity.connectionToClient);

        var otherPlayer = players.Other(currentPlayer);
        otherPlayer.state = TurnState.OPPONENT_TURN;
        otherPlayer.RpcStartGameAsSecond(otherPlayer.netIdentity.connectionToClient);

    }


    public void EndTurn(NetworkGamePlayer player)
    {
        player.state = TurnState.OPPONENT_TURN;
        player.RpcEndTurn(player.netIdentity.connectionToClient);

        var otherPlayer = players.Other(player);
        otherPlayer.state = TurnState.PLAYING;
        otherPlayer.RpcStartTurn(otherPlayer.netIdentity.connectionToClient);
    }

    public void ConfirmBlockers(NetworkGamePlayer player)
    {

    }

    public void ConfirmAttackers(NetworkGamePlayer player)
    {

    }
}