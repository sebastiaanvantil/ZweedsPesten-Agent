namespace ZweedsPesten_Agent;

public class MCTSTree(MCTSState initialMctsState, TILDE_RT qFunction) {
    private readonly MCTSNode _rootNode = new(initialMctsState);
    private readonly Random _rnd = new();
    private const double ExplorationConstant = 1;

    public Action.ActionType RunMCTS(int simulations) {
        for (int i = 0; i < simulations; i++) {
            var selectedNode = Selection();
            double reward = Simulation(selectedNode);
            BackPropagation(selectedNode, reward);
        }

        var bestChild = _rootNode.Children.MaxBy(child => child.Visits)!;

        return bestChild.ParentAction;
    }

    private MCTSNode Selection() {
        var node = _rootNode;
        while (node.UnexploredChildren.Count == 0) {
            var bestUCTChild = BestUCTChild(node);
            if (bestUCTChild == null || GoalConditionReached(node.MCTSState)) {
                return node;
            }
            node = bestUCTChild;
        }

        var action = node.UnexploredChildren[0];
        node.Children = node.ExpandChildren();
        var result = node.Children.First(child => child.ParentAction == action);
        node.UnexploredChildren.RemoveAt(0);
        return result;
    }

    private double Simulation(MCTSNode selectedNode) {
        var node = selectedNode;
        int i = 0;
        while (!GoalConditionReached(node.MCTSState)) {
            node.Children = node.ExpandChildren();

            var softMaxValues = (List<double>)[];
            foreach (var child in node.Children) {
                var example = TILDE_RT.MCTSNode2Example(node, child.ParentAction.ToString());
                double prediction = qFunction.Predict(qFunction.Root, example);

                softMaxValues.Add(Math.Pow(Math.E, prediction));
            }

            double totalSoftMaxValue = softMaxValues.Sum();
            var normalisedSoftMaxValues = softMaxValues.Select(value => value / totalSoftMaxValue).ToList();
            double random = _rnd.NextDouble();
            double sum = 0;
            int index = 0;
            for (int m = 0; m < normalisedSoftMaxValues.Count; m++) {
                sum += normalisedSoftMaxValues[m];
                if (random < sum) {
                    index = m;
                    break;
                }
            }
            
            node = node.Children[index];
            i++;
            //Console.WriteLine("Simulation Iteration: " + i);
        }
        return RewardFromGame(node.MCTSState);
    }

    private void BackPropagation(MCTSNode selectedNode, double reward) {
        var node = selectedNode;
        while (node != null) {
            node.Visits++;
            node.Value += reward;
            node = node.Parent;
        }
    }

    private static MCTSNode? BestUCTChild(MCTSNode parent) {
        return parent.Children.MaxBy(child => (child.Value/child.Visits + ExplorationConstant * double.Sqrt(Math.Log(parent.Visits)/child.Visits)));
    }
    
    public static bool GoalConditionReached(MCTSState currentMCTSState) {
        return currentMCTSState.Players.Any(player => player.GetListOfCards().Count == 0);
    }

     private static int RewardFromGame(MCTSState currentMCTSState) {
        var player = currentMCTSState.PlayerQueue.First();
        if (player.GetListOfCards().Count == 0) {
            return 1;
        }

        return 0;
    }
}
