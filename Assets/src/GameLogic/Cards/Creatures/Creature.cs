using System;
using Mirror;
using UnityEngine;

public class Creature : Card
{
    [HideInInspector] public NetworkGamePlayer owningPlayer;
    public string creatureIdentifier;
    [SyncVar][SerializeField] private int _attack;
    [SyncVar][SerializeField] private int _maxHealth;
    [SyncVar] private int _health;
    [SyncVar(hook = nameof(HookAttackToggled))] private bool _attacking = false;
    [SyncVar(hook = nameof(HookBlockToggled))] private bool _blocking = false;
    [SyncVar] private bool _attackConfirmed = false;
    [SyncVar] private bool _blockConfirmed = false;
    [SyncVar] private bool _canAttack = false;

    public int attack
    {
        get { return _attack; }
        internal set { _attack = value; }
    }

    public int maxHealth
    {
        get { return _maxHealth; }
        internal set { _maxHealth = value; }
    }

    public int health
    {
        get { return _health; }
        internal set { _health = value; }
    }

    public bool attacking
    {
        get { return _attacking; }
        internal set { _attacking = value; }
    }

    public bool blocking
    {
        get { return _blocking; }
        internal set { _blocking = value; }
    }

    public bool attackConfirmed
    {
        get { return _attackConfirmed; }
        internal set { _attackConfirmed = value; }
    }

    public bool blockConfirmed
    {
        get { return _blockConfirmed; }
        internal set { _blockConfirmed = value; }
    }

    public bool canAttack
    {
        get { return _canAttack; }
        internal set { _canAttack = value; }
    }

    private NetworkManagerImpl _networkManager;
    private NetworkManagerImpl networkManager
    {
        get
        {
            if (_networkManager != null)
            {
                return _networkManager;
            }
            return _networkManager = NetworkManager.singleton as NetworkManagerImpl;
        }
    }

    void Awake()
    {
        health = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        networkManager.serverBoard.CreatureDestroyed(owningPlayer, this);
        NetworkServer.Destroy(gameObject);
    }
    

    public virtual void Heal(int healing)
    {
        health = Math.Min(maxHealth, health + healing);
    }

    [Command]
    public virtual void CmdToggleCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }

    [Command]
    public virtual void CmdToggleAttack()
    {
        attacking = !attacking;
    }

    [Command]
    public virtual void CmdToggleBlock()
    {
        blocking = !blocking;
    }

    [Command]
    public virtual void CmdConfirmAttack()
    {
        attackConfirmed = true;
    }

    [Command]
    public virtual void CmdConfirmBlock()
    {
        blockConfirmed = true;
    }

    [Command]
    public virtual void CmdResetCombatState()
    {
        ResetCombatState();
    }

    public virtual void ResetCombatState()
    {
        attacking = false;
        blocking = false;
        attackConfirmed = false;
        blockConfirmed = false;
    }
    
    public virtual void RequestBeginTurn()
    {
        CmdToggleCanAttack(true);
    }

    public virtual void RequestToggleAttack()
    {
        CmdToggleAttack();
    }

    public virtual void HookAttackToggled(bool oldValue, bool newValue)
    {
        if (attacking)
        {
            owningPlayer.attackingCreatures.Add(this);
        }
        else
        {
            owningPlayer.attackingCreatures.Remove(this);
        }
    }

    public virtual void RequestToggleBlock()
    {
        CmdToggleBlock();
    }

    public virtual void HookBlockToggled(bool oldValue, bool newValue)
    {
        if (blocking)
        {
            owningPlayer.blockingCreatures.Add(this);
        }
        else
        {
            owningPlayer.blockingCreatures.Remove(this);
        }
    }

    public virtual void RequestConfirmAttack()
    {
        CmdConfirmAttack();
    }

    public virtual void RequestConfirmBlock()
    {
        CmdConfirmBlock();
    }

    public virtual void RequestResetCombatState()
    {
        CmdResetCombatState();
    }
}