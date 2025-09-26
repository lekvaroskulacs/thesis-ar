using System.Collections.Generic;

public class Players<Type>
{
    private readonly List<Type> players = new List<Type>(2);

    /// <summary>
    /// Adds a player. Returns true if added, false if list is full or already contains the player.
    /// </summary>
    public bool Add(Type player)
    {
        if (players.Count >= 2 || players.Contains(player))
        {
            return false;
        }

        players.Add(player);

        return true;
    }

    /// <summary>
    /// Removes a player. Returns true if removed, false if not found.
    /// </summary>
    public bool Remove(Type player)
    {
        bool removed = players.Remove(player);
        return removed;
    }

    /// <summary>
    /// Returns the other player than the one passed in, or default if not found.
    /// </summary>
    public Type Other(Type player)
    {
        if (players.Count != 2) return default;

        if (EqualityComparer<Type>.Default.Equals(players[0], player))
            return players[1];

        if (EqualityComparer<Type>.Default.Equals(players[1], player))
            return players[0];

        return default;
    }

    public List<Type> data
    {
        get
        {
            return players;
        }
    }
}
