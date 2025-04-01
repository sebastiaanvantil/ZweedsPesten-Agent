using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZweedsPesten_Agent
{
    public class Program
    {
        public static void Main(string[] args) {
            var QFunction = new TILDE_RT(100, 2);
            var ZweedsPesten_Agent = new Agent(QFunction);
            ZweedsPesten_Agent.Train(1);
            QFunction.PrintTree(QFunction.Root, 100);
        }
    }
}

/*

var toyExamples = new List<Dictionary<string, object>> {
            new Dictionary<string, object> { { "num_cards", 8 }, { "highest_card", 12 }, { "lowest_permitted_card", 12 }, { "num_pile_cards", 4 }, { "num_stock_cards", 13 }, { "top_card_pile_value", 14 }, { "has_2", false }, { "has_3", false }, { "has_7", true }, { "has_10", false }, { "action", "PlayTwo" }, { "q_value", 0.241f } },
            new Dictionary<string, object> { { "num_cards", 4 }, { "highest_card", 6 }, { "lowest_permitted_card", 4 }, { "num_pile_cards", 6 }, { "num_stock_cards", 8 }, { "top_card_pile_value", 14 }, { "has_2", false }, { "has_3", true }, { "has_7", true }, { "has_10", true }, { "action", "PlayMultipleTen" }, { "q_value", 0.770f } },
            new Dictionary<string, object> { { "num_cards", 12 }, { "highest_card", 5 }, { "lowest_permitted_card", 5 }, { "num_pile_cards", 8 }, { "num_stock_cards", 10 }, { "top_card_pile_value", 5 }, { "has_2", true }, { "has_3", true }, { "has_7", true }, { "has_10", true }, { "action", "PlayTen" }, { "q_value", 0.417f } },
            new Dictionary<string, object> { { "num_cards", 10 }, { "highest_card", 7 }, { "lowest_permitted_card", 4 }, { "num_pile_cards", 11 }, { "num_stock_cards", 5 }, { "top_card_pile_value", 4 }, { "has_2", false }, { "has_3", true }, { "has_7", false }, { "has_10", true }, { "action", "PlayTen" }, { "q_value", 0.030f } },
            new Dictionary<string, object> { { "num_cards", 6 }, { "highest_card", 4 }, { "lowest_permitted_card", 10 }, { "num_pile_cards", 11 }, { "num_stock_cards", 14 }, { "top_card_pile_value", 4 }, { "has_2", true }, { "has_3", false }, { "has_7", true }, { "has_10", true }, { "action", "PlayThree" }, { "q_value", 0.956f } },
            new Dictionary<string, object> { { "num_cards", 5 }, { "highest_card", 10 }, { "lowest_permitted_card", 6 }, { "num_pile_cards", 6 }, { "num_stock_cards", 12 }, { "top_card_pile_value", 8 }, { "has_2", false }, { "has_3", true }, { "has_7", true }, { "has_10", true }, { "action", "PlayTen" }, { "q_value", 0.642f } },
            new Dictionary<string, object> { { "num_cards", 9 }, { "highest_card", 13 }, { "lowest_permitted_card", 9 }, { "num_pile_cards", 8 }, { "num_stock_cards", 13 }, { "top_card_pile_value", 5 }, { "has_2", false }, { "has_3", true }, { "has_7", false }, { "has_10", false }, { "action", "PlayThree" }, { "q_value", 0.486f } },
            new Dictionary<string, object> { { "num_cards", 6 }, { "highest_card", 6 }, { "lowest_permitted_card", 10 }, { "num_pile_cards", 10 }, { "num_stock_cards", 10 }, { "top_card_pile_value", 6 }, { "has_2", false }, { "has_3", false }, { "has_7", true }, { "has_10", true }, { "action", "PlayMultipleTen" }, { "q_value", 0.173f } },
            new Dictionary<string, object> { { "num_cards", 10 }, { "highest_card", 11 }, { "lowest_permitted_card", 7 }, { "num_pile_cards", 7 }, { "num_stock_cards", 4 }, { "top_card_pile_value", 5 }, { "has_2", false }, { "has_3", true }, { "has_7", true }, { "has_10", true }, { "action", "PlayMultipleThree" }, { "q_value", 0.855f } },
            new Dictionary<string, object> { { "num_cards", 13 }, { "highest_card", 6 }, { "lowest_permitted_card", 12 }, { "num_pile_cards", 5 }, { "num_stock_cards", 12 }, { "top_card_pile_value", 10 }, { "has_2", false }, { "has_3", false }, { "has_7", true }, { "has_10", false }, { "action", "PlaySeven" }, { "q_value", 0.129f } },
            new Dictionary<string, object> { { "num_cards", 5 }, { "highest_card", 8 }, { "lowest_permitted_card", 5 }, { "num_pile_cards", 7 }, { "num_stock_cards", 6 }, { "top_card_pile_value", 7 }, { "has_2", true }, { "has_3", false }, { "has_7", true }, { "has_10", true }, { "action", "PlaySeven" }, { "q_value", 0.504f } },
            new Dictionary<string, object> { { "num_cards", 11 }, { "highest_card", 14 }, { "lowest_permitted_card", 6 }, { "num_pile_cards", 9 }, { "num_stock_cards", 9 }, { "top_card_pile_value", 11 }, { "has_2", true }, { "has_3", true }, { "has_7", false }, { "has_10", false }, { "action", "PlayHighest" }, { "q_value", 0.322f } },
            new Dictionary<string, object> { { "num_cards", 7 }, { "highest_card", 7 }, { "lowest_permitted_card", 4 }, { "num_pile_cards", 6 }, { "num_stock_cards", 10 }, { "top_card_pile_value", 7 }, { "has_2", false }, { "has_3", false }, { "has_7", true }, { "has_10", true }, { "action", "PlayMultipleSeven" }, { "q_value", 0.606f } },
            new Dictionary<string, object> { { "num_cards", 14 }, { "highest_card", 10 }, { "lowest_permitted_card", 6 }, { "num_pile_cards", 13 }, { "num_stock_cards", 13 }, { "top_card_pile_value", 6 }, { "has_2", false }, { "has_3", true }, { "has_7", false }, { "has_10", true }, { "action", "PlayTen" }, { "q_value", 0.911f } },
            new Dictionary<string, object> { { "num_cards", 4 }, { "highest_card", 5 }, { "lowest_permitted_card", 4 }, { "num_pile_cards", 4 }, { "num_stock_cards", 11 }, { "top_card_pile_value", 5 }, { "has_2", false }, { "has_3", false }, { "has_7", false }, { "has_10", false }, { "action", "PlayLowestPermitted" }, { "q_value", 0.350f } },
            new Dictionary<string, object> { { "num_cards", 7 }, { "highest_card", 9 }, { "lowest_permitted_card", 9 }, { "num_pile_cards", 5 }, { "num_stock_cards", 7 }, { "top_card_pile_value", 9 }, { "has_2", true }, { "has_3", false }, { "has_7", true }, { "has_10", true }, { "action", "PlayMultipleLowestPermitted" }, { "q_value", 0.728f } },
            new Dictionary<string, object> { { "num_cards", 6 }, { "highest_card", 13 }, { "lowest_permitted_card", 11 }, { "num_pile_cards", 6 }, { "num_stock_cards", 8 }, { "top_card_pile_value", 11 }, { "has_2", true }, { "has_3", true }, { "has_7", false }, { "has_10", true }, { "action", "PlayMultipleHighest" }, { "q_value", 0.684f } },
            new Dictionary<string, object> { { "num_cards", 12 }, { "highest_card", 12 }, { "lowest_permitted_card", 7 }, { "num_pile_cards", 9 }, { "num_stock_cards", 9 }, { "top_card_pile_value", 12 }, { "has_2", false }, { "has_3", false }, { "has_7", true }, { "has_10", false }, { "action", "PlayHighest" }, { "q_value", 0.395f } },
            new Dictionary<string, object> { { "num_cards", 11 }, { "highest_card", 11 }, { "lowest_permitted_card", 8 }, { "num_pile_cards", 11 }, { "num_stock_cards", 14 }, { "top_card_pile_value", 10 }, { "has_2", false }, { "has_3", true }, { "has_7", false }, { "has_10", false }, { "action", "PlayThree" }, { "q_value", 0.274f } },
            new Dictionary<string, object> { { "num_cards", 10 }, { "highest_card", 8 }, { "lowest_permitted_card", 4 }, { "num_pile_cards", 6 }, { "num_stock_cards", 7 }, { "top_card_pile_value", 8 }, { "has_2", false }, { "has_3", true }, { "has_7", true }, { "has_10", false }, { "action", "PlayMultipleThree" }, { "q_value", 0.533f } },
            new Dictionary<string, object> { { "num_cards", 13 }, { "highest_card", 5 }, { "lowest_permitted_card", 6 }, { "num_pile_cards", 8 }, { "num_stock_cards", 10 }, { "top_card_pile_value", 6 }, { "has_2", true }, { "has_3", false }, { "has_7", false }, { "has_10", true }, { "action", "PlayLowestPermitted" }, { "q_value", 0.889f } },
            new Dictionary<string, object> { { "num_cards", 5 }, { "highest_card", 6 }, { "lowest_permitted_card", 5 }, { "num_pile_cards", 6 }, { "num_stock_cards", 6 }, { "top_card_pile_value", 5 }, { "has_2", false }, { "has_3", false }, { "has_7", true }, { "has_10", false }, { "action", "PlaySeven" }, { "q_value", 0.189f } },
            new Dictionary<string, object> { { "num_cards", 10 }, { "highest_card", 10 }, { "lowest_permitted_card", 10 }, { "num_pile_cards", 10 }, { "num_stock_cards", 5 }, { "top_card_pile_value", 10 }, { "has_2", true }, { "has_3", true }, { "has_7", false }, { "has_10", true }, { "action", "PlayMultipleTen" }, { "q_value", 0.741f } },
            new Dictionary<string, object> { { "num_cards", 9 }, { "highest_card", 9 }, { "lowest_permitted_card", 9 }, { "num_pile_cards", 9 }, { "num_stock_cards", 9 }, { "top_card_pile_value", 9 }, { "has_2", true }, { "has_3", true }, { "has_7", true }, { "has_10", false }, { "action", "PlayMultipleLowestPermitted" }, { "q_value", 0.462f } }
};


        var testExample = new Dictionary<string, object?> {
            { "num_cards", 6 }, { "highest_card", 6 }, { "lowest_permitted_card", 6 }, { "num_pile_cards", 6 },
            { "num_stock_cards", 6 }, { "top_card_pile_value", 6 }, { "has_2", true }, { "has_3", true },
            { "has_7", true }, { "has_10", true }, { "action", "PlayMultipleLowestPermitted" }
        };
        
        var tilde = new TILDE_RT(10, 2);
        tilde.Train(toyList);
        TILDE_RT.PrintTree(tilde.Root, 10);
        double? result = tilde.Predict(tilde.Root, testExample);
        Console.WriteLine(result);
        }

*/