namespace ZweedsPesten_Agent;

public class Game {
    private readonly List<Player> _players = [];
    private readonly Stock _stock = new Stock();
    private readonly Pile _pile = new Pile();
    private readonly LinkedList<Player> _playerQueue = [];
    private readonly List<(MCTSState, Action.ActionType)> _history = [];
    private readonly TILDE_RT _qFunction;
    private readonly int _agentId;

    public Game(int numPlayers, int agentId, TILDE_RT qFunction) {
        for (int i = 0; i < numPlayers; i++) {
            _players.Add(new Player(i));
        }
        _agentId = agentId;
        _qFunction = qFunction;
    }

    private void DealCards() {
        foreach (var player in _players) {
            for (int i = 0; i < 3; i++) {
                player.DrawToClosed(_stock.Draw());
                player.DrawToOpen(_stock.Draw());
                player.DrawToHand(_stock.Draw());
            }
        }
    }

    private Player GetStartingPlayer() {
        var startingPlayer = _players[0];
        var lowestValueCard = startingPlayer.GetLowestValueCard();
        foreach (var player in _players) {
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
        for (int i = startingPlayerId; i < _players.Count; i++) {
            _playerQueue.AddLast(_players[i]);
        }

        for (int i = 0; i < startingPlayerId; i++) {
            _playerQueue.AddLast(_players[i]);
        }
    }

    public void Run() {
        DealCards();
        var startingPlayer = GetStartingPlayer();
        InitQueue(startingPlayer);
        
        bool gameOver = false;
        int i = 0;
        while (!gameOver) {
            var player = _playerQueue.First();
            Turn(player);
            Console.WriteLine("Turn: " + i);
            
            gameOver = GoalConditionReached();
            i++;
        }
        
    }
    
    // The game is over if there is any player that has no cards left
    private bool GoalConditionReached() {
        bool goal = _players.Any(player => player.GetListOfCards().Count == 0);
        if (goal) WinningPlayer();
        return goal;
    }

    private Player WinningPlayer() {
        var winningPlayer = _players.First(player => player.GetListOfCards().Count == 0);
        Console.WriteLine("Winning Player: " + winningPlayer.Id + ", Agent: " + _agentId);
        return winningPlayer;
    }

    private void Turn(Player player) {
        var currentState = new MCTSState(_playerQueue, _players, _pile, _stock);

        var mctsGame = new MCTSTree(currentState, _qFunction);
        
        var bestAction = mctsGame.RunMCTS(100);
        Action.PlayAction(player, _pile, bestAction);
        while (_stock.Cards.Count > 0 && player.Hand.Count < 3) {
            var cardToDraw = _stock.Cards.Pop();
            player.DrawToHand(cardToDraw);
        }
        
        _playerQueue.RemoveFirst();
        bool anotherTurn = OneMoreTurn(_pile, bestAction);
        if (anotherTurn) {
            _playerQueue.AddFirst(player);
        }
        else {
            _playerQueue.AddLast(player);
        }
        
        if (player.Id == _agentId) {
            _history.Add((currentState, bestAction));
        }
    }
    
    public static bool OneMoreTurn(Pile pile, Action.ActionType action) {
        if (action == Action.ActionType.PickUpPile) {
            return false;
        }
        if (pile.Cards.Count > 0) {
            if (pile.Cards.Peek().Rank == Rank.NonExist) {
                pile.Cards.Pop();
                return false;
            }
        }
        else {
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
        int numSameCards = 1;
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
        const double DiscountFactor = 0.99;
        var qValues = Enumerable.Repeat((double)0, _history.Count).ToList();
        var examples = new List<Dictionary<string, object>>();
        for (int i = _history.Count - 1; i >= 0; i--) {
            var (mctsState, action) = _history[i];
            var player = mctsState.PlayerQueue.First();
            var pile = mctsState.Pile;
            var stock = mctsState.Stock;

            var example = GetExample(player, pile, stock, action);

            int reward = 0;
            if (MCTSTree.GoalConditionReached(mctsState)) {
                bool won = WinningPlayer().Id == _agentId;
                reward = won ? 1 : 0;
            }

            double qValue;
            if (i < _history.Count - 1) {
                qValue = reward + DiscountFactor * qValues[i + 1];
            }
            else {
                qValue = reward;
            }
            qValues[i] = qValue;
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