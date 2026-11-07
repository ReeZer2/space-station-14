<<<<<<< HEAD
using Content.Shared.Chemistry.Reagent;
=======
>>>>>>> wizards/stable
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.SS220.ChemicalAdaptation;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects.StatusEffects;

/// <summary>
/// Applies a given movement speed modifier status effect to this entity.
/// Duration is modified by scale.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class MovementSpeedModifierEntityEffectSystem : EntityEffectSystem<MovementSpeedModifierComponent, MovementSpeedModifier>
{
<<<<<<< HEAD
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly MovementModStatusSystem _movementModStatus = default!;
    [Dependency] private readonly ChemicalAdaptationSystem _chemicalAdaptation = default!;// SS220 add movement speed modification depending on the adaptation
=======
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private MovementModStatusSystem _movementModStatus = default!;
>>>>>>> wizards/stable

    protected override void Effect(Entity<MovementSpeedModifierComponent> entity, ref EntityEffectEvent<MovementSpeedModifier> args)
    {
        var duration = args.Effect.Time * args.Scale;
        var proto = args.Effect.EffectProto;
        var sprintMod = args.Effect.SprintSpeedModifier;
        var walkMod = args.Effect.WalkSpeedModifier;
        var delay = args.Effect.Delay;

        // SS220 add movement speed modification depending on the adaptation begin
        if (args.Effect.DependsOnAdaptation
            && args.Effect.Reagent != null
            && _chemicalAdaptation.TryGetMetabolized(entity, args.Effect.Reagent, out var metabolized)
            )
        {
            sprintMod += metabolized * args.Effect.MetabolizedModifier;
            walkMod += metabolized * args.Effect.MetabolizedModifier;
        }
        // SS220 add movement speed modification depending on the adaptation end

        switch (args.Effect.Type)
        {
            case StatusEffectMetabolismType.Update:
                _movementModStatus.TryUpdateMovementSpeedModDuration(
                    entity,
                    proto,
                    duration,
                    sprintMod,
                    walkMod,
                    delay);
                break;
            case StatusEffectMetabolismType.Add:
                if (duration != null)
                {
                    _movementModStatus.TryAddMovementSpeedModDuration(
                        entity,
                        proto,
                        duration.Value,
                        sprintMod,
                        walkMod,
                        delay);
                }
                else
                {
                    _movementModStatus.TryUpdateMovementSpeedModDuration(
                        entity,
                        proto,
                        duration,
                        sprintMod,
                        walkMod,
                        delay);
                }
                break;
            case StatusEffectMetabolismType.Remove:
                _status.TryRemoveTime(entity, args.Effect.EffectProto, duration);
                break;
            case StatusEffectMetabolismType.Set:
                _status.TrySetStatusEffectDuration(entity, proto, duration, delay);
                break;
        }
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class MovementSpeedModifier : BaseStatusEntityEffect<MovementSpeedModifier>
{
    /// <summary>
    /// How much the entities' walk speed is multiplied by.
    /// </summary>
    [DataField]
    public float WalkSpeedModifier = 1f;

    /// <summary>
    /// How much the entities' run speed is multiplied by.
    /// </summary>
    [DataField]
    public float SprintSpeedModifier = 1f;

    // SS220 add movement speed modification depending on the adaptation begin
    [DataField]
    public bool DependsOnAdaptation = false;

    [DataField]
    public float MetabolizedModifier = 1f;

    /// <summary>
    ///     Prototype of the reagent we're adding.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype>? Reagent = null;
    // SS220 add movement speed modification depending on the adaptation end

    /// <summary>
    /// Movement speed modifier prototype we're adding. Adding in case we ever want more than one prototype that boosts speed.
    /// </summary>
    [DataField]
    public EntProtoId EffectProto = MovementModStatusSystem.ReagentSpeed;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
    Time == null
        ? null // Not gonna make a whole new looc for something that shouldn't ever exist.
        : Loc.GetString("entity-effect-guidebook-movespeed-modifier",
            ("chance", Probability),
            ("sprintspeed", SprintSpeedModifier),
            ("time", Time.Value.TotalSeconds));
}
