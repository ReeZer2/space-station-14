using Content.Shared.SS220.AdditionalShuttleControl;
using Content.Shared.SS220.Input;
using Robust.Client.Input;

namespace Content.Client.SS220.AdditionalShuttleControl;

public sealed class AdditionalShuttleControlSystem : SharedAdditionalShuttleControlSystem
{
    [Dependency] private readonly IInputManager _input = default!;

    private const string AdditionalShuttleContext = "additionalShuttle";
    private const string HumanContext = "human";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdditionalShuttleControlComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<AdditionalShuttleControlComponent, BoundUIClosedEvent>(OnBoundUIClosed);

        var additionalShuttle = _input.Contexts.New(AdditionalShuttleContext, HumanContext);
        additionalShuttle.AddFunction(KeyFunctions220.FireShuttle);
    }

    private void OnBoundUIOpened(Entity<AdditionalShuttleControlComponent> ent, ref BoundUIOpenedEvent args)
    {
        _input.Contexts.SetActiveContext(AdditionalShuttleContext);
    }

    private void OnBoundUIClosed(Entity<AdditionalShuttleControlComponent> ent, ref BoundUIClosedEvent args)
    {
        _input.Contexts.SetActiveContext(HumanContext);
    }
}
