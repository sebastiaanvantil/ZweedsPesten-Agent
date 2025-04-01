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
            Console.WriteLine("turn: " + i);
            
            gameOver = GoalConditionReached();
            i++;
        }
    }
    
    // The game is over if there is any player that has no cards left
    private bool GoalConditionReached() {
        return Players.Any(player => player.GetListOfCards().Count == 0);
    }

    private Player WinningPlayer() {
        return Players.First(player => player.GetListOfCards() == (List<Card>)[]);
    }

    public void Turn(Player player) {
        var opponents = Players.Where(p => p != player).ToList();
        var currentState = new MCTSState(PlayerQueue, Players, Pile, Stock);

        var MCTSGame = new MCTSTree(currentState, _qFunction);
        var action = MCTSGame.RunMCTS(100);
        Action.PlayAction(player, currentState.Pile, action);
        while (Stock.Cards.Count > 0 && player.Hand.Count < 3) {
            var cardToDraw = Stock.Cards.Pop();
            player.DrawToHand(cardToDraw);
        }
        
        PlayerQueue.RemoveFirst();
        bool anotherTurn = OneMoreTurn(Pile, action);
        if (Pile.Cards.Count == 0 || anotherTurn) {
            PlayerQueue.AddFirst(player);
        }
        else {
            PlayerQueue.AddLast(player);
        }
        
        if (player.Id == AgentId) {
            History.Add((currentState, action));
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
        var QValues = new List<double> { };
        var examples = new List<Dictionary<string, object>> { };
        for (int i = History.Count - 1; i >= 0; i--) {
            var point = History[i];
            var example = new Dictionary<string, object>();
            var player = point.Item1.PlayerQueue.First();
            var pile = point.Item1.Pile;
            var stock = point.Item1.Stock;
            example["num_cards"] = player.GetNumCards();
            example["highest_card"] = (int)player.GetHighestCard().Rank;
            example["lowest_permitted_card"] = (int)player.GetLowestPermittedCard(pile.Cards.Peek().Rank).Rank;
            example["num_pile_cards"] = pile.Cards.Count;
            example["num_stock_cards"] = stock.Cards.Count;
            example["top_card_pile_value"] = pile.Cards.Peek().Rank;
            example["has_2"] = player.HasTwo();
            example["has_3"] = player.HasThree();
            example["has_7"] = player.HasSeven();
            example["has_10"] = player.HasTen();
            example["action"] = point.Item2.ToString();

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
}