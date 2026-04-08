// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.SS220.MindExtension;
using Robust.Shared.Network;

namespace Content.Server.SS220.MindExtension;

public partial class MindExtensionSystem //MindTransferSystem
{
    public void TransferExtension(EntityUid? oldEntity, EntityUid? newEntity, NetUserId? player)
    {
        if (player is null)
            return;

        if (!TryGetExtension(player.Value, out var data))
        {
            if (newEntity == null)
                return;

            data = GetOrCreateExtension(player.Value);
        }

        if (oldEntity != null && HasComp<MindExtensionContainerComponent>(oldEntity))
        {
            ChangeOrAddTrailPoint(data, oldEntity.Value, CheckEntityAbandoned(oldEntity.Value));
            RemComp<MindExtensionContainerComponent>(oldEntity.Value);
        }

        if (newEntity == null)
        {
            data.CurrentBody = null;
            return;
        }

        data.CurrentBody = newEntity;
        ChangeOrAddTrailPoint(data, newEntity.Value, false);
        SetRespawnTimer(data, newEntity.Value, player.Value);

        EnsureComp<MindExtensionContainerComponent>(newEntity.Value);
    }

    public void MarkAsNotAbandoned(EntityUid invoker, NetUserId player)
    {
        if (TryGetExtension(player, out var data))
            ChangeOrAddTrailPoint(data, invoker, false);
    }
}
