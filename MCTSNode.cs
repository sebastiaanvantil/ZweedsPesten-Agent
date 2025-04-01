using System.Dynamic;
using System.Reflection;

namespace ZweedsPesten_Agent;

public class MCTSNode {
    public MCTSState MCTSState;
    public MCTSNode? Parent;
    public Action.ActionType ParentAction;
    public List<MCTSNode> Children;
    public int Visits;
    public double Value;
    public List<Action.ActionType> UnexploredChildren;

    public MCTSNode(MCTSState mctsState, MCTSNode? parent = null, Action.ActionType parentAction = Action.ActionType.PlayHighest) {
        MCTSState = mctsState;
        Parent = parent;
        ParentAction = parentAction;
        Children = [];
        Visits = 0;
        Value = 0;
        UnexploredChildren = [];
        var player = MCTSState.PlayerQueue.First();
        var opponents = MCTSState.Players.Where(p => p.Id != player.Id).ToList();
        var permittedActions = Action.GetAllowedActions(player, MCTSState.Pile, opponents);
        foreach (var action in permittedActions) {
            UnexploredChildren.Add(action);
        }
    }

    public List<MCTSNode> ExpandChildren() {
        var result = new List<MCTSNode>();
        var player = MCTSState.PlayerQueue.First();
        var opponents = MCTSState.Players.Where(p => p.Id != player.Id).ToList();
        var permittedActions = Action.GetAllowedActions(player, MCTSState.Pile, opponents);
        foreach (var action in permittedActions) {
            var childMCTSState = new MCTSState(MCTSState);
            var clonedPlayer = childMCTSState.Players.First(p => p.Id == player.Id);
            
            childMCTSState.PlayerQueue.RemoveFirst();
            childMCTSState.PlayerQueue.AddFirst(clonedPlayer);
            
            int playerIndex = childMCTSState.Players.FindIndex(p => p.Id == clonedPlayer.Id);
            if (playerIndex >= 0) childMCTSState.Players[playerIndex] = clonedPlayer;
            
            
            Action.PlayAction(clonedPlayer, childMCTSState.Pile, action);
            if (!Game.OneMoreTurn(childMCTSState.Pile, action)) {
                childMCTSState.PlayerQueue.RemoveFirst();
                childMCTSState.PlayerQueue.AddLast(clonedPlayer);
            }
            
            while (childMCTSState.Stock.Cards.Count > 0 && clonedPlayer.Hand.Count < 3) {
                var cardToDraw = childMCTSState.Stock.Cards.Pop();
                clonedPlayer.DrawToHand(cardToDraw);
            }
            
            var child = new MCTSNode(childMCTSState, this, action);
            result.Add(child);
        }
        return result;
    }
}
