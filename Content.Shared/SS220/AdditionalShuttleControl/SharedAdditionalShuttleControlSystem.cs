using Content.Shared.DeviceLinking;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.Physics;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.AdditionalShuttleControl;

public abstract class SharedAdditionalShuttleControlSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedDeviceListSystem _deviceList = default!;
    [Dependency] private readonly SharedDeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public const string Trigger = "Trigger";

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestResetRotateShuttleGuns>(OnRequestResetRotateShuttleGuns);
        SubscribeNetworkEvent<RequestRotateGunToPoint>(OnRequestRotateGunToPoint);
        SubscribeNetworkEvent<RequestShuttleGunsFire>(OnRequestShuttleGunsFire);

        SubscribeLocalEvent<AdditionalShuttleControlComponent, DeviceListUpdateEvent>(OnDeviceListUpdate);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AdditionalShuttleControlComponent>();

        while (query.MoveNext(out var console, out var additionalControl))
        {
            if (additionalControl.LastRotateToPoint == null)
                continue;

            RotateToPoint((console, additionalControl), additionalControl.LastRotateToPoint.Value);
        }
    }

    private void OnRequestResetRotateShuttleGuns(RequestResetRotateShuttleGuns ev)
    {
        var console = GetEntity(ev.Console);
        if (!TryComp<AdditionalShuttleControlComponent>(console, out var consoleComponent))
            return;

        consoleComponent.LastRotateToPoint = null;
        foreach (var gunRecord in consoleComponent.ShuttleGunRecords)
        {
            var gun = GetEntity(gunRecord.Key);
            if (TerminatingOrDeleted(gun))
                continue;

            _xform.SetWorldRotation(gun, gunRecord.Value);
        }
    }

    private void OnRequestRotateGunToPoint(RequestRotateGunToPoint ev)
    {
        var console = GetEntity(ev.Console);
        if (!TryComp<AdditionalShuttleControlComponent>(console, out var consoleComponent))
            return;

        consoleComponent.LastRotateToPoint = ev.Coords;
        Dirty(console, consoleComponent);

        RotateToPoint((console, consoleComponent), ev.Coords);
    }

    private void OnRequestShuttleGunsFire(RequestShuttleGunsFire ev)
    {
        var console = GetEntity(ev.Console);
        if (!HasComp<AdditionalShuttleControlComponent>(console))
            return;

        var devices = _deviceList.GetAllDevices(console);
        foreach (var device in devices)
        {
            if (!CanShoot(device, out _))
                continue;

            if (!TryComp<DeviceNetworkComponent>(device, out var deviceNetworkDevice))
                continue;

            var payload = new NetworkPayload
            {
                [SharedDeviceLinkSystem.InvokedPort] = Trigger,
            };

            _deviceNetwork.QueuePacket(console, deviceNetworkDevice.Address, payload, deviceNetworkDevice.ReceiveFrequency);
        }
    }

    private void OnDeviceListUpdate(Entity<AdditionalShuttleControlComponent> ent, ref DeviceListUpdateEvent args)
    {
        ent.Comp.ShuttleGunRecords.Clear();

        foreach (var device in args.Devices)
        {
            if (HasComp<GunComponent>(device))
            {
                AddGunToRecords(ent, device);
                continue;
            }

            _deviceList.DeleteDeviceFromList(ent.Owner, device);
        }

        Dirty(ent);
    }

    private void RotateToPoint(Entity<AdditionalShuttleControlComponent> console, MapCoordinates coords)
    {
        var deviceList = _deviceList.GetAllDevices(console);
        foreach (var gun in deviceList)
        {
            var gunWorldPos = _xform.GetWorldPosition(gun);
            var direction = coords.Position - gunWorldPos;
            if (direction.LengthSquared() < 0.01f)
                continue;

            var angle = direction.ToWorldAngle();
            _xform.SetWorldRotation(gun, angle);
        }

        Dirty(console);
    }

    private void AddGunToRecords(Entity<AdditionalShuttleControlComponent> console, EntityUid gun)
    {
        var netGun = GetNetEntity(gun);
        if (console.Comp.ShuttleGunRecords.ContainsKey(netGun))
            return;

        var currentRotation = _xform.GetWorldRotation(gun);
        console.Comp.ShuttleGunRecords.Add(netGun, currentRotation);
    }

    public bool CanShoot(EntityUid gun, out float hitDistance)
    {
        hitDistance = SharedRadarConsoleSystem.DefaultMaxRange;
        if (!TryComp<GunComponent>(gun, out var gunComp))
            return false;

        var xform = Transform(gun);
        if (xform.GridUid == null)
            return true;

        if (!_gun.CanShoot(gunComp))
            return false;

        var gunPos = _xform.GetWorldPosition(xform);
        var gunRot = _xform.GetWorldRotation(xform);
        var direction = gunRot.ToWorldVec();

        var collisionMask = (int) CollisionGroup.BulletImpassable; // can I be sure that all projectiles ammo used this collision group?

        if (TryComp<HitscanBatteryAmmoProviderComponent>(gun, out var hitscan) &&
            _proto.TryIndex<HitscanPrototype>(hitscan.Prototype, out var hitscanPrototype))
        {
            collisionMask = hitscanPrototype.CollisionMask;
            hitDistance = hitscanPrototype.MaxLength;
        }

        var ray = new CollisionRay(gunPos, direction, collisionMask);
        var results = _physics.IntersectRay(xform.MapID, ray, ignoredEnt: gun);

        foreach (var result in results)
        {
            var hitXform = Transform(result.HitEntity);
            if (hitXform.GridUid != xform.GridUid)
                continue;

            hitDistance = result.Distance;
            return false;
        }

        return true;
    }
}

[Serializable, NetSerializable]
public sealed class RequestRotateGunToPoint(NetEntity console, MapCoordinates coords) : EntityEventArgs
{
    public NetEntity Console = console;
    public MapCoordinates Coords = coords;
}

[Serializable, NetSerializable]
public sealed class RequestShuttleGunsFire(NetEntity console) : EntityEventArgs
{
    public NetEntity Console = console;
}

[Serializable, NetSerializable]
public sealed class RequestResetRotateShuttleGuns(NetEntity console) : EntityEventArgs
{
    public NetEntity Console = console;
}
