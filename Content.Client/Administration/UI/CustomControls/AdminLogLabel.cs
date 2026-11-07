using Content.Client.UserInterface.Controls;
using Content.Shared.Administration.Logs;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client.Administration.UI.CustomControls;

public sealed class AdminLogLabel : RichTextLabel
{
<<<<<<< HEAD
    //SS220-make-copy-for-logs-begin
    public bool SelectHoverMode = false;

    private readonly Color _selectedTextColor = Color.FromHex("#b3f1abff");
    // null used for base RichTextLabel color
    private readonly Color? _textColor = null;
    //SS220-make-copy-for-logs-begin

    public AdminLogLabel(ref SharedAdminLog log, HSeparator separator)
=======
    public AdminLogLabel(ref SharedAdminLog log, Separator separator)
>>>>>>> wizards/stable
    {
        Log = log;
        Separator = separator;

<<<<<<< HEAD
        //SS220-make-copy-for-logs-begin
        MouseFilter = MouseFilterMode.Pass;
        OnKeyBindDown += (args) =>
        {
            if (args.Function == EngineKeyFunctions.UIClick)
                MarkedForCopying = !_markedForCopying;

            if (args.Function == EngineKeyFunctions.UIRightClick)
                MarkedForCopying = true;
        };

        OnMouseEntered += (_) =>
        {
            if (SelectHoverMode)
                MarkedForCopying = true;
        };
        //SS220-make-copy-for-logs-end

        SetMessage($"{log.Date:HH:mm:ss}: {log.Message}", defaultColor: _textColor); //SS220-make-copy-for-logs
=======
        var localTime = log.Date.ToLocalTime();

        SetMessage($"{localTime:HH:mm:ss}: {log.Message}");
>>>>>>> wizards/stable
        OnVisibilityChanged += VisibilityChanged;
    }

    public new SharedAdminLog Log { get; }

    public Separator Separator { get; }

    // SS220-make-copy-for-logs-begin
    private bool _markedForCopying = false;
    public bool MarkedForCopying
    {
        // two purposes
        // 1. save marked log in case of mistake
        // 2. make copying intuitive
        get => Visible && _markedForCopying;
        set
        {
            if (value)
                SetMessage($"{Log.Date:HH:mm:ss}: {Log.Message}", defaultColor: _selectedTextColor);
            else
                SetMessage($"{Log.Date:HH:mm:ss}: {Log.Message}", defaultColor: _textColor);

            _markedForCopying = value;
        }
    }
    // SS220-make-copy-for-logs-end

    private void VisibilityChanged(Control control)
    {
        Separator.Visible = Visible;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        OnVisibilityChanged -= VisibilityChanged;
    }
}
