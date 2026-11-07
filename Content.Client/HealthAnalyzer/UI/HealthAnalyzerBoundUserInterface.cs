<<<<<<< HEAD
﻿using Content.Shared.MedicalScanner;
using Content.Shared.SS220.MedicalScanner;
=======
using Content.Shared.Medical.HealthAnalyzer;
using Content.Shared.MedicalScanner;
>>>>>>> wizards/stable
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.HealthAnalyzer.UI;

[UsedImplicitly]
public sealed partial class HealthAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private HealthAnalyzerWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<HealthAnalyzerWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
    }

    /// <summary>
    /// This will update the UI to reflect the newest health changes of the scanned entity.
    /// This gets called in the <see cref="HealthAnalyzerSystem"/> by SetUIState().
    /// </summary>
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_window == null || state is not HealthAnalyzerUiState cast)
            return;

<<<<<<< HEAD
            _window = this.CreateWindow<HealthAnalyzerWindow>();

            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
            _window.HealthAnalyzer.PrintButton.OnPressed += _ => SendMessage(new HealthAnalyzerPrintMessage()); // SS220-health-analyzer-report
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not HealthAnalyzerScannedUserMessage cast)
                return;

            _window.Populate(cast);
        }
=======
        _window.Populate(cast);
>>>>>>> wizards/stable
    }
}
