using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.SS220.MimicChest;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MimicChestComponent : Component
{
    [ViewVariables, DataField, AutoNetworkedField]
    public bool IsBusy;

    [ViewVariables, DataField, AutoNetworkedField]
    public TimeSpan TimeStay;

    [AutoNetworkedField]
    [ViewVariables, DataField]
    public EntityUid? CapturedTarget;

    [ViewVariables, DataField, AutoNetworkedField]
    public TimeSpan CaptureStartTime;

    [ViewVariables, DataField, AutoNetworkedField]
    public TimeSpan LastPulseTime;

    [DataField("damage", required: true)]
    public DamageSpecifier Damage = default!;

}
