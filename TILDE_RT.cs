using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ZweedsPesten_Agent;



[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TILDE_RT(int maxDepth, int minSamplesSplit) {
    private readonly List<string> _numericalFeatures = [
        "num_cards", "highest_card", "lowest_permitted_card", "num_pile_cards", "num_stock_cards", "top_card_pile_value"
    ];
    private readonly List<string> _booleanFeatures = ["has_2", "has_3", "has_7", "has_10"];

    public readonly TILDE_RT_Node Root = new (false);

    public TILDE_RT_Node Train(List<Dictionary<string, object>> examples) {
        Split(Root, examples, 0);
        return Root;
    }

    private void Split(TILDE_RT_Node node, List<Dictionary<string, object>> examples, int depth) {
        if (examples.Count < minSamplesSplit || depth >= maxDepth) {
            SplitLeaf(node, examples);
            return;
        }
        
        var bestSplit = FindBestSplit(examples);
        if (bestSplit == null) {
            SplitLeaf(node, examples);
            return;
        }

        node.Test = bestSplit.Item1;
        
        var leftExamples = examples.Where(e => bestSplit.Item2(e)).ToList();
        var rightExamples = examples.Where(e => !bestSplit.Item2(e)).ToList();
        
        node.LeftChild = new TILDE_RT_Node(false);
        node.RightChild = new TILDE_RT_Node(false);

        Split(node.LeftChild, leftExamples, depth + 1);
        Split(node.RightChild, rightExamples, depth + 1);
    }

    private void SplitLeaf(TILDE_RT_Node node, List<Dictionary<string, object>> examples) {
        string[] uniqueActions = [
            "PlayTwo",
            "PlayMultipleTwo",
            "PlayThree",
            "PlayMultipleThree",
            "PlaySeven",
            "PlayMultipleSeven",
            "PlayTen",
            "PlayMultipleTen",
            "PlayHighest",
            "PlayMultipleHighest",
            "PlayLowestPermitted",
            "PlayMultipleLowestPermitted",
            "PickUpPile",
            "PlayClosedCard",
            "PlayOpenCard",
        ];

        var actionsInExamples = examples.Where(example => example.ContainsKey("action"))
            .Select(example => (string)example["action"]).Distinct().ToList();

        foreach (string action in uniqueActions) {
            var actionLeaf = new TILDE_RT_Node(true, action);
            if (actionsInExamples.Contains(action)) {
                var actionExamples = examples.Where(example => (string)example["action"] == action).ToList();
                double summedQValue = actionExamples.Sum(example => Convert.ToDouble(example["q_value"]));
                actionLeaf.Qvalue = summedQValue / actionExamples.Count;
            }
            else {
                actionLeaf.Qvalue = 0.5;
            }
            node.ActionChildren.Add(actionLeaf);
        }
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
                .ToList();

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
        
        return bestFeature == null ? null : Tuple.Create(bestFeature, bestFolLiteral);
    }   
    
    private static double Variance(List<double> values) {
        double mean = values.Average();
        return values.Average(v => Math.Pow(v - mean, 2));
    }

    public static double Predict(TILDE_RT_Node rootNode, Dictionary<string, object> example) {
        var node = rootNode;

        while (node != null && !node.IsLeafNode()) {
            bool nodeHasChanged = false;
            if (node.Test == null) {
                return 0;
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
                string? actionCheck = example[action].ToString();
                node = actionCheck == "True" ? node.LeftChild : node.RightChild;
            }
            else if (!nodeHasChanged && node.Test.Contains("==")) {
                string[] test = node.Test.Split("==");
                string feature = test[0].Trim();
                node = example[feature] is true ? node.LeftChild : node.RightChild;
            }
        }

        if (node == null) {
            return 0.5;
        }
        string exampleAction = (string)example["action"];

        foreach (var actionLeaf in node.ActionChildren) {
            if (actionLeaf.Test == exampleAction) {
                return actionLeaf.Qvalue;
            }
        }

        return 0.5;
    }
    
    // I did not come up with this printing approach myself
    // Inspired by: https://cs.phyley.com/binary-tree/print-level-by-level or https://www.youtube.com/watch?v=o-_Gk0rBeIo on YouTube
    public static void PrintTree(TILDE_RT_Node root) {
        var queue = new Queue<(TILDE_RT_Node?, int depth)>();
        queue.Enqueue((root, 0));
        
        int currentDepth = 0;

        while(queue.Count > 0) {
            (var node, int depth) = queue.Dequeue();
            if (node == null) continue;
            if (depth != currentDepth) {
                Console.WriteLine();
                currentDepth = depth;
            }
            
            string label = "";
            if (!node.IsLeaf) {
                label += node.Test;
            }
            else {
                label += Convert.ToString(node.Qvalue, CultureInfo.CurrentCulture);
            }
                
            Console.Write("[" + label + "]");

            if (node.IsLeafNode()) {
                foreach (var t in node.ActionChildren) {
                    queue.Enqueue((t, depth + 1));
                }
            }
            else if (node.LeftChild != null && node.RightChild != null) {
                queue.Enqueue((node.LeftChild, depth + 1));
                queue.Enqueue((node.RightChild, depth + 1));
            }
        }
    }

    public static Dictionary<string, object> MCTSNode2Example(MCTSNode node, string action) {
        var example = new Dictionary<string, object>();
        var player = node.MCTSState.PlayerQueue.First();
        var pile = node.MCTSState.Pile;
        var stock = node.MCTSState.Stock;
        example["num_cards"] = player.GetNumCards();
        example["highest_card"] = (int)player.GetHighestCard().Rank;
        var topCard = new Card(Suit.Clubs, Rank.NonExist);
        if (pile.Cards.Count > 0) topCard = pile.Cards.Peek();
        example["lowest_permitted_card"] = (int)player.GetLowestPermittedCard(topCard.Rank).Rank;
        example["num_pile_cards"] = pile.Cards.Count;
        example["num_stock_cards"] = stock.Cards.Count;
        example["top_card_pile_value"] = topCard.Rank;
        example["has_2"] = player.HasTwo();
        example["has_3"] = player.HasThree();
        example["has_7"] = player.HasSeven();
        example["has_10"] = player.HasTen();
        example["action"] = action;

        return example;
    }
}