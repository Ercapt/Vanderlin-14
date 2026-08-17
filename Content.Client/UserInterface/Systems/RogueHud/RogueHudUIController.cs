using Content.Client.Actions;
using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Client.Hands.Systems;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Inventory;
using Content.Client.UserInterface.Systems.RogueHud.Widgets;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.CombatMode;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Player;

namespace Content.Client.UserInterface.Systems.RogueHud;

public sealed partial class RogueHudUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnSystemChanged<CombatModeSystem>, IOnSystemChanged<HandsSystem>, IOnSystemChanged<ActionsSystem>
{
    [Dependency] private IPlayerManager _player = default!;

    [UISystemDependency] private readonly CombatModeSystem _combatMode = default!;
    [UISystemDependency] private readonly HandsSystem _handsSystem = default!;
    [UISystemDependency] private readonly ActionsSystem _actionsSystem = default!;

    private RogueHudGui? RogueHud => UIManager.GetActiveUIWidgetOrNull<RogueHudGui>();

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        if (RogueHud == null)
            return;

        RogueHud.OnCombatTogglePressed += ToggleCombatMode;
        RogueHud.OnDropPressed += DropActiveItem;

        if (_player.LocalEntity is { } localPlayer)
        {
            RogueHud.UpdateCombatModeState(_combatMode.IsInCombatMode(localPlayer));
        }
    }

    public void OnStateEntered(GameplayState state)
    {
    }

    public void OnStateExited(GameplayState state)
    {
        if (RogueHud != null)
        {
            RogueHud.OnCombatTogglePressed -= ToggleCombatMode;
            RogueHud.OnDropPressed -= DropActiveItem;
        }
    }

    public void OnSystemLoaded(CombatModeSystem system)
    {
        system.LocalPlayerCombatModeUpdated += OnCombatModeUpdated;
    }

    public void OnSystemUnloaded(CombatModeSystem system)
    {
        system.LocalPlayerCombatModeUpdated -= OnCombatModeUpdated;
    }

    public void OnSystemLoaded(HandsSystem system)
    {
    }

    public void OnSystemUnloaded(HandsSystem system)
    {
    }

    public void OnSystemLoaded(ActionsSystem system)
    {
    }

    public void OnSystemUnloaded(ActionsSystem system)
    {
    }

    private void OnCombatModeUpdated(bool inCombat)
    {
        RogueHud?.UpdateCombatModeState(inCombat);
    }

    private void ToggleCombatMode()
    {
        if (_player.LocalEntity is { } localPlayer &&
            EntityManager.TryGetComponent<CombatModeComponent>(localPlayer, out var combatComp) &&
            combatComp.CombatToggleActionEntity is { } actionEnt &&
            EntityManager.TryGetComponent<ActionComponent>(actionEnt, out var actionComp))
        {
            _actionsSystem.TriggerAction((actionEnt, actionComp));
        }
        else if (_player.LocalEntity is { } entity)
        {
            _combatMode.SetInCombatMode(entity, !_combatMode.IsInCombatMode(entity));
        }
    }

    private void DropActiveItem()
    {
        if (_player.LocalEntity is { } localPlayer)
        {
            _handsSystem.TryDrop(localPlayer);
        }
    }
}
