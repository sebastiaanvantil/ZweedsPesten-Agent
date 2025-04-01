using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace ZweedsPesten_Agent;

public class Game {
    public List<Player> Players = [];
    public Stock Stock = new Stock();
    public Pile Pile = new Pile();
    public LinkedList<Player> PlayerQueue = [];
    public List<(MCTSState, Action.ActionType)> History = [];
    private TILDE_RT _qFunction;
    private int AgentId;

    public Game(int numPlayers, int agentId, TILDE_RT qFunction) {
        for (int i = 0; i < numPlayers; i++) {
            Players.Add(new Player(i));
        }
        AgentId = agentId;
        _qFunction = qFunction;
    }

    private void DealCards() {
        foreach (var player in Players) {
            for (int i = 0; i < 3; i++) {
                player.DrawToClosed(Stock.Draw());
                player.DrawToOpen(Stock.Draw());
                player.DrawToHand(Stock.Draw());
            }
        }
    }

    private Player GetStartingPlayer() {
        var startingPlayer = Players[0];
        var lowestValueCard = startingPlayer.GetLowestValueCard();
        foreach (var player in Players) {
            var lowestValueCardComparison = player.GetLowestValueCard();
            if (lowestValueCard.Rank > lowestValueCardComparison.Rank) {
                startingPlayer = player;
                lowestValueCard = lowestValueCardComparison;
            }
            else if (lowestValueCard.Rank == lowestValueCardComparison.Rank) {
                if (lowestValueCard.Suit > lowestValueCardComparison.Suit) {
                    startingPlayer = player;
                    lowestValueCard = lowestValueCardComparison;
                }
            }
        }
        
        return startingPlayer;
    }

    private void InitQueue(Player startingPlayer) {
        int startingPlayerId = startingPlayer.Id;
        for (int i = startingPlayerId; i < Players.Count; i++) {
            PlayerQueue.AddLast(Players[i]);
        }

        for (int i = 0; i < startingPlayerId; i++) {
            PlayerQueue.AddLast(Players[i]);
        }
    }

    public void Run() {
        DealCards();
        var startingPlayer = GetStartingPlayer();
        InitQueue(startingPlayer);
        
        bool gameOver = false;
        int i = 0;
        while (!gameOver) {
            var player = PlayerQueue.First();
            Turn(player);
            Console.WriteLine("Turn: " + i);
            
            gameOver = GoalConditionReached();
            i++;
        }
        
    }
    
    // The game is over if there is any player that has no cards left
    private bool GoalConditionReached() {
        return Players.Any(player => player.GetListOfCards().Count == 0);
    }

    private Player WinningPlayer() {
        var winningPlayer = Players.First(player => player.GetListOfCards() == (List<Card>)[]);
        Console.WriteLine("Winning Player: " + winningPlayer.Id + ", Agent: " + AgentId);
        return winningPlayer;
    }

    public void Turn(Player player) {
        var opponents = Players.Where(p => p != player).ToList();
        var currentState = new MCTSState(PlayerQueue, Players, Pile, Stock);

        var MCTSGame = new MCTSTree(currentState, _qFunction);
        /*
         code for cutting out the MCTS.
        var possibleActions = Action.GetAllowedActions(player, currentState.Pile, opponents);
        var bestAction = possibleActions[0];
        var example = GetExample(player, currentState.Pile, currentState.Stock, bestAction);
        var bestQValue = _qFunction.Predict(_qFunction.Root, example);
        foreach (var action in possibleActions) {
            example = GetExample(player, currentState.Pile, currentState.Stock, action);
            var QValue = _qFunction.Predict(_qFunction.Root, example);
            if (QValue > bestQValue) {
                bestQValue = QValue;
                bestAction = action;
            }
        }*/
        
        var bestAction = MCTSGame.RunMCTS(100);
        Action.PlayAction(player, Pile, bestAction);
        while (Stock.Cards.Count > 0 && player.Hand.Count < 3) {
            var cardToDraw = Stock.Cards.Pop();
            player.DrawToHand(cardToDraw);
        }
        
        PlayerQueue.RemoveFirst();
        bool anotherTurn = OneMoreTurn(Pile, bestAction);
        if (anotherTurn) {
            PlayerQueue.AddFirst(player);
        }
        else {
            PlayerQueue.AddLast(player);
        }
        
        if (player.Id == AgentId) {
            History.Add((currentState, bestAction));
        }
    }
    
    public static bool OneMoreTurn(Pile pile, Action.ActionType action) {
        if (action == Action.ActionType.PickUpPile) {
            return false;
        }
        if (pile.Cards.Count == 0) {
            return true;
        }
        var tempStack = new Stack<Card>();
        var topCard = pile.Cards.Peek();
        if (topCard.Rank == Rank.Ten) {
            pile.Burn();
            return true;
        }
        var lookaheadCard = pile.Cards.Pop();
        tempStack.Push(topCard);
        int numSameCards = 0;
        while (pile.Cards.Count > 0 && (lookaheadCard.Rank == topCard.Rank || lookaheadCard.Rank == Rank.Three)) {
            lookaheadCard = pile.Cards.Pop();
            tempStack.Push(lookaheadCard);
            if (lookaheadCard.Rank == topCard.Rank) {
                numSameCards++;
            }
        }

        if (numSameCards == 4) {
            pile.Burn();
            return true;
        }
        // Restore original Pile if no set of 4 is found
        while (tempStack.Count > 0) {
            lookaheadCard = tempStack.Pop();
            pile.Cards.Push(lookaheadCard);
        }

        return false;
    }

    public List<Dictionary<string, object>> GetExamplesFromHistory() {
        var QValues = Enumerable.Repeat((double)0, History.Count).ToList();
        var examples = new List<Dictionary<string, object>> { };
        for (int i = History.Count - 1; i >= 0; i--) {
            var point = History[i];
            var player = point.Item1.PlayerQueue.First();
            var pile = point.Item1.Pile;
            var stock = point.Item1.Stock;
            var action = point.Item2;

            var example = GetExample(player, pile, stock, action);

            int reward = 0;
            if (MCTSTree.GoalConditionReached(point.Item1)) {
                bool won = WinningPlayer().Id == AgentId;
                reward = won ? 1 : 0;
            }

            double qValue = 0;
            if (i < History.Count - 1) {
                qValue = reward + QValues[i + 1];
            }
            else {
                qValue = reward;
            }
            QValues[i] = qValue;
            example["q_value"] = qValue;
            examples.Add(example);
        }

        return examples;
    }

    private Dictionary<string, object> GetExample(Player player, Pile pile, Stock stock, Action.ActionType action) {
        var topCard = pile.Cards.Count > 0 ? pile.Cards.Peek() : new Card(Suit.NonExist, Rank.NonExist);
        var example = new Dictionary<string, object> {
            ["num_cards"] = player.GetNumCards(),
            ["highest_card"] = (int)player.GetHighestCard().Rank,
            ["lowest_permitted_card"] = (int)player.GetLowestPermittedCard(topCard.Rank).Rank,
            ["num_pile_cards"] = pile.Cards.Count,
            ["num_stock_cards"] = stock.Cards.Count,
            ["top_card_pile_value"] = pile.Cards.Count > 0 ? pile.Cards.Peek().Rank : 0,
            ["has_2"] = player.HasTwo(),
            ["has_3"] = player.HasThree(),
            ["has_7"] = player.HasSeven(),
            ["has_10"] = player.HasTen(),
            ["action"] = action.ToString(),
        };

        return example;
    }
}