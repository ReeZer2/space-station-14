using Content.Client.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Client.RoundEnd;

[UsedImplicitly]
public sealed partial class RoundEndSummaryUIController : UIController,
    IOnSystemLoaded<ClientGameTicker>
{
<<<<<<< HEAD
    // [Dependency] private readonly IInputManager _input = default!; // SS220 Round End Titles
=======
    [Dependency] private IInputManager _input = default!;
>>>>>>> wizards/stable

    private RoundEndSummaryWindow? _window;

    private void ToggleScoreboardWindow(ICommonSession? session = null)
    {
        if (_window == null)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.OpenCenteredRight();
            _window.MoveToFront();
        }
    }

    public void OpenRoundEndSummaryWindow(RoundEndMessageEvent message)
    {
        // Don't open duplicate windows (mainly for replays).
        if (_window?.RoundId == message.RoundId)
            return;

        _window = new RoundEndSummaryWindow(message.GamemodeTitle, message.RoundEndText,
            message.RoundDuration, message.RoundId, message.AllPlayersEndInfo);
    }

    public void OnSystemLoaded(ClientGameTicker system)
    {
        // SS220 Round End Titles (this should not override our re-implementation's bind)
        //_input.SetInputCommand(ContentKeyFunctions.ToggleRoundEndSummaryWindow,
        //    InputCmdHandler.FromDelegate(ToggleScoreboardWindow));
    }
}
