using System.Linq;
using Content.Server.Administration.Logs;
using Content.Shared.IdentityManagement;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Server.Power.Components;
using Content.Server.Stack;
using Content.Shared.ActionBlocker;
using Content.Shared.Construction;
using Content.Shared.Database;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Shared.Access.Systems;
using Content.Shared.Access.Components;

namespace Content.Server.Materials;

/// <summary>
/// This handles <see cref="SharedMaterialStorageSystem"/>
/// </summary>
public sealed partial class MaterialStorageSystem : SharedMaterialStorageSystem
{
<<<<<<< HEAD
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!; // SS220 Add access check for material eject
=======
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StackSystem _stackSystem = default!;
>>>>>>> wizards/stable

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MaterialStorageComponent, MachineDeconstructedEvent>(OnDeconstructed);

        SubscribeAllEvent<EjectMaterialMessage>(OnEjectMessage);
    }

    private void OnDeconstructed(EntityUid uid, MaterialStorageComponent component, MachineDeconstructedEvent args)
    {
        if (!component.DropOnDeconstruct)
            return;

        foreach (var (material, amount) in component.Storage)
        {
            SpawnMultipleFromMaterial(amount, material, Transform(uid).Coordinates);
        }
    }

    private void OnEjectMessage(EjectMaterialMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        var uid = GetEntity(msg.Entity);

        if (!TryComp<MaterialStorageComponent>(uid, out var component))
            return;

        if (!Exists(uid))
            return;

        if (!_actionBlocker.CanInteract(player, uid))
            return;

<<<<<<< HEAD
        // SS220 Add access check for material eject begin
        if (TryComp<AccessReaderComponent>(uid, out var accessReader) &&
            !_accessReader.IsAllowed(player, uid, accessReader))
        {
            _popup.PopupEntity(Loc.GetString("material-storage-eject-access-denied"), uid);
            _audio.PlayPvs(component.AccessDeniedSound, uid);
            return;
        }
        // SS220 Add access check for material eject end

        if (!component.CanEjectStoredMaterials || !_prototypeManager.TryIndex<MaterialPrototype>(msg.Material, out var material))
=======
        if (!component.CanEjectStoredMaterials || !ProtoMan.TryIndex<MaterialPrototype>(msg.Material, out var material))
>>>>>>> wizards/stable
            return;

        var volume = 0;

        if (material.StackEntity != null)
        {
            if (!ProtoMan.Index(material.StackEntity.Value).TryComp(out PhysicalCompositionComponent? composition, EntityManager.ComponentFactory))
                return;

            var volumePerSheet = composition.MaterialComposition.FirstOrDefault(kvp => kvp.Key == msg.Material).Value;
            var sheetsToExtract = Math.Min(msg.SheetsToExtract, _stackSystem.GetMaxCount(material.StackEntity.Value));

            volume = sheetsToExtract * volumePerSheet;
        }

        if (volume <= 0 || !TryChangeMaterialAmount(uid, msg.Material, -volume))
            return;

        var mats = SpawnMultipleFromMaterial(volume, material, Transform(uid).Coordinates, out _);
        foreach (var mat in mats.Where(mat => !TerminatingOrDeleted(mat)))
        {
            _stackSystem.TryMergeToContacts(mat);
        }

        // SS220 Add logging when user eject materials begin
        mats = mats.Where(mat => !TerminatingOrDeleted(mat)).ToList();
        foreach (var mat in mats)
        {
            TryComp<StackComponent>(mat, out var stack);
            var count = stack?.Count ?? 1;
            _adminLogger.Add(LogType.Action,
                LogImpact.Low,
                $"{ToPrettyString(player):player} ejected {count} {ToPrettyString(mat):ejected} from {ToPrettyString(uid):source}");
        }
        // SS220 Add logging when user eject materials end
    }

    public override bool TryInsertMaterialEntity(EntityUid user,
        EntityUid toInsert,
        EntityUid receiver,
        MaterialStorageComponent? storage = null,
        MaterialComponent? material = null,
        PhysicalCompositionComponent? composition = null)
    {
        if (!Resolve(receiver, ref storage) || !Resolve(toInsert, ref material, ref composition, false))
            return false;
        if (TryComp<ApcPowerReceiverComponent>(receiver, out var power) && !power.Powered)
            return false;
        if (!base.TryInsertMaterialEntity(user, toInsert, receiver, storage, material, composition))
            return false;
        _audio.PlayPvs(storage.InsertingSound, receiver);
        _popup.PopupEntity(Loc.GetString("machine-insert-item",
                ("user", Identity.Entity(user, EntityManager)),
                ("machine", receiver),
                ("item", toInsert)),
            receiver);
        QueueDel(toInsert);

        // Logging
        TryComp<StackComponent>(toInsert, out var stack);
        var count = stack?.Count ?? 1;
        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(user):player} inserted {count} {ToPrettyString(toInsert):inserted} into {ToPrettyString(receiver):receiver}");
        return true;
    }

    /// <inheritdoc/>
    [PublicAPI]
    public override List<EntityUid> SpawnMultipleFromMaterial(int amount, MaterialPrototype materialProto, EntityCoordinates coordinates, out int overflowMaterial)
    {
        overflowMaterial = 0;

        if (amount <= 0 || materialProto.StackEntity == null)
            return new List<EntityUid>();

        var entProto = ProtoMan.Index<EntityPrototype>(materialProto.StackEntity);
        if (!entProto.TryComp<PhysicalCompositionComponent>(out var composition, EntityManager.ComponentFactory))
            return new List<EntityUid>();

        var materialPerStack = composition.MaterialComposition[materialProto.ID];
        var amountToSpawn = amount / materialPerStack;
        overflowMaterial = amount - amountToSpawn * materialPerStack;

        if (amountToSpawn == 0)
            return new List<EntityUid>();

        return _stackSystem.SpawnMultipleAtPosition(materialProto.StackEntity.Value, amountToSpawn, coordinates);
    }

    /// <summary>
    /// Eject a material out of this storage. The internal counts are updated.
    /// Material that cannot be ejected stays in storage. (e.g. only have 50 but a sheet needs 100).
    /// </summary>
    /// <param name="entity">The entity with storage to eject from.</param>
    /// <param name="material">The material prototype to eject.</param>
    /// <param name="maxAmount">The maximum amount to eject. If not given, as much as possible is ejected.</param>
    /// <param name="coordinates">The position where to spawn the created sheets. If not given, they're spawned next to the entity.</param>
    /// <param name="component">The storage component on <paramref name="entity"/>. Resolved automatically if not given.</param>
    /// <returns>The stack entities that were spawned.</returns>
    public List<EntityUid> EjectMaterial(
        EntityUid entity,
        string material,
        int? maxAmount = null,
        EntityCoordinates? coordinates = null,
        MaterialStorageComponent? component = null)
    {
        if (!Resolve(entity, ref component))
            return new List<EntityUid>();

        coordinates ??= Transform(entity).Coordinates;

        var amount = GetMaterialAmount(entity, material, component);
        if (maxAmount != null)
            amount = Math.Min(maxAmount.Value, amount);

        var spawned = SpawnMultipleFromMaterial(amount, material, coordinates.Value, out var overflow);

        TryChangeMaterialAmount(entity, material, -(amount - overflow), component);
        return spawned;
    }

    /// <summary>
    /// Eject all material stored in an entity, with the same mechanics as <see cref="EjectMaterial"/>.
    /// </summary>
    /// <param name="entity">The entity with storage to eject from.</param>
    /// <param name="coordinates">The position where to spawn the created sheets. If not given, they're spawned next to the entity.</param>
    /// <param name="component">The storage component on <paramref name="entity"/>. Resolved automatically if not given.</param>
    /// <param name="mergeContacts">If true, spawned stacks will attempt to merge with existing stacks in the area.</param>
    /// <returns>The stack entities that were spawned.</returns>
    public List<EntityUid> EjectAllMaterial(
        EntityUid entity,
        EntityCoordinates? coordinates = null,
        MaterialStorageComponent? component = null,
        bool mergeContacts = false)
    {
        if (!Resolve(entity, ref component))
            return new List<EntityUid>();

        coordinates ??= Transform(entity).Coordinates;

        var allSpawned = new List<EntityUid>();
        foreach (var material in component.Storage.Keys.ToArray())
        {
            var spawned = EjectMaterial(entity, material, null, coordinates, component);
            allSpawned.AddRange(spawned);
        }

        if (mergeContacts)
        {
            foreach (var stack in allSpawned)
            {
                _stackSystem.TryMergeToContacts(stack);
            }
        }

        return allSpawned;
    }
}
