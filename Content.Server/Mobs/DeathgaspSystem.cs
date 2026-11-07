using Content.Server.Chat.Systems;
<<<<<<< HEAD
using Content.Server.Speech.Muting;
=======
>>>>>>> wizards/stable
using Content.Shared.Mobs;
using Content.Server.EUI;
using Content.Server.SS220.Deathreminder;
using Robust.Server.Player;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffectNew;

namespace Content.Server.Mobs;

<<<<<<< HEAD

/// <see cref="DeathgaspComponent"/>
public sealed class DeathgaspSystem: EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EuiManager _euiManager = null!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeathgaspComponent, MobStateChangedEvent>(OnMobStateChanged);
    }


    private void OnMobStateChanged(EntityUid uid, DeathgaspComponent component, MobStateChangedEvent args)
=======
/// <summary>
/// A system that handles death gasps, an emote a character makes when they die.
/// </summary>
/// <seealso cref="DeathgaspComponent"/>
public sealed partial class DeathgaspSystem : SharedDeathgaspSystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    [SubscribeLocalEvent]
    private void OnMobStateChanged(Entity<DeathgaspComponent> ent, ref MobStateChangedEvent args)
>>>>>>> wizards/stable
    {

        if (args.NewMobState == MobState.Dead)
        {
            if (TryComp<MindContainerComponent>(uid, out var mindContainer))
            {
                if (mindContainer.Mind is { } mind)
                {
                    if (TryComp<MindComponent>(mind, out var mindComp))
                    {
                        if (mindComp.UserId != null && _playerManager.TryGetSessionById(mindComp.UserId.Value, out var client))
                        {
                            _euiManager.OpenEui(new DeathReminderEui(), client);
                        }
                    }
                }
            }
        }

        // don't deathgasp if they arent going straight from crit to dead
        if (args.NewMobState != MobState.Dead || args.OldMobState != MobState.Critical)
            return;

        Deathgasp(ent, ent.Comp);
    }

    /// <summary>
    ///     Causes an entity to perform their deathgasp emote, if they have one.
    /// </summary>
    public override bool Deathgasp(EntityUid uid, DeathgaspComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (_statusEffects.HasEffectComp<MutedStatusEffectComponent>(uid))
            return false;

        _chat.TryEmoteWithChat(uid, component.Prototype, ignoreActionBlocker: true);

        return true;
    }
}
