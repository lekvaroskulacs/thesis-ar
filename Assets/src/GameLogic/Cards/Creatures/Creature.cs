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
    [SyncVar] private bool _blocking = false;
    [SyncVar] private bool _attackConfirmed = false;
    [SyncVar] private bool _blockConfirmed = false;

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

    public bool canAttack { get; internal set; } = false;

    void Awake()
    {
        health = maxHealth;
    }

    [Command]
    public virtual void CmdTakeDamage(int damage)
    {
        health -= damage;
        // Die()
    }

    [Command]
    public virtual void CmdHeal(int healing)
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