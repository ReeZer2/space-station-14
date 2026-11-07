using System.Text.RegularExpressions;
using Content.Shared.Chat;

namespace Content.Shared.Speech.EntitySystems;

public sealed partial class AccentSystem : EntitySystem
{
    public static readonly Regex SentenceRegex = new(@"(?<=[\.!\?‽])(?![\.!\?‽])", RegexOptions.Compiled);

    [SubscribeLocalEvent]
    private void AccentHandler(TransformSpeechEvent args)
    {
        if (args.Cancelled)
            return;

        // SS220 Mindslave-stop-word begin
        var beforeAccentGetEvent = new BeforeAccentGetEvent(args.Sender, args.Message);
        RaiseLocalEvent(args.Sender, beforeAccentGetEvent, true);
        var accentEvent = new AccentGetEvent(args.Sender, beforeAccentGetEvent.Message);
        // var accentEvent = new AccentGetEvent(args.Sender, args.Message);
        // SS220 Mindslave-stop-word end

        RaiseLocalEvent(args.Sender, ref accentEvent);
        args.Message = accentEvent.Message;
    }
}
