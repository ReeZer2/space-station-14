using Content.Client.Movement.Systems;
using Content.Shared.Actions;
<<<<<<< HEAD
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Ghost;
=======
using Content.Shared.Ghost.Components;
using Content.Shared.Ghost.Systems;
using Content.Shared.NightVision;
using Content.Shared.Overlays;
>>>>>>> wizards/stable
using Robust.Client.Console;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Threading.Tasks;
using Content.Shared.Sprite;

namespace Content.Client.Ghost
{
    public sealed partial class GhostSystem : SharedGhostSystem
    {
<<<<<<< HEAD
        [Dependency] private readonly IClientConsoleHost _console = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly PointLightSystem _pointLightSystem = default!;
        [Dependency] private readonly ContentEyeSystem _contentEye = default!;
        [Dependency] private readonly SpriteSystem _sprite = default!;

        public int AvailableGhostRoleCount { get; private set; }

        //SS220-ghost-hats
        private const string HatEquippedState = "equipped-HELMET";

        private bool _ghostVisibility = true;

        private bool GhostVisibility
        {
            get => _ghostVisibility;
            set
            {
                if (_ghostVisibility == value)
                {
                    return;
                }

                _ghostVisibility = value;

                var query = AllEntityQuery<GhostComponent, SpriteComponent>();
                while (query.MoveNext(out var uid, out _, out var sprite))
                {
                    _sprite.SetVisible((uid, sprite), value || uid == _playerManager.LocalEntity);
                }
            }
        }
=======
        [Dependency] private IClientConsoleHost _console = default!;
        [Dependency] private IPlayerManager _playerManager = default!;
        [Dependency] private SharedActionsSystem _actions = default!;
        [Dependency] private ContentEyeSystem _contentEye = default!;
        [Dependency] private SpriteSystem _sprite = default!;
        [Dependency] private SharedNightVisionSystem _nv = default!;

        [Dependency] private EntityQuery<NightVisionComponent> _nightVisionQuery;

        public int AvailableGhostRoleCount { get; private set; }

        public GhostVisibilityMode GhostVisibility { get; private set; } = GhostVisibilityMode.ShowAllGhosts;
>>>>>>> wizards/stable

        public GhostComponent? Player => CompOrNull<GhostComponent>(_playerManager.LocalEntity);
        public bool IsGhost => Player != null;

        public event Action<GhostComponent>? PlayerRemoved;
        public event Action<GhostComponent>? PlayerUpdated;
        public event Action<GhostComponent>? PlayerAttached;
        public event Action? PlayerDetached;
        public event Action<GhostWarpsResponseEvent>? GhostWarpsResponse;
        public event Action<GhostUpdateGhostRoleCountEvent>? GhostRoleCountUpdated;

        [SubscribeLocalEvent]
        private void OnStartup(EntityUid uid, GhostComponent component, ComponentStartup args)
        {
            _sprite.SetVisible(uid, GetGhostVisible(uid, GhostVisibility));
        }

        [SubscribeLocalEvent]
        private void OnToggleLighting(EntityUid uid, EyeComponent component, ToggleLightingActionEvent args)
        {
            if (args.Handled)
                return;

            if (!component.DrawLight)
            {
                // normal lighting
                Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-lighting-manager-popup-normal"), args.Performer);
                _contentEye.RequestEye(component.DrawFov, true);
            }
            else if (_nightVisionQuery.TryComp(uid, out var nv) && !nv.Enabled)
            {
                Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-lighting-manager-popup-half-bright"), args.Performer);
                _nv.SetEnabled((uid, nv), true);
            }
            else
            {
                // fullbright mode
                Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-lighting-manager-popup-fullbright"), args.Performer);
                _contentEye.RequestEye(component.DrawFov, false);
                _nv.SetEnabled((uid, nv), false);
            }

            args.Handled = true;
        }

<<<<<<< HEAD
        //SS220 colourful ghost begin
        // Changing FoV must not modify the server-selected ghost color.
=======
        [SubscribeLocalEvent]
>>>>>>> wizards/stable
        private void OnToggleFoV(EntityUid uid, EyeComponent component, ToggleFoVActionEvent args)
        {
            if (args.Handled)
                return;

            Popup.PopupEntity(Loc.GetString("ghost-gui-toggle-fov-popup"), args.Performer);
            _contentEye.RequestToggleFov(uid, component);
            args.Handled = true;
        }
        //SS220 colourful ghost end

        [SubscribeLocalEvent]
        private void OnToggleGhosts(EntityUid uid, GhostComponent component, ToggleGhostsActionEvent args)
        {
            if (args.Handled)
                return;

            var locId = GhostVisibility switch
            {
                GhostVisibilityMode.ShowAllGhosts => "ghost-gui-toggle-ghost-visibility-popup-off",
                GhostVisibilityMode.HideOtherGhosts => "ghost-gui-toggle-all-ghosts-visibility-popup-off",
                GhostVisibilityMode.HideOtherGhostsAndSelf => "ghost-gui-toggle-ghost-visibility-popup-on",
                _ => throw new ArgumentOutOfRangeException()
            };

            Popup.PopupEntity(Loc.GetString(locId), args.Performer);
            if (uid == _playerManager.LocalEntity)
                ToggleGhostVisibility();

            args.Handled = true;
        }

<<<<<<< HEAD
        //SS220-ghost-hats begin
        private void SetBodyVisuals(EntityUid uid, SpriteComponent? sprite, bool visible)
        {
            if (!Resolve(uid, ref sprite))
                return;

            if (!HasComp<GhostComponent>(uid))
                return;

            var typingIndicatorStates = new string[0];
            if (TryComp<TypingIndicatorComponent>(uid, out var typingIndicator) &&
                _prototypeManager.TryIndex<TypingIndicatorPrototype>(typingIndicator.TypingIndicatorPrototype, out var typingProto))
                typingIndicatorStates = new string[] { typingProto.TypingState, typingProto.IdleState };

            var spriteLayers = sprite.AllLayers;
            foreach (var layer in spriteLayers)
            {
                if (layer.RsiState == HatEquippedState ||
                    typingIndicatorStates.Contains(layer.RsiState.ToString()))
                    continue;

                layer.Visible = visible;
            }
        }
        //SS220-ghost-hats end

=======
        [SubscribeLocalEvent]
>>>>>>> wizards/stable
        private void OnGhostRemove(EntityUid uid, GhostComponent component, ComponentRemove args)
        {
            _actions.RemoveAction(uid, component.ToggleLightingActionEntity);
            _actions.RemoveAction(uid, component.ToggleFoVActionEntity);
            _actions.RemoveAction(uid, component.ToggleGhostsActionEntity);
            _actions.RemoveAction(uid, component.ToggleGhostHearingActionEntity);
            //ss220 add filter tts for ghost
            _actions.RemoveAction(uid, component.ToggleRadioChannelsUIEntity);
            //SS220-ghost-hats
            _actions.RemoveAction(uid, component.ToggleAGhostBodyVisualsActionEntity);

            if (uid != _playerManager.LocalEntity)
                return;

<<<<<<< HEAD
            ResetGhostVisibility(); // SS220-Fix ghosts visibility for other entities
            PlayerRemoved?.Invoke(component);
        }

        //SS220 colourful ghost begin
        // Attaching a player must not replace the networked color with a local random tint.
        private void OnGhostPlayerAttach(EntityUid uid, GhostComponent component, LocalPlayerAttachedEvent localPlayerAttachedEvent)
        {
            ResetGhostVisibility(); // SS220-Fix ghosts visibility for other entities
=======
            ApplyGhostVisibility(GhostVisibilityMode.HideOtherGhosts);
            PlayerRemoved?.Invoke(component);
        }

        [SubscribeLocalEvent]
        private void OnGhostPlayerAttach(EntityUid uid, GhostComponent component, LocalPlayerAttachedEvent localPlayerAttachedEvent)
        {
            ApplyGhostVisibility(GhostVisibilityMode.ShowAllGhosts);
>>>>>>> wizards/stable
            PlayerAttached?.Invoke(component);
        }
        //SS220 colourful ghost end

        [SubscribeLocalEvent]
        private void OnGhostState(EntityUid uid, GhostComponent component, ref AfterAutoHandleStateEvent args)
        {
<<<<<<< HEAD
            if (TryComp<SpriteComponent>(uid, out var sprite))
            {
                _sprite.LayerSetColor((uid, sprite), 0, component.Color); //SS220 colourful ghost

                //SS220-ghost-hats
                SetBodyVisuals(uid, sprite, component.BodyVisible);
            }
=======
            _sprite.LayerSetColor(uid, 0, component.Color);
>>>>>>> wizards/stable

            if (uid != _playerManager.LocalEntity)
                return;

            PlayerUpdated?.Invoke(component);
        }

        [SubscribeLocalEvent]
        private void OnGhostPlayerDetach(EntityUid uid, GhostComponent component, LocalPlayerDetachedEvent args)
        {
<<<<<<< HEAD
            ResetGhostVisibility(); // SS220-Fix ghosts visibility for other entities
            PlayerDetached?.Invoke();
        }

        // SS220-Fix ghosts visibility for other entities-begin
        // Just a semantic separation for a better understanding of the logic
        private void ResetGhostVisibility()
        {
            GhostVisibility = true;
        }
        // SS220-Fix ghosts visibility for other entities-end

=======
            ApplyGhostVisibility(GhostVisibilityMode.HideOtherGhosts);
            PlayerDetached?.Invoke();
        }

        [SubscribeNetworkEvent]
>>>>>>> wizards/stable
        private void OnGhostWarpsResponse(GhostWarpsResponseEvent msg)
        {
            if (!IsGhost)
            {
                return;
            }

            GhostWarpsResponse?.Invoke(msg);
        }

        [SubscribeNetworkEvent]
        private void OnUpdateGhostRoleCount(GhostUpdateGhostRoleCountEvent msg)
        {
            AvailableGhostRoleCount = msg.AvailableGhostRoles;
            GhostRoleCountUpdated?.Invoke(msg);
        }

        public void RequestWarps()
        {
            RaiseNetworkEvent(new GhostWarpsRequestEvent());
        }

        public void ReturnToBody()
        {
            var msg = new GhostReturnToBodyRequest();
            RaiseNetworkEvent(msg);
        }

        public void OpenGhostRoles()
        {
            _console.RemoteExecuteCommand(null, "ghostroles");
        }

        /// <summary>
        /// Advances through the available ghost visibility modes in sequence.
        /// </summary>
        public void ToggleGhostVisibility()
        {
            var nextMode = GhostVisibility switch
            {
                GhostVisibilityMode.ShowAllGhosts => GhostVisibilityMode.HideOtherGhosts,
                GhostVisibilityMode.HideOtherGhosts => GhostVisibilityMode.HideOtherGhostsAndSelf,
                GhostVisibilityMode.HideOtherGhostsAndSelf => GhostVisibilityMode.ShowAllGhosts,
                _ => throw new ArgumentOutOfRangeException()
            };

            ApplyGhostVisibility(nextMode);
        }

        private void ApplyGhostVisibility(GhostVisibilityMode mode)
        {
            if (GhostVisibility == mode)
                return;

            GhostVisibility = mode;

            var query = AllEntityQuery<GhostComponent, SpriteComponent>();
            while (query.MoveNext(out var uid, out _, out var sprite))
            {
                _sprite.SetVisible((uid, sprite), GetGhostVisible(uid, mode));
            }
        }

        private bool GetGhostVisible(EntityUid uid, GhostVisibilityMode mode)
        {
            return mode switch
            {
                GhostVisibilityMode.ShowAllGhosts => true,
                GhostVisibilityMode.HideOtherGhosts => uid == _playerManager.LocalEntity,
                GhostVisibilityMode.HideOtherGhostsAndSelf => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }

    /// <summary>
    /// Controls which ghost sprites are visible to the local client.
    /// </summary>
    public enum GhostVisibilityMode : byte
    {
        /// <summary>
        /// Show all ghosts.
        /// </summary>
        ShowAllGhosts,

        /// <summary>
        /// Hide other ghosts, but keep the local ghost visible.
        /// </summary>
        HideOtherGhosts,

        /// <summary>
        /// Hide all ghosts, including the local ghost.
        /// </summary>
        HideOtherGhostsAndSelf,
    }
}
