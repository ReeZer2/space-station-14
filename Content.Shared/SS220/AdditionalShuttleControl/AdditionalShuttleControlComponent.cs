using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.AdditionalShuttleControl;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AdditionalShuttleControlComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public MapCoordinates? LastRotateToPoint;

    [DataField]
    [AutoNetworkedField]
    public float LastRotateToPointRadius = 1.5f;

    [DataField]
    [AutoNetworkedField]
    public float GunRadius = 1f;

    [DataField]
    [AutoNetworkedField]
    public HashSet<AdditionalShuttleGunRecord> ShuttleGunRecords = new();
}

[Serializable, NetSerializable]
public record struct AdditionalShuttleGunRecord(NetEntity ShuttleGun, Angle ShuttleGunRotation);
