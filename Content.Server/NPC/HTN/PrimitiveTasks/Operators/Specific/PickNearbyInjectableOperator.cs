using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Interaction;
using Content.Shared.Silicons.Bots;
<<<<<<< HEAD
using Content.Shared.Emag.Components;
using Content.Shared.Stealth.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes; //ss220 fix medibot
using Content.Shared.FixedPoint;
=======
>>>>>>> wizards/stable

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyInjectableOperator : HTNOperator
{
<<<<<<< HEAD
    [Dependency] private readonly IEntityManager _entManager = default!;

    private MedibotSystem _medibot = default!;
    private PathfindingSystem _pathfinding = default!;
    private DamageableSystem _damageable = default!;

    private EntityQuery<DamageableComponent> _damageQuery = default!;
    private EntityQuery<InjectableSolutionComponent> _injectQuery = default!;
    private EntityQuery<NPCRecentlyInjectedComponent> _recentlyInjected = default!;
    private EntityQuery<MobStateComponent> _mobState = default!;
    private EntityQuery<EmaggedComponent> _emaggedQuery = default!;
    private SharedContainerSystem _container = default!; //ss220 stealth inject fix
=======
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private MedibotSystem _medibot = default!;
    [Dependency] private PathfindingSystem _pathfinding = default!;
>>>>>>> wizards/stable

    [DataField("rangeKey")] public string RangeKey = NPCBlackboard.MedibotInjectRange;

    /// <summary>
    /// Target entity to inject
    /// </summary>
    [DataField("targetKey", required: true)]
    public string TargetKey = string.Empty;

    /// <summary>
    /// Target entitycoordinates to move to.
    /// </summary>
    [DataField("targetMoveKey", required: true)]
    public string TargetMoveKey = string.Empty;

<<<<<<< HEAD
    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _medibot = sysManager.GetEntitySystem<MedibotSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
        _damageable = sysManager.GetEntitySystem<DamageableSystem>();

        _damageQuery = _entManager.GetEntityQuery<DamageableComponent>();
        _injectQuery = _entManager.GetEntityQuery<InjectableSolutionComponent>();
        _recentlyInjected = _entManager.GetEntityQuery<NPCRecentlyInjectedComponent>();
        _mobState = _entManager.GetEntityQuery<MobStateComponent>();
        _emaggedQuery = _entManager.GetEntityQuery<EmaggedComponent>();
        _container = sysManager.GetEntitySystem<SharedContainerSystem>(); //ss220 npc stealth inject fix
    }

=======
>>>>>>> wizards/stable
    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out _, _entManager))
            return (false, null);

        if (!_entManager.TryGetComponent<MedibotComponent>(owner, out var medibot))
            return (false, null);

<<<<<<< HEAD
        var stealthQuery = _entManager.GetEntityQuery<StealthComponent>(); //ss220 medibot inject in stealth entity fix

=======
>>>>>>> wizards/stable
        if (!blackboard.TryGetValue<IEnumerable<KeyValuePair<EntityUid, float>>>("TargetList", out var patients, _entManager))
            return (false, null);

        foreach (var (entity, _) in patients)
        {
            if (!_medibot.CheckInjectable((owner, medibot), entity, true))
                continue;

            //Needed to make sure it doesn't sometimes stop right outside it's interaction range
            var pathRange = SharedInteractionSystem.InteractionRange - 1f;
            var path = await _pathfinding.GetPath(owner, entity, pathRange, cancelToken);

            if (path.Result == PathResult.NoPath)
                continue;

            return (true, new Dictionary<string, object>()
            {
<<<<<<< HEAD
                //ss220 medibot inject in stealth entity fix start
                if (_container.IsEntityInContainer(entity))
                {
                    continue;
                }
                if (stealthQuery.TryGetComponent(entity, out var stealthComponent) && stealthComponent.Enabled)
                {
                    continue;
                }
                //ss220 medibot inject in stealth entity fix end

                // no treating dead bodies
                if (!_medibot.TryGetTreatment(medibot, state.CurrentState, out var treatment))
                    continue;

                // Only go towards a target if the bot can actually help them or if the medibot is emagged
                // note: this and the actual injecting don't check for specific damage types so for example,
                // radiation damage will trigger injection but the tricordrazine won't heal it.
                if (!_emaggedQuery.HasComponent(entity) && _damageable.GetTotalDamage((entity, damage)) == FixedPoint2.Zero)
                    continue;
                //ss220 fix medibot end

                //Needed to make sure it doesn't sometimes stop right outside it's interaction range
                var pathRange = SharedInteractionSystem.InteractionRange - 1f;
                var path = await _pathfinding.GetPath(owner, entity, pathRange, cancelToken);

                if (path.Result == PathResult.NoPath)
                    continue;

                return (true, new Dictionary<string, object>()
                {
                    {TargetKey, entity},
                    {TargetMoveKey, _entManager.GetComponent<TransformComponent>(entity).Coordinates},
                    {NPCBlackboard.PathfindKey, path},
                });
            }
=======
                { TargetKey, entity },
                { TargetMoveKey, _entManager.GetComponent<TransformComponent>(entity).Coordinates },
                { NPCBlackboard.PathfindKey, path },
            });
>>>>>>> wizards/stable
        }

        return (false, null);
    }
}
