// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Trap;

[Serializable, NetSerializable]
public enum TrapVisuals : byte
{
    Visual,
    Armed,
    Unarmed
}
