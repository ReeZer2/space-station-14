using Content.Server.EUI;
<<<<<<< HEAD
using Content.Server.Ghost.Components;
using Content.Server.Paper;
=======
using Content.Shared.DeviceNetwork;
>>>>>>> wizards/stable
using Content.Shared.Eui;
using Content.Shared.Fax.Components;
using Content.Shared.Fax;
using Content.Shared.Follower;
using Content.Shared.Paper;
using Content.Shared.SS220.Photocopier;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Ghost.Components;

namespace Content.Server.Fax.AdminUI;

public sealed partial class AdminFaxEui : BaseEui
{
<<<<<<< HEAD
    [Dependency] private readonly IEntityManager _entityManager = default!;

=======
    [Dependency] private IEntityManager _entityManager = default!;
>>>>>>> wizards/stable
    private readonly FaxSystem _faxSystem;
    private readonly FollowerSystem _followerSystem;

    public AdminFaxEui()
    {
        IoCManager.InjectDependencies(this);
        _faxSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<FaxSystem>();
        _followerSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<FollowerSystem>();
    }

    public override void Opened()
    {
        StateDirty();
    }

    public override AdminFaxEuiState GetNewState()
    {
        var faxes = _entityManager.EntityQueryEnumerator<FaxMachineComponent, DeviceNetworkComponent>();
        var entries = new List<AdminFaxEntry>();
        while (faxes.MoveNext(out var uid, out var fax, out var device))
        {
            entries.Add(new AdminFaxEntry(_entityManager.GetNetEntity(uid), fax.FaxName, DeviceLocalizationHelpers.GetAddressFromId(device)));
        }
        return new AdminFaxEuiState(entries);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case AdminFaxEuiMsg.Follow followData:
                {
                    if (Player.AttachedEntity == null ||
                        !_entityManager.HasComponent<GhostComponent>(Player.AttachedEntity.Value))
                        return;

                    _followerSystem.StartFollowingEntity(Player.AttachedEntity.Value, _entityManager.GetEntity(followData.TargetFax));
                    break;
                }
            case AdminFaxEuiMsg.Send sendData:
                {
                    // SS220 Photocopy begin
                    //var printout = new FaxPrintout(sendData.Content, sendData.Title, null, null, sendData.StampState,
                    //        new() { new StampDisplayInfo { StampedName = sendData.From, StampedColor = sendData.StampColor } },
                    //        locked: sendData.Locked);
                    //_faxSystem.Receive(_entityManager.GetEntity(sendData.Target), printout);
                    //break;

                    var dataToCopy = new Dictionary<Type, IPhotocopiedComponentData>();
                    var paperDataToCopy = new PaperPhotocopiedData()
                    {
                        Content = sendData.Content,
                        StampState = sendData.StampState,
                        EditingDisabled = sendData.Locked,
                        StampedBy = new() { new StampDisplayInfo { StampedName = sendData.From, StampedColor = sendData.StampColor } }
                    };
                    dataToCopy.Add(typeof(PaperComponent), paperDataToCopy);

                    var metaData = new PhotocopyableMetaData()
                    {
                        EntityName = sendData.Title,
                        PrototypeId = "PaperNtFormCc"
                    };

                    var printout = new PhotocopyableFaxPrintout(dataToCopy, metaData);
                    _faxSystem.Receive(_entityManager.GetEntity(sendData.Target), printout);
                    break;
                    // SS220 Photocopy end
                }
        }
    }
}
