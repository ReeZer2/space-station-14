using System.Linq;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.Trigger.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.AdditionalShuttleControl;

public sealed class AdditionalShuttleControlSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedDeviceListSystem _deviceList = default!;

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
            var gun = GetEntity(gunRecord.ShuttleGun);
            if (TerminatingOrDeleted(gun))
                continue;

            _xform.SetWorldRotation(gun, gunRecord.ShuttleGunRotation);
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

        var deviceNetwork = EntityManager.System<SharedDeviceNetworkSystem>();
        var devices = _deviceList.GetAllDevices(console);
        foreach (var device in devices)
        {
            if (!TryComp<DeviceNetworkComponent>(device, out var deviceNetworkDevice))
                continue;

            var payload = new NetworkPayload
            {
                [SharedDeviceLinkSystem.InvokedPort] = TriggerSystem.DefaultTriggerKey,
            };

            deviceNetwork.QueuePacket(console, deviceNetworkDevice.Address, payload, deviceNetworkDevice.ReceiveFrequency);
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
        if (console.Comp.ShuttleGunRecords.Any(r => r.ShuttleGun == netGun))
            return;

        var currentRotation = _xform.GetWorldRotation(gun);
        var newRecord = new AdditionalShuttleGunRecord(netGun, currentRotation);
        console.Comp.ShuttleGunRecords.Add(newRecord);
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
