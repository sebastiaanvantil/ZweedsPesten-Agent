namespace ZweedsPesten_Agent;



public class TILDE_RT(int maxDepth, int minSamplesSplit) {
    private readonly List<string> _numericalFeatures = [
        "num_cards", "highest_card", "lowest_permitted_card", "num_pile_cards", "num_stock_cards", "top_card_pile_value"
    ];
    private readonly List<string> _booleanFeatures = ["has_2", "has_3", "has_7", "has_10"];

    private readonly List<string> _actionFeature = ["action"];

    public readonly TILDE_RT_Node Root = new TILDE_RT_Node();

    public TILDE_RT_Node Train(List<Dictionary<string, object>> examples, int depth = 0) {
        Split(Root, examples, 0);
        return Root;
    }

    private void Split(TILDE_RT_Node node, List<Dictionary<string, object>> examples, int depth) {
        if (examples.Count < minSamplesSplit || depth >= maxDepth) {
            double total = examples.Sum(example => Convert.ToDouble(example["q_value"]));
            node.Qvalue = total / examples.Count;  
            return;
        }
        
        var bestSplit = FindBestSplit(examples);
        if (bestSplit == null) {
            double total = examples.Sum(example => Convert.ToDouble(example["q_value"]));
            node.Qvalue = total / examples.Count;  
            return;
        }

        node.Test = bestSplit.Item1;
        
        var leftExamples = examples.Where(e => bestSplit.Item2(e)).ToList();
        var rightExamples = examples.Where(e => !bestSplit.Item2(e)).ToList();
        
        node.LeftChild = new TILDE_RT_Node();
        node.RightChild = new TILDE_RT_Node();

        Split(node.LeftChild, leftExamples, depth + 1);
        Split(node.RightChild, rightExamples, depth + 1);
    }

    private Tuple<string, Func<Dictionary<string, object>, bool>?>? FindBestSplit(List<Dictionary<string, object>> examples) {
        double minVariance = double.MaxValue;
        string? bestFeature = null;
        Func<Dictionary<string, object>, bool>? bestFolLiteral = null;
        
        foreach (string feature in _numericalFeatures) {
            double min = examples.Min(e => Convert.ToDouble(e[feature]));
            double max = examples.Max(e => Convert.ToDouble(e[feature]));
            
            var testValues = Enumerable.Range(0, 5)
                .Select(i => min + i * (max - min) / (5 - 1))
                .ToList();;

            foreach (double testValue in testValues) {
                Func<Dictionary<string, object>, bool> folLiteral = e => Convert.ToDouble(e[feature]) <= testValue;

                var left = examples.Where(folLiteral).Select(e => Convert.ToDouble(e["q_value"])).ToList();
                var right = examples.Where(e => !folLiteral(e)).Select(e => Convert.ToDouble(e["q_value"])).ToList();

                if (left.Count == 0 || right.Count == 0) continue;

                double variance = (left.Count * Variance(left) + right.Count * Variance(right)) / examples.Count;
                if (variance < minVariance)
                {
                    minVariance = variance;
                    bestFeature = $"{feature} <= {testValue}";
                    bestFolLiteral = folLiteral;
                }
            }
            
        }
        
        foreach (string feature in _booleanFeatures) {
            Func<Dictionary<string, object>, bool> folLiteral = e => Convert.ToBoolean(e[feature]);

            var left = examples.Where(folLiteral).Select(e => Convert.ToDouble(e["q_value"])).ToList();
            var right = examples.Where(e => !folLiteral(e)).Select(e => Convert.ToDouble(e["q_value"])).ToList();

            if (left.Count == 0 || right.Count == 0) continue;

            double variance = (left.Count * Variance(left) + right.Count * Variance(right)) / examples.Count;
            if (variance < minVariance)
            {
                minVariance = variance;
                bestFeature = $"{feature} == true";
                bestFolLiteral = folLiteral;
            }
        }

        foreach (string feature in _actionFeature) {
            var uniqueActions = examples.Select(e => e[feature]).Distinct().ToList();

            foreach (string action in uniqueActions) {
                Func<Dictionary<string, object>, bool> folLiteral = e => Convert.ToBoolean((string)e[feature] == action);
                
                var left = examples.Where(folLiteral).Select(e => Convert.ToDouble(e["q_value"])).ToList();
                var right = examples.Where(e => !folLiteral(e)).Select(e => Convert.ToDouble(e["q_value"])).ToList();

                if (left.Count == 0 || right.Count == 0) continue;

                double variance = (left.Count * Variance(left) + right.Count * Variance(right)) / examples.Count;
                if (variance < minVariance)
                {
                    minVariance = variance;
                    bestFeature = $"action == {action}";
                    bestFolLiteral = folLiteral;
                }
            }
        }
        
        return bestFeature == null ? null : Tuple.Create(bestFeature, bestFolLiteral);
    }   
    
    private double Variance(List<double> values)
    {
        double mean = values.Average();
        return values.Average(v => Math.Pow(v - mean, 2));
    }

    public double Predict(TILDE_RT_Node rootNode, Dictionary<string, object> example) {
        var node = rootNode;

        while (node != null && !node.IsLeafNode()) {
            bool nodeHasChanged = false;
            if (node.Test == null) {
                return double.NaN;
            }
            if (node.Test.Contains("<=")) {
                string[] test = node.Test.Split("<=");
                string feature = test[0].Trim();
                double numericValue = Convert.ToDouble(example[feature]);
                double testValue = Convert.ToDouble(test[1]);
                node = numericValue <= testValue ? node.LeftChild : node.RightChild;
                nodeHasChanged = true;
            }

            if (!nodeHasChanged && node.Test.Contains("action")) {
                string[] test = node.Test.Split("==");
                string action = test[0].Trim();
                var actioncheck = example[action]?.ToString();
                node = actioncheck == "True" ? node.LeftChild : node.RightChild;
            }
            else if (!nodeHasChanged && node.Test.Contains("==")) {
                string[] test = node.Test.Split("==");
                string feature = test[0].Trim();
                Console.WriteLine("value: " + example[feature]);
                node = example[feature] is true ? node.LeftChild : node.RightChild;
            }
        }

        if (node.Qvalue != null) {
            return node.Qvalue;
        }

        return 0;
    }
    
    public void PrintTree(TILDE_RT_Node? node, int depth) {
        if (node == null) return;
        
        string indent = new string(' ', depth * 4);
        if (node.IsLeafNode())
            Console.WriteLine($"{indent}[Leaf] Q-value: {node.Qvalue:F2}");
        else
        {
            Console.WriteLine($"{indent}If {node.Test}:");
            PrintTree(node.LeftChild, depth + 1);
            Console.WriteLine($"{indent}Else:");
            PrintTree(node.RightChild, depth + 1);
        }
    }

    public static Dictionary<string, object> MCTSNode2Example(MCTSNode node, string action) {
        var example = new Dictionary<string, object>();
        var player = node.MCTSState.PlayerQueue.First();
        var pile = node.MCTSState.Pile;
        var stock = node.MCTSState.Stock;
        example["num_cards"] = player.GetNumCards();
        example["highest_card"] = (int)player.GetHighestCard().Rank;
        var topcard = new Card(Suit.Clubs, Rank.Two);
        if (pile.Cards.Count > 0) topcard = pile.Cards.Peek();
        example["lowest_permitted_card"] = (int)player.GetLowestPermittedCard(topcard.Rank).Rank;
        example["num_pile_cards"] = pile.Cards.Count;
        example["num_stock_cards"] = stock.Cards.Count;
        example["top_card_pile_value"] = topcard.Rank;
        example["has_2"] = player.HasTwo();
        example["has_3"] = player.HasThree();
        example["has_7"] = player.HasSeven();
        example["has_10"] = player.HasTen();
        example["action"] = action;

        return example;
    }
}