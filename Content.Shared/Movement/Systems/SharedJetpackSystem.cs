using Content.Shared.Actions;
<<<<<<< HEAD
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
=======
using Content.Shared.Effects.Systems;
>>>>>>> wizards/stable
using Content.Shared.Gravity;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedJetpackSystem : EntitySystem
{
<<<<<<< HEAD
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; //SS220 Magboots with jet fix
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;

    [Dependency] private readonly EntityQuery<JetpackComponent> _jetpackQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JetpackComponent, GetItemActionsEvent>(OnJetpackGetAction);
        SubscribeLocalEvent<JetpackComponent, DroppedEvent>(OnJetpackDropped);
        SubscribeLocalEvent<JetpackComponent, ToggleJetpackEvent>(OnJetpackToggle);

        SubscribeLocalEvent<JetpackUserComponent, RefreshWeightlessModifiersEvent>(OnJetpackUserWeightlessMovement);

        SubscribeLocalEvent<JetpackUserComponent, CanWeightlessMoveEvent>(OnJetpackUserCanWeightless);
        SubscribeLocalEvent<JetpackUserComponent, EntParentChangedMessage>(OnJetpackUserEntParentChanged);
        SubscribeLocalEvent<JetpackUserComponent, MagbootsUpdateStateEvent>(OnMagbootsUpdateState); //SS220 Magboots with jet fix
        SubscribeLocalEvent<JetpackComponent, EntGotInsertedIntoContainerMessage>(OnJetpackMoved);

        SubscribeLocalEvent<GravityChangedEvent>(OnJetpackUserGravityChanged);
        SubscribeLocalEvent<JetpackComponent, MapInitEvent>(OnMapInit);
    }
=======
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] protected SharedAppearanceSystem Appearance = default!;
    [Dependency] private SharedContainerSystem Container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    [Dependency] private SharedParticleEmitterSystem _particleEmitter = default!;

    [Dependency] private EntityQuery<JetpackComponent> _jetpackQuery;
>>>>>>> wizards/stable

    [SubscribeLocalEvent]
    private void OnJetpackUserWeightlessMovement(Entity<JetpackUserComponent> ent, ref RefreshWeightlessModifiersEvent args)
    {
        // Yes, this bulldozes the values but primarily for backwards compat atm.
        args.WeightlessAcceleration = ent.Comp.WeightlessAcceleration;
        args.WeightlessModifier = ent.Comp.WeightlessModifier;
        args.WeightlessFriction = ent.Comp.WeightlessFriction;
        args.WeightlessFrictionNoInput = ent.Comp.WeightlessFrictionNoInput;
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<JetpackComponent> ent, ref MapInitEvent args)
    {
        _actionContainer.EnsureAction(ent.Owner, ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnJetpackUserGravityChanged(ref GravityChangedEvent ev)
    {
        var gridUid = ev.ChangedGridIndex;
        var query = EntityQueryEnumerator<JetpackUserComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var user, out var transform))
        {
            if (transform.GridUid != gridUid || !ev.HasGravity ||
                !_jetpackQuery.TryGetComponent(user.Jetpack, out var jetpack))
                continue;

            _popup.PopupEntity(Loc.GetString("jetpack-to-grid"), uid, uid);
            SetEnabled((user.Jetpack, jetpack), false, uid);
        }
    }

    [SubscribeLocalEvent]
    private void OnJetpackDropped(Entity<JetpackComponent> ent, ref DroppedEvent args)
    {
        SetEnabled(ent.AsNullable(), false, args.User);
    }

    [SubscribeLocalEvent]
    private void OnJetpackMoved(Entity<JetpackComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (args.Container.Owner != ent.Comp.JetpackUser)
            SetEnabled(ent.AsNullable(), false, ent.Comp.JetpackUser);
    }

    [SubscribeLocalEvent]
    private void OnJetpackUserCanWeightless(Entity<JetpackUserComponent> ent, ref CanWeightlessMoveEvent args)
    {
        args.CanMove = true;
    }

    [SubscribeLocalEvent]
    private void OnJetpackUserEntParentChanged(Entity<JetpackUserComponent> ent, ref EntParentChangedMessage args)
    {
<<<<<<< HEAD
        if (TryComp<JetpackComponent>(component.Jetpack, out var jetpack) &&
            (!CanEnableOnGrid(args.Transform.GridUid) || HasActiveMagboots(uid)) && // SS220 Magboots with jet fix
            !HasMoonBoots(uid)) // ss220-fix-jetpack-effect
        {
            SetEnabled(component.Jetpack, jetpack, false, uid);
=======
        if (!_jetpackQuery.TryGetComponent(ent.Comp.Jetpack, out var jetpack) ||
            CanEnableOnGrid(args.Transform.GridUid))
            return;
>>>>>>> wizards/stable

        SetEnabled((ent.Comp.Jetpack, jetpack), false, ent.Owner);
        _popup.PopupEntity(Loc.GetString("jetpack-to-grid"), ent.Owner, ent.Owner);
    }

    private void SetupUser(Entity<JetpackComponent> ent, EntityUid user)
    {
        EnsureComp<JetpackUserComponent>(user, out var userComp);
        ent.Comp.JetpackUser = user;

        if (TryComp<PhysicsComponent>(user, out var physics))
            _physics.SetBodyStatus(user, physics, BodyStatus.InAir);

        userComp.Jetpack = ent.Owner;
        userComp.WeightlessAcceleration = ent.Comp.Acceleration;
        userComp.WeightlessModifier = ent.Comp.WeightlessModifier;
        userComp.WeightlessFriction = ent.Comp.Friction;
        userComp.WeightlessFrictionNoInput = ent.Comp.Friction;

        _movementSpeedModifier.RefreshWeightlessModifiers(user);

        // TODO_ss220: this could be obsolete
        //ss220 fix activated jetpack in container start
        if (!TryComp<JetpackComponent>(jetpackUid, out var jetpackComponent))
            return;

        jetpackComponent.User = user;
        //ss220 fix activated jetpack in container end
    }

    private void RemoveUser(Entity<JetpackComponent> ent, EntityUid user)
    {
<<<<<<< HEAD
        //ss220 fix activated jetpack in container start
        if (!TryComp<JetpackUserComponent>(uid, out var jetpackUser))
            return;

        if (!TryComp<JetpackComponent>(jetpackUser.Jetpack, out var jetpackComponent))
            return;

        jetpackComponent.User = null;
        //ss220 fix activated jetpack in container end
        if (!RemComp<JetpackUserComponent>(uid))
=======
        if (!RemComp<JetpackUserComponent>(user))
>>>>>>> wizards/stable
            return;

        ent.Comp.JetpackUser = null;

        if (TryComp<PhysicsComponent>(user, out var physics))
            _physics.SetBodyStatus(user, physics, BodyStatus.OnGround);

        _movementSpeedModifier.RefreshWeightlessModifiers(user);
    }

    [SubscribeLocalEvent]
    private void OnJetpackToggle(Entity<JetpackComponent> ent, ref ToggleJetpackEvent args)
    {
        if (args.Handled)
            return;

<<<<<<< HEAD
        //ss220 magboots with jet on gravity fix start
        if (!TryComp(uid, out TransformComponent? xform))
            return;
        //ss220 magboots with jet on gravity fix end

        //SS220 Magboots with jet fix begin
        if (HasActiveMagboots(args.Performer))
        {
            _popup.PopupClient(Loc.GetString("jetpack-no-magboots-on-grid"), uid, args.Performer);
            return;
        }

        var slotEnumerator = _inventory.GetSlotEnumerator(args.Performer);
        while (slotEnumerator.NextItem(out var item))
        {
            // SS220 FIX JETPACK CAMERA START (fix: https://github.com/SerbiaStrong-220/space-station-14/issues/1746)
            if (TryComp<BuckleComponent>(args.Performer, out var buckleComponent) && buckleComponent.BuckledTo != null)
            {
                _buckle.TryUnbuckle(args.Performer, args.Performer, buckleComponent);
            }
            // SS220 FIX JETPACK CAMERA END

            //SS220 Moonboots with jet fix begin
            if (HasComp<AntiGravityClothingComponent>(item))
            {
                SetEnabled(uid, component, !IsEnabled(uid));
                return;
            }
            //SS220 Moonboots with jet fix end
        }
        //SS220 Magboots with jet fix end

        //ss220 magboots with jet on gravity fix start
        if (!CanEnableOnGrid(xform.GridUid))
=======
        if (!CanEnableOnGrid(Transform(ent.Owner).GridUid))
>>>>>>> wizards/stable
        {
            _popup.PopupEntity(Loc.GetString("jetpack-no-station"), ent.Owner, args.Performer);
            return;
        }
        //ss220 magboots with jet on gravity fix end

        SetEnabled(ent.AsNullable(), !IsEnabled(ent.Owner));
    }

    private bool CanEnableOnGrid(EntityUid? gridUid)
    {
<<<<<<< HEAD
        //return gridUid == null ||
        //       (!HasComp<GravityComponent>(gridUid));  //SS220 Convert to comment

        //SS220 Fix jet in zero gravity begin
        if (gridUid == null || !TryComp<GravityComponent>(gridUid, out var comp))
            return true;

        return !comp.Enabled;
        //SS220 Fix jet in zero gravity end
    }

    // SS220 Magboots with jet fix begin
    /// <summary>
    ///     Checks whether the entity can use jet with magboots
    /// </summary>
    /// <returns>
    ///     true if entity can use jet with magboots
    /// </returns>
    private bool HasActiveMagboots(EntityUid user)
    {
        var xform = Transform(user);
        if (xform.GridUid is null)
            return false;

        var slotEnumerator = _inventory.GetSlotEnumerator(user);
        while (slotEnumerator.NextItem(out var item))
        {
            if (HasComp<MagbootsComponent>(item) &&
                TryComp<ItemToggleComponent>(item, out var itemToggle) &&
                itemToggle.Activated)
                return true;
        }

        return false;
    }
    // SS220 Magboots with jet fix end

    // ss220-fix-jetpack-effect-begin
    private bool HasMoonBoots(EntityUid user)
    {
        var slotEnumerator = _inventory.GetSlotEnumerator(user);
        while (slotEnumerator.NextItem(out var item, out var slot))
        {
            if (slot.SlotFlags != SlotFlags.FEET)
                continue;

            if (HasComp<AntiGravityClothingComponent>(item))
                return true;
        }

        return false;
    }
    // ss220-fix-jetpack-effect-end

    private void OnJetpackGetAction(EntityUid uid, JetpackComponent component, GetItemActionsEvent args)
=======
        // No and no again! Do not attempt to activate the jetpack on a grid with gravity disabled. You will not be the first or the last to try this.
        // https://discord.com/channels/310555209753690112/310555209753690112/1270067921682694234
        return gridUid == null || !HasComp<GravityComponent>(gridUid);
    }

    [SubscribeLocalEvent]
    private void OnJetpackGetAction(Entity<JetpackComponent> ent, ref GetItemActionsEvent args)
>>>>>>> wizards/stable
    {
        args.AddAction(ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
    }

    private bool IsEnabled(EntityUid uid)
    {
        return HasComp<ActiveJetpackComponent>(uid);
    }

    public void SetEnabled(Entity<JetpackComponent?> ent, bool enabled, EntityUid? user = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (IsEnabled(ent.Owner) == enabled ||
            enabled && !CanEnable((ent.Owner, ent.Comp)))
            return;

        if (user == null)
        {
            if (!Container.TryGetContainingContainer((ent.Owner, null, null), out var container))
                return;

            user = container.Owner;
        }

        if (enabled)
        {
            if (TryComp<JetpackUserComponent>(user, out var userComp) &&
                userComp.Jetpack != ent.Owner &&
                _jetpackQuery.TryGetComponent(userComp.Jetpack, out var oldJetpack))
                SetEnabled((userComp.Jetpack, oldJetpack), false, user);

            SetupUser((ent.Owner, ent.Comp), user.Value);
            EnsureComp<ActiveJetpackComponent>(ent.Owner);
        }
        else
        {
            RemoveUser((ent.Owner, ent.Comp), user.Value);
            RemComp<ActiveJetpackComponent>(ent.Owner);
        }

<<<<<<< HEAD
        // SS220-add-gas-usage-modifier-begin
        TryComp<ActiveJetpackComponent>(uid, out var activeJetpack);

        var ev = new JetPackActivatedEvent((uid, activeJetpack));
        RaiseLocalEvent(user.Value, ref ev);
        // SS220-add-gas-usage-modifier-end
=======
        _particleEmitter.SetEnabled(ent.Owner, enabled);
>>>>>>> wizards/stable

        Appearance.SetData(ent.Owner, JetpackVisuals.Enabled, enabled);
        Dirty(ent.Owner, ent.Comp);
    }

    protected virtual bool CanEnable(Entity<JetpackComponent> ent)
    {
        return true;
    }

    //SS220 Magboots with jet fix begin
    private void OnMagbootsUpdateState(Entity<JetpackUserComponent> ent, ref MagbootsUpdateStateEvent args)
    {
        if (!args.State)
            return;

        if (TryComp<JetpackComponent>(ent.Comp.Jetpack, out var jetpack))
        {
            SetEnabled(ent.Comp.Jetpack, jetpack, false, ent.Owner);
            _popup.PopupClient(Loc.GetString("jetpack-to-grid"), ent.Comp.Jetpack, ent.Owner);
        }
    }
    //SS220 Magboots with jet fix end
}

[Serializable, NetSerializable]
public enum JetpackVisuals : byte
{
    Enabled,
    Layer
}
