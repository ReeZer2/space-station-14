using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Stack;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking.Components;
using Content.Shared.Stacks;
using Robust.Server.GameObjects;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Spawners.EntitySystems;

// TODO: This whole system is a mess. A lot of this should be marked obsolete.
// TODO: It should probably use interfaces with entity tables *if* more than one component is needed.
// TODO: Remove the TransformSystem Dependency when engine SpawnAtPosition EntityCoordinates override is fixed.
/// <summary>
/// A system for spawning random or conditional entities, either on initializing spawner entities, or on adding GameRules.
/// </summary>
/// <seealso cref="ConditionalSpawnerComponent"/>
/// <seealso cref="RandomSpawnerComponent"/>
/// <seealso cref="EntityTableSpawnerComponent"/>
public sealed partial class ConditionalSpawnerSystem : EntitySystem
{
    [Dependency] private IRobustRandom _robustRandom = default!;
    [Dependency] private ServerGameTicker _ticker = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private StackSystem _stack = default!;
    [Dependency] private TransformSystem _xform = default!;

    [SubscribeLocalEvent]
    private void OnCondSpawnMapInit(Entity<ConditionalSpawnerComponent> ent, ref MapInitEvent args)
    {
<<<<<<< HEAD
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly EntityTableSystem _entityTable = default!;
// SS220-prototype-validation-begin
#if !FULL_RELEASE
        [Dependency] private IPrototypeManager _prototype = default!;
#endif
// SS220-prototype-validation-end
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GameRuleStartedEvent>(OnRuleStarted);
            SubscribeLocalEvent<ConditionalSpawnerComponent, MapInitEvent>(OnCondSpawnMapInit);
            SubscribeLocalEvent<RandomSpawnerComponent, MapInitEvent>(OnRandSpawnMapInit);
            SubscribeLocalEvent<EntityTableSpawnerComponent, MapInitEvent>(OnEntityTableSpawnMapInit);
        }

        private void OnCondSpawnMapInit(EntityUid uid, ConditionalSpawnerComponent component, MapInitEvent args)
        {
            TrySpawn(uid, component);
        }

        private void OnRandSpawnMapInit(EntityUid uid, RandomSpawnerComponent component, MapInitEvent args)
        {
// SS220-prototype-validation-begin
#if !FULL_RELEASE
            foreach (var possibleSpawnId in component.Prototypes)
            {
                if (!_prototype.HasIndex<EntityPrototype>(possibleSpawnId))
                    Log.Error($"Attempted to spawn an entity with an invalid prototype: {possibleSpawnId} in random spawner component of {ToPrettyString(uid)}");
            }

            foreach (var possibleSpawnId in component.RarePrototypes)
            {
                if (!_prototype.HasIndex<EntityPrototype>(possibleSpawnId))
                    Log.Error($"Attempted to spawn an entity with an invalid prototype: {possibleSpawnId} in random spawner component of {ToPrettyString(uid)}");
            }
#endif
// SS220-prototype-validation-end
            Spawn(uid, component);
            if (component.DeleteSpawnerAfterSpawn)
                QueueDel(uid);
        }

        private void OnEntityTableSpawnMapInit(Entity<EntityTableSpawnerComponent> ent, ref MapInitEvent args)
        {
            Spawn(ent);
            if (ent.Comp.DeleteSpawnerAfterSpawn && !TerminatingOrDeleted(ent) && Exists(ent))
                QueueDel(ent);
        }

        private void OnRuleStarted(ref GameRuleStartedEvent args)
        {
            var query = EntityQueryEnumerator<ConditionalSpawnerComponent>();
            while (query.MoveNext(out var uid, out var spawner))
            {
                RuleStarted(uid, spawner, args);
            }
        }

        public void RuleStarted(EntityUid uid, ConditionalSpawnerComponent component, GameRuleStartedEvent obj)
        {
            if (component.GameRules.Contains(obj.RuleId))
                Spawn(uid, component);
        }

        private void TrySpawn(EntityUid uid, ConditionalSpawnerComponent component)
        {
            if (component.GameRules.Count == 0)
            {
                Spawn(uid, component);
                return;
            }

            foreach (var rule in component.GameRules)
            {
                if (!_ticker.IsGameRuleActive(rule))
                    continue;
                Spawn(uid, component);
                return;
            }
        }

        private void Spawn(EntityUid uid, ConditionalSpawnerComponent component)
        {
            if (component.Chance != 1.0f && !_robustRandom.Prob(component.Chance))
                return;

            if (component.Prototypes.Count == 0)
            {
                Log.Warning($"Prototype list in ConditionalSpawnComponent is empty! Entity: {ToPrettyString(uid)}");
                return;
            }

            if (!Deleted(uid))
                Spawn(_robustRandom.Pick(component.Prototypes), Transform(uid).Coordinates);
        }

        private void Spawn(EntityUid uid, RandomSpawnerComponent component)
        {
            if (component.RarePrototypes.Count > 0 && (component.RareChance == 1.0f || _robustRandom.Prob(component.RareChance)))
            {
                Spawn(_robustRandom.Pick(component.RarePrototypes), Transform(uid).Coordinates);
                return;
            }

            if (component.Chance != 1.0f && !_robustRandom.Prob(component.Chance))
                return;

            if (component.Prototypes.Count == 0)
            {
                Log.Warning($"Prototype list in RandomSpawnerComponent is empty! Entity: {ToPrettyString(uid)}");
                return;
            }

            if (Deleted(uid))
                return;

            var offset = component.Offset;
            var xOffset = _robustRandom.NextFloat(-offset, offset);
            var yOffset = _robustRandom.NextFloat(-offset, offset);

            var coordinates = Transform(uid).Coordinates.Offset(new Vector2(xOffset, yOffset));

            Spawn(_robustRandom.Pick(component.Prototypes), coordinates);
        }

        private void Spawn(Entity<EntityTableSpawnerComponent> ent)
        {
            if (TerminatingOrDeleted(ent) || !Exists(ent))
                return;

            var coords = Transform(ent).Coordinates;
            var offset = ent.Comp.Offset;

            var spawns = _entityTable.GetSpawns(ent.Comp.Table);
            foreach (var proto in spawns)
            {
                var xOffset = _robustRandom.NextFloat(-offset, offset);
                var yOffset = _robustRandom.NextFloat(-offset, offset);
                var trueCoords = coords.Offset(new Vector2(xOffset, yOffset));

                SpawnAttachedTo(proto, trueCoords);
            }
=======
        TrySpawn(ent, ent);
    }

    [SubscribeLocalEvent]
    private void OnRandSpawnMapInit(Entity<RandomSpawnerComponent> ent, ref MapInitEvent args)
    {
        Spawn(ent, ent);
        if (ent.Comp.DeleteSpawnerAfterSpawn)
            QueueDel(ent);
    }

    [SubscribeLocalEvent]
    private void OnEntityTableSpawnMapInit(Entity<EntityTableSpawnerComponent> ent, ref MapInitEvent args)
    {
        Spawn(ent);
        if (ent.Comp.DeleteSpawnerAfterSpawn && !TerminatingOrDeleted(ent) && Exists(ent))
            QueueDel(ent);
    }

    [SubscribeLocalEvent]
    private void OnRuleStarted(ref GameRuleStartedEvent args)
    {
        var query = EntityQueryEnumerator<ConditionalSpawnerComponent>();
        while (query.MoveNext(out var uid, out var spawner))
        {
            RuleStarted(uid, spawner, args);
>>>>>>> wizards/stable
        }
    }

    public void RuleStarted(EntityUid uid, ConditionalSpawnerComponent component, GameRuleStartedEvent args)
    {
        if (args.RuleId is not { } id)
            return;

        if (component.GameRules.Contains(id))
            Spawn(uid, component);
    }

    private void TrySpawn(EntityUid uid, ConditionalSpawnerComponent component)
    {
        if (component.GameRules.Count == 0)
        {
            Spawn(uid, component);
            return;
        }

        foreach (var rule in component.GameRules)
        {
            if (!_ticker.IsGameRuleActive(rule))
                continue;
            Spawn(uid, component);
            return;
        }
    }

    private void Spawn(EntityUid uid, ConditionalSpawnerComponent component)
    {
        if (component.Chance != 1.0f && !_robustRandom.Prob(component.Chance))
            return;

        if (component.Prototypes.Count == 0)
        {
            Log.Warning($"Prototype list in ConditionalSpawnComponent is empty! Entity: {ToPrettyString(uid)}");
            return;
        }

        if (Deleted(uid))
            return;

        var xform = Transform(uid);
        var coords = _xform.GetMapCoordinates(uid, xform);
        var rotation = _xform.GetWorldRotation(xform);

        var toSpawn = _robustRandom.Pick(component.Prototypes);
        Spawn(toSpawn, coords, rotation: rotation);
    }

    private void Spawn(EntityUid uid, RandomSpawnerComponent component)
    {
        if (Deleted(uid))
            return;

        if (GetPrototype((uid, component)) is not { } proto)
            return;

        var xform = Transform(uid);
        var coords = _xform.GetMapCoordinates(uid, xform);
        var coordinates = GetRandomOffset(coords, component.Offset);
        var rotation = _xform.GetWorldRotation(xform);

        Spawn(proto, coordinates, rotation: rotation);
    }

    private void Spawn(Entity<EntityTableSpawnerComponent> ent)
    {
        if (TerminatingOrDeleted(ent) || !Exists(ent))
            return;

        var xform = Transform(ent);
        var coords = _xform.GetMapCoordinates(ent, xform);
        var rotation = _xform.GetWorldRotation(xform);

        EntityTableSpawnerComponent comp = ent;
        var spawns = _entityTable.GetSpawns(comp.Table);
        if (comp.AutoStack)
        {
            SpawnStackedWhenPossible(spawns, ent, coords, comp.Offset, rotation);
        }
        else
        {
            SpawnAtRandomOffset(spawns, coords, comp.Offset, rotation);
        }
    }

    private void SpawnStackedWhenPossible(IEnumerable<EntProtoId> spawns,
        Entity<EntityTableSpawnerComponent> ent,
        MapCoordinates coords,
        float offset,
        Angle rotation)
    {
        Dictionary<ProtoId<StackPrototype>, (EntProtoId Proto, int Count)> prototypeStacks = new();
        ValueList<EntProtoId> nonStackable = [];
        foreach (var protoId in spawns)
        {
            var prototype = ProtoMan.Index(protoId);
            if (!prototype.TryComp<StackComponent>(out var stack, Factory))
            {
                nonStackable.Add(protoId);
                continue;
            }

            prototypeStacks[stack.StackTypeId] = prototypeStacks.TryGetValue(stack.StackTypeId, out var found)
                ? (protoId, found.Count + 1)
                : (protoId, 1);
        }

        SpawnAtRandomOffset(nonStackable, coords, offset, rotation);

        foreach (var (protoId, count) in prototypeStacks.Values)
        {
            var trueCoords = GetRandomOffset(coords, offset);
            var entCoordinates = _xform.ToCoordinates((ent, null), trueCoords);
            _stack.SpawnMultipleAtPosition(protoId, count, entCoordinates);
        }
    }

    private void SpawnAtRandomOffset(IEnumerable<EntProtoId> spawns, MapCoordinates coords, float offset, Angle rotation)
    {
        foreach (var proto in spawns)
        {
            SpawnAtRandomOffset(proto, coords, offset, rotation);
        }
    }

    private EntityUid SpawnAtRandomOffset(EntProtoId proto, MapCoordinates coords, float offset, Angle rotation)
    {
        var trueCoords = GetRandomOffset(coords, offset);

        return Spawn(proto, trueCoords, rotation: rotation);
    }

    private EntProtoId? GetPrototype(Entity<RandomSpawnerComponent> spawner)
    {
        if (GetPrototypes(spawner) is not { } list)
            return null;

        return _robustRandom.Pick(list);
    }

    private List<EntProtoId>? GetPrototypes(Entity<RandomSpawnerComponent> spawner)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (spawner.Comp.RarePrototypes.Count > 0 &&
            (spawner.Comp.RareChance == 1.0f || _robustRandom.Prob(spawner.Comp.RareChance)))
        {
            return spawner.Comp.RarePrototypes;
        }

        if (spawner.Comp.Prototypes.Count == 0)
        {
            Log.Warning($"Prototype list in RandomSpawnerComponent is empty! Entity: {ToPrettyString(spawner)}");
            return null;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (spawner.Comp.Chance == 1.0f || _robustRandom.Prob(spawner.Comp.Chance))
        {
            return spawner.Comp.Prototypes;
        }

        return null;
    }

    private MapCoordinates GetRandomOffset(MapCoordinates coords, float offset)
    {
        var vOffset = _robustRandom.NextVector2Box(offset, offset);
        return coords.Offset(vOffset);
    }
}
