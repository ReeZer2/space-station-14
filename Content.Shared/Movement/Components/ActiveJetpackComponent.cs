using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Added to an enabled jetpack. Tracks server gas consumption timing.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedJetpackSystem))]
public sealed partial class ActiveJetpackComponent : Component
{
<<<<<<< HEAD
    public float EffectCooldown = 0.3f;

    public float MaxDistance = 0.7f;

    public EntityCoordinates LastCoordinates;

    public TimeSpan TargetTime = TimeSpan.Zero;

    // SS220-add-gas-usage-modifier-begin
    public float GasUsageModifier = 1f;
    // SS220-add-gas-usage-modifier-end
=======
    [ViewVariables]
    public TimeSpan NextGasUsage = TimeSpan.Zero;
>>>>>>> wizards/stable
}
