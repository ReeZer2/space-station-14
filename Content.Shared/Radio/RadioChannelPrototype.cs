<<<<<<< HEAD
using Content.Shared.FixedPoint;
using Content.Shared.SS220.TTS;
=======
using Content.Shared.DeviceNetwork;
>>>>>>> wizards/stable
using Robust.Shared.Prototypes;

namespace Content.Shared.Radio;

/// <summary>
/// Defines a radio channel and its transmission properties.
/// </summary>
[Prototype]
public sealed partial class RadioChannelPrototype : IHearableChannelPrototype //ss220 add telepathy mute for ghosts
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Human-readable name for the channel.
    /// </summary>
    [DataField]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    /// The localized display name of this channel.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    /// <summary>
    /// Single-character prefix to determine what channel a message should be sent to.
    /// </summary>
    [DataField("keycode")]
    public char KeyCode { get; private set; } = '\0';

<<<<<<< HEAD
    [DataField("frequency")]
    public int Frequency { get; private set; } = 0;

    // SS220-radio-headset-begin
    [DataField]
    public FixedPoint2 MinFrequency { get; private set; } = 0;

    [DataField]
    public FixedPoint2 MaxFrequency { get; private set; } = 0;

    /// <summary>
    /// Defines when to use frequency logic for this channel
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool FrequencyRadio = false;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public LocId FrequencyChanelName = "base-frequency-channel-name";
    // SS220-radio-headset-end

    [DataField("color")]
=======
    /// <summary>
    /// Color used to display the channel.
    /// </summary>
    [DataField]
>>>>>>> wizards/stable
    public Color Color { get; private set; } = Color.Lime;

    /// <summary>
    /// The device network frequency used by this channel.
    /// </summary>
    [DataField]
    public DeviceFrequency Frequency { get; private set; } = 1;

    /// <summary>
    /// Whether the channel can transmit across different stations without a telecommunications server.
    /// </summary>
<<<<<<< HEAD
    [DataField("longRange"), ViewVariables]
    public bool LongRange = false;

    //SS220-synd_key_stealth begin
    /// <summary>
    ///     Determines the visibility of the key during inspection
    /// </summary>
    [DataField("stealthChannel")]
    public bool? StealthChannel = false;
    //SS220-synd_key_stealth end
=======
    [DataField]
    public bool LongRange;
>>>>>>> wizards/stable
}
