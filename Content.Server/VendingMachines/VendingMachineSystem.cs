using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server.Cargo.Systems;
using Content.Server.VendingMachines.Components;
using Content.Server.Vocalization.Systems;
using Content.Shared.Cargo;
using Content.Shared.Damage.Systems;
using Content.Shared.Emp;
using Content.Shared.Throwing;
using Content.Shared.VendingMachines;
using Content.Shared.VendingMachines.Components;
using Content.Shared.Wall;
using Robust.Server.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.VendingMachines;

public sealed partial class VendingMachineSystem : SharedVendingMachineSystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PricingSystem _pricing = default!;
    [Dependency] private ThrowingSystem _throwingSystem = default!;

    private const float WallVendEjectDistanceFromWall = 1f;

    protected override bool ShouldThrowVendItem(Entity<VendingMachineEjectComponent> entity)
    {
<<<<<<< HEAD
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PricingSystem _pricing = default!;
        [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
        [Dependency] private readonly ContainerSystem _container = default!; // SS220 SS220 vend-dupe-fix
=======
        return HasComp<VendingMachineShootComponent>(entity.Owner);
    }
>>>>>>> wizards/stable

    protected override void EjectItem(Entity<VendingMachineComponent?, VendingMachineEjectComponent?> entity, bool forceEject = false)
    {
        if (!Resolve(entity.Owner, ref entity.Comp1, ref entity.Comp2))
            return;

        var uid = entity.Owner;
        var ejectComponent = entity.Comp2;

        if (ejectComponent.NextItemToEject is not { } item)
        {
            ejectComponent.ThrowNextItem = false;
            return;
        }

        // Default spawn coordinates
        var xform = Transform(uid);
        var spawnCoordinates = xform.Coordinates;

        //Make sure the wallvends spawn outside of the wall.
        if (TryComp<WallMountComponent>(uid, out var wallMountComponent))
        {
            var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * WallVendEjectDistanceFromWall;
            spawnCoordinates = spawnCoordinates.Offset(offset);
        }

        var ent = Spawn(item, spawnCoordinates);

        if (ejectComponent.ThrowNextItem)
        {
            var range = ejectComponent.NonLimitedEjectRange;
            var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
            _throwingSystem.TryThrow(ent, direction, ejectComponent.NonLimitedEjectForce);
        }

        ejectComponent.NextItemToEject = null;
        ejectComponent.ThrowNextItem = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = Timing.CurTime;
        var dispenseOnHitQuery = EntityQueryEnumerator<VendingMachineDispenseOnHitComponent>();
        while (dispenseOnHitQuery.MoveNext(out _, out var dispenseOnHit))
        {
            if (dispenseOnHit.NextDispenseTime is not { } nextDispenseTime || curTime <= nextDispenseTime)
                continue;

            dispenseOnHit.NextDispenseTime = null;
        }

        var disabled = EntityQueryEnumerator<EmpDisabledComponent, VendingMachineComponent, VendingMachineEjectComponent>();
        while (disabled.MoveNext(out var uid, out _, out var comp, out var eject))
        {
            if (eject.NextEmpEject >= curTime) continue;

            EjectRandom((uid, comp, eject), true, false);
            eject.NextEmpEject += (5 * eject.EjectDelay);
        }
    }

    [SubscribeLocalEvent]
    private void OnVendingPrice(Entity<VendingMachineComponent> entity, ref PriceCalculationEvent args)
    {
        var price = 0.0;

        foreach (var entry in entity.Comp.Inventory.Values)
        {
            if (!ProtoMan.TryIndex<EntityPrototype>(entry.ID, out var proto))
            {
                Log.Error($"Unable to find entity prototype {entry.ID} on {ToPrettyString(entity)} vending.");
                continue;
            }

            price += entry.Amount * _pricing.GetEstimatedPrice(proto);
        }

        args.Price += price;
    }

    [SubscribeLocalEvent]
    private void OnDamageChanged(Entity<VendingMachineComponent> entity, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased && entity.Comp.Broken)
        {
            entity.Comp.Broken = false;
            Dirty(entity);
            return;
        }

        if (!TryComp<VendingMachineDispenseOnHitComponent>(entity.Owner, out var dispenseOnHit))
            return;

        if (entity.Comp.Broken || dispenseOnHit.CoolingDown || args.DamageDelta == null)
            return;

        if (!(args.DamageIncreased && args.DamageDelta.GetTotal() >= dispenseOnHit.Threshold) ||
            !_random.Prob(dispenseOnHit.Chance)) return;

        if (dispenseOnHit.NextDispenseDelay != null)
        {
            dispenseOnHit.NextDispenseTime = Timing.CurTime + dispenseOnHit.NextDispenseDelay.Value;
        }

        EjectRandom((entity.Owner, entity.Comp), throwItem: true, forceEject: true);
    }

    [SubscribeLocalEvent]
    private void OnSelfDispense(Entity<VendingMachineComponent> entity, ref VendingMachineSelfDispenseEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        EjectRandom((entity.Owner, entity.Comp), throwItem: true, forceEject: false);
    }

    [SubscribeLocalEvent]
    private void OnPriceCalculation(Entity<VendingMachineRestockComponent> entity, ref PriceCalculationEvent args)
    {
        List<double> priceSets = new();

        // Find the most expensive inventory and use that as the highest price.
        foreach (var vendingInventory in entity.Comp.CanRestock)
        {
            double total = 0;

            if (ProtoMan.TryIndex(vendingInventory, out VendingMachineInventoryPrototype? inventoryPrototype))
            {
                foreach (var (item, amount) in inventoryPrototype.StartingInventory)
                {
<<<<<<< HEAD
                    Log.Error($"Unable to find entity prototype {entry.ID} on {ToPrettyString(uid)} vending.");
                    continue;
                }

                price += entry.Amount * _pricing.GetEstimatedPrice(proto);
            }

            args.Price += price;
        }

        protected override void OnMapInit(EntityUid uid, VendingMachineComponent component, MapInitEvent args)
        {
            base.OnMapInit(uid, component, args);

            if (HasComp<ApcPowerReceiverComponent>(uid))
            {
                TryUpdateVisualState((uid, component));
            }
        }

        private void OnPowerChanged(EntityUid uid, VendingMachineComponent component, ref PowerChangedEvent args)
        {
            TryUpdateVisualState((uid, component));
        }

        private void OnDamageChanged(EntityUid uid, VendingMachineComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased && component.Broken)
            {
                component.Broken = false;
                Dirty(uid, component);
                TryUpdateVisualState((uid, component));
                return;
            }

            if (component.Broken || component.DispenseOnHitCoolingDown ||
                component.DispenseOnHitChance == null || args.DamageDelta == null)
                return;

            if (args.DamageIncreased && args.DamageDelta.GetTotal() >= component.DispenseOnHitThreshold &&
                _random.Prob(component.DispenseOnHitChance.Value))
            {
                if (component.DispenseOnHitCooldown != null)
                {
                    component.DispenseOnHitEnd = Timing.CurTime + component.DispenseOnHitCooldown.Value;
                }

                EjectRandom(uid, throwItem: true, forceEject: true, component);
            }
        }

        private void OnSelfDispense(EntityUid uid, VendingMachineComponent component, VendingMachineSelfDispenseEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            EjectRandom(uid, throwItem: true, forceEject: false, component);
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.CanShoot"/> property of the vending machine.
        /// </summary>
        public void SetShooting(EntityUid uid, bool canShoot, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.CanShoot = canShoot;
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.Contraband"/> property of the vending machine.
        /// </summary>
        public void SetContraband(EntityUid uid, bool contraband, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.Contraband = contraband;
            Dirty(uid, component);
        }

        /// <summary>
        /// Ejects a random item from the available stock. Will do nothing if the vending machine is empty.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="throwItem">Whether to throw the item in a random direction after dispensing it.</param>
        /// <param name="forceEject">Whether to skip the regular ejection checks and immediately dispense the item without animation.</param>
        /// <param name="vendComponent"></param>
        public void EjectRandom(EntityUid uid, bool throwItem, bool forceEject = false, VendingMachineComponent? vendComponent = null)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            var availableItems = GetAvailableInventory(uid, vendComponent);
            if (availableItems.Count <= 0)
                return;

            var item = _random.Pick(availableItems);

            if (forceEject)
            {
                vendComponent.NextItemToEject = item.ID;
                vendComponent.ThrowNextItem = throwItem;
                var entry = GetEntry(uid, item.ID, item.Type, vendComponent);
                if (entry != null)
                    entry.Amount--;
                EjectItem(uid, vendComponent, forceEject);
            }
            else
            {
                TryEjectVendorItem(uid, item.Type, item.ID, throwItem, user: null, vendComponent: vendComponent);
            }
        }

        protected override void EjectItem(EntityUid uid, VendingMachineComponent? vendComponent = null, bool forceEject = false)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            // No need to update the visual state because we never changed it during a forced eject
            if (!forceEject)
                TryUpdateVisualState((uid, vendComponent));

            if (string.IsNullOrEmpty(vendComponent.NextItemToEject))
            {
                vendComponent.ThrowNextItem = false;
                return;
            }

            // Default spawn coordinates
            var xform = Transform(uid);
            var spawnCoordinates = xform.Coordinates;

            //Make sure the wallvends spawn outside of the wall.
            if (TryComp<WallMountComponent>(uid, out var wallMountComponent))
            {
                var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * WallVendEjectDistanceFromWall;
                spawnCoordinates = spawnCoordinates.Offset(offset);
            }

            // SS220 vend-dupe-fix start
            EntityUid? ent = null;

            // Сначала пытаемся получить существующий предмет и выбросить его, если таковой существует
            if (TryGetInjectedItem((uid, vendComponent), vendComponent.NextItemToEject, out var existingItem, out var entry)
                && TryEjectInjectedItem((uid, vendComponent), entry, existingItem.Value))
                ent = existingItem.Value;

            // var ent = Spawn(vendComponent.NextItemToEject, spawnCoordinates);
            ent ??= Spawn(vendComponent.NextItemToEject, spawnCoordinates);
            // SS220 vend-dupe-fix end

            if (vendComponent.ThrowNextItem)
            {
                var range = vendComponent.NonLimitedEjectRange;
                var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
                // SS220 vend-dupe-fix start
                //_throwingSystem.TryThrow(ent, direction, vendComponent.NonLimitedEjectForce);
                _throwingSystem.TryThrow(ent.Value, direction, vendComponent.NonLimitedEjectForce);
                // SS220 vend-dupe-fix end
            }

            vendComponent.NextItemToEject = null;
            vendComponent.ThrowNextItem = false;
        }

        // SS220 vend-dupe-fix start
        public bool TryGetInjectedItem(Entity<VendingMachineComponent> vend, string protoId, [NotNullWhen(true)] out EntityUid? item, [NotNullWhen(true)] out VendingMachineInventoryEntry? entry)
        {
            item = null;
            entry = null;

            entry = GetAllInventory(vend, vend).ToList().Find(x => x.ID == protoId);
            var ents = entry?.EntityUids;

            if (ents == null || ents.Count == 0)
                return false;

            if (TryGetEntity(ents[0], out item) && entry is not null)
                return true;

            return false;
        }

        public bool TryEjectInjectedItem(Entity<VendingMachineComponent> vend, VendingMachineInventoryEntry entry, EntityUid item)
        {
            try
            {
                _container.RemoveEntity(vend, item);
                entry.EntityUids.Remove(GetNetEntity(item));
                Dirty(vend, vend.Comp);
                return true;
            }
            catch
            {
                return false;
            }
        }
        // SS220 vend-dupe-fix end

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var disabled = EntityQueryEnumerator<EmpDisabledComponent, VendingMachineComponent>();
            while (disabled.MoveNext(out var uid, out _, out var comp))
            {
                if (comp.NextEmpEject < Timing.CurTime)
                {
                    EjectRandom(uid, true, false, comp);
                    comp.NextEmpEject += (5 * comp.EjectDelay);
=======
                    if (ProtoMan.TryIndex(item, out EntityPrototype? prototype))
                        total += _pricing.GetEstimatedPrice(prototype) * amount;
>>>>>>> wizards/stable
                }
            }

            priceSets.Add(total);
        }

        args.Price += priceSets.Max();
    }

    [SubscribeLocalEvent]
    private void OnTryVocalize(Entity<VendingMachineComponent> ent, ref TryVocalizeEvent args)
    {
        args.Cancelled |= ent.Comp.Broken;
    }

    public void SetShooting(Entity<VendingMachineEjectComponent?> entity, bool canShoot)
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
            return;

        if (canShoot)
            EnsureComp<VendingMachineShootComponent>(entity.Owner);
        else
            RemComp<VendingMachineShootComponent>(entity.Owner);
    }

    /// <summary>
    /// Sets the <see cref="VendingMachineComponent.Contraband"/> property of the vending machine.
    /// </summary>
    public void SetContraband(Entity<VendingMachineComponent> entity, bool contraband)
    {
        entity.Comp.Contraband = contraband;
        Dirty(entity);
    }

    /// <summary>
    /// Ejects a random item from the available stock. Will do nothing if the vending machine is empty.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="throwItem">Whether to throw the item in a random direction after dispensing it.</param>
    /// <param name="forceEject">Whether to skip the regular ejection checks and immediately dispense the item without animation.</param>
    public void EjectRandom(
        Entity<VendingMachineComponent?, VendingMachineEjectComponent?> entity,
        bool throwItem,
        bool forceEject = false)
    {
        if (!Resolve(entity.Owner, ref entity.Comp1, ref entity.Comp2))
            return;

        var uid = entity.Owner;
        var vendComponent = entity.Comp1;
        var ejectComponent = entity.Comp2;
        var availableItems = GetAvailableInventory(uid, vendComponent);
        if (availableItems.Count <= 0)
            return;

        var item = _random.Pick(availableItems);

        if (forceEject)
        {
            ejectComponent.NextItemToEject = item.ID;
            ejectComponent.ThrowNextItem = throwItem;
            var entry = GetEntry(uid, item.ID, item.Type, vendComponent);
            if (entry != null)
            {
                entry.Amount--;
                Dirty(uid, vendComponent);
                UpdateUI((uid, vendComponent));
            }

            EjectItem((uid, vendComponent, ejectComponent), forceEject);
        }
        else
        {
            TryEjectVendorItem(uid, item.Type, item.ID, throwItem, user: null, vendComponent: vendComponent, ejectComponent: ejectComponent);
        }
    }
}
