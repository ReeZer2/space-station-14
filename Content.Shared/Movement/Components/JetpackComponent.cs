using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Movement.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JetpackComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? JetpackUser;

    [DataField]
    public float MoleUsage = 0.012f;

    [DataField]
    public EntProtoId ToggleAction = "ActionToggleJetpack";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField]
    public float Acceleration = 1f;

    [DataField]
    public float Friction = 0.25f; // same as off-grid friction

    [DataField]
    public float WeightlessModifier = 1.2f;

<<<<<<< HEAD
    //ss220 fix activated jetpack in container start
    [DataField, AutoNetworkedField]
    public EntityUid? User;
    //ss220 fix activated jetpack in container end
=======
    /// <summary>
    /// Interval in seconds between gas consumption while the jetpack is active.
    /// </summary>
    [DataField]
    public float GasUsageInterval = 0.3f;
>>>>>>> wizards/stable
}
