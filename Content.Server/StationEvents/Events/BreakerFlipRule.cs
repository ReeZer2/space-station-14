using System.Linq;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.StationEvents.Components;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
<<<<<<< HEAD
using Content.Shared.SS220.Power.Components;
using Content.Shared.Station.Components;
=======
>>>>>>> wizards/stable
using JetBrains.Annotations;

namespace Content.Server.StationEvents.Events;

/// <summary>
/// Handler for events that turn off a handful of APCs on a station.
/// </summary>
/// <seealso cref="BreakerFlipRuleComponent"/>
[UsedImplicitly]
public sealed partial class BreakerFlipRule : StationEventSystem<BreakerFlipRuleComponent>
{
    [Dependency] private ApcSystem _apcSystem = default!;

    protected override void Added(Entity<BreakerFlipRuleComponent, GameRuleComponent> ent, ref GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(ent, out var stationEvent))
            return;

        var str = Loc.GetString("station-event-breaker-flip-announcement", ("data", Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}")));
        stationEvent.StartAnnouncement = str;

        base.Added(ent, ref args);
    }

    protected override void Started(Entity<BreakerFlipRuleComponent, GameRuleComponent> ent, ref GameRuleStartedEvent args)
    {
        base.Started(ent, ref args);

        var stationApcs = Station.GetEntitiesWithComponentOnStation<ApcComponent>(true).ToList();

<<<<<<< HEAD
        var stationApcs = new List<Entity<ApcComponent>>();
        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            //SS220-SM-fix-begin
            if (apc.MainBreakerEnabled && !HasComp<HighPriorityAPCComponent>(apcUid)
                && CompOrNull<StationMemberComponent>(xform.GridUid)?.Station == chosenStation) //SS220-SM-fix-end
            {
                stationApcs.Add((apcUid, apc));
            }
        }

        var toDisable = Math.Min(RobustRandom.Next(3, 7), stationApcs.Count);
=======
        var toDisable = Math.Min(ent.Comp1.AffectedQuantity.Next(RobustRandom), stationApcs.Count);
>>>>>>> wizards/stable
        if (toDisable == 0)
            return;

        RobustRandom.Shuffle(stationApcs);

        for (var i = 0; i < toDisable; i++)
        {
            var apc = stationApcs[i];
            _apcSystem.ApcToggleBreaker(apc, apc);

            var stateString = apc.Comp.MainBreakerEnabled ? "Enabled" : "Disabled";
            AdminLogManager.Add(LogType.ItemConfigure,
                LogImpact.Medium,
                $"Station event {ToPrettyString(ent):user} set the main breaker state of {ToPrettyString(apc):entity} to {stateString:state}");
        }
    }
}
