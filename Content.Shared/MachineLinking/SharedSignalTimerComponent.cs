using Robust.Shared.Serialization;

namespace Content.Shared.MachineLinking;

/// <summary>
/// Bound user interface key for the signal timer.
/// </summary>
[Serializable, NetSerializable]
public enum SignalTimerUiKey : byte
{
    Key,
}

/// <summary>
/// Message requesting a change to the signal timer's displayed text.
/// </summary>
[Serializable, NetSerializable]
public sealed class SignalTimerTextChangedMessage(string text) : BoundUserInterfaceMessage
{
<<<<<<< HEAD
    public string CurrentText;
    public string CurrentDescriptionText; //SS220-brig-timer-description
    public string CurrentDelayMinutes;
    public string CurrentDelaySeconds;
    public bool ShowText;
    public TimeSpan TriggerTime;
    public bool TimerStarted;
    public bool HasAccess;

    public SignalTimerBoundUserInterfaceState(string currentText,
        string currentDescriptionText,
        string currentDelayMinutes,
        string currentDelaySeconds,
        bool showText,
        TimeSpan triggerTime,
        bool timerStarted,
        bool hasAccess)
    {
        CurrentText = currentText;
        CurrentDescriptionText = currentDescriptionText; //SS220-brig-timer-description
        CurrentDelayMinutes = currentDelayMinutes;
        CurrentDelaySeconds = currentDelaySeconds;
        ShowText = showText;
        TriggerTime = triggerTime;
        TimerStarted = timerStarted;
        HasAccess = hasAccess;
    }
=======
    public readonly string Text = text;
>>>>>>> wizards/stable
}

/// <summary>
/// Message requesting a change to the signal timer's delay.
/// </summary>
[Serializable, NetSerializable]
public sealed class SignalTimerDelayChangedMessage(TimeSpan delay) : BoundUserInterfaceMessage
{
    public readonly TimeSpan Delay = delay;
}

<<<<<<< HEAD
//SS220-brig-timer-description begin
[Serializable, NetSerializable]
public sealed class SignalTimerDescriptionTextChangedMessage : BoundUserInterfaceMessage
{
    public string Text { get; }

    public SignalTimerDescriptionTextChangedMessage(string text)
    {
        Text = text;
    }
}
//SS220-brig-timer-description end

=======
/// <summary>
/// Message requesting that the signal timer start counting down.
/// </summary>
>>>>>>> wizards/stable
[Serializable, NetSerializable]
public sealed class SignalTimerStartMessage : BoundUserInterfaceMessage;
