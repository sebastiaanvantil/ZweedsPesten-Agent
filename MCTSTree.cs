using System.ComponentModel.Design;

namespace ZweedsPesten_Agent;

public class MCTSTree(MCTSState initialMctsState, TILDE_RT qFunction) {
    private MCTSNode _rootNode = new(initialMctsState);
    private Random _rnd = new Random();
    private double _explorationConstant = 1;
    private TILDE_RT _qFunction = qFunction;

    public Action.ActionType RunMCTS(int simulations) {
        for (int i = 0; i < simulations; i++) {
            Console.WriteLine("Starting MCTS iteration " + i);
            var selectedNode = Selection();
            Console.WriteLine("Selection succeeded at iteration " + i);
            double reward = Simulation(selectedNode);
            Console.WriteLine("Simulation succeeded at iteration" + i);
            BackPropagation(selectedNode, reward);
            Console.WriteLine("Finished MCTS iteration " + i);
        }

        var bestChild = _rootNode.Children.MaxBy(child => child.Visits);

        return bestChild.ParentAction;
    }

    public MCTSNode Selection() {
        var node = _rootNode;
        while (node.UnexploredChildren.Count == 0) {
            node = BestUCTChild(node);
        }

        var action = node.UnexploredChildren[0];
        node.Children = node.ExpandChildren();
        var result = node.Children.First(child => child.ParentAction == action);
        node.UnexploredChildren.RemoveAt(0);
        return result;
    }

    public double Simulation(MCTSNode selectedNode) {
        var node = selectedNode;
        //int i = 0;
        while (!GoalConditionReached(node.MCTSState)) {
            node.Children = node.ExpandChildren();
            
            var bestChild = node.Children[0];
            var example1 = TILDE_RT.MCTSNode2Example(node, bestChild.ParentAction.ToString());
            double? bestQValue = _qFunction.Predict(_qFunction.Root, example1);
            foreach (var child in node.Children) {
                var example = TILDE_RT.MCTSNode2Example(node, child.ParentAction.ToString());
                double? prediction = _qFunction.Predict(_qFunction.Root, example);
                if (prediction > bestQValue) {
                    bestChild = child;
                    bestQValue = prediction;
                }
            }
            node = bestChild;
            //i++;
            int numCardsInGame = bestChild.MCTSState.Stock.Cards.Count + bestChild.MCTSState.Pile.Cards.Count + bestChild.MCTSState.Pile.BurnedCards.Count;
            foreach (var player in bestChild.MCTSState.Players) {
                numCardsInGame += player.Hand.Count + player.Open.Count + player.Closed.Count;
            }
            Console.WriteLine("Number of cards in the game: " + numCardsInGame);
        }
        return RewardFromGame(node.MCTSState);
    }

    public void BackPropagation(MCTSNode selectedNode, double reward) {
        var node = selectedNode;
        while (node != null) {
            node.Visits++;
            node.Value += reward;
            node = node.Parent;
        }
    }

    public MCTSNode BestUCTChild(MCTSNode parent) {
        return parent.Children.MaxBy(child => (child.Value/child.Visits + _explorationConstant * double.Sqrt(Math.Log(parent.Visits)/child.Visits)))!;
    }
    
    public static bool GoalConditionReached(MCTSState currentMCTSState) {
        return currentMCTSState.Players.Any(player => player.GetListOfCards() == (List<Card>)[]);
    }

    private int RewardFromGame(MCTSState currentMCTSState) {
        var player = currentMCTSState.PlayerQueue.First();
        if (player.GetListOfCards().Count == 0) {
            return 1;
        }

        return 0;
    }
}
