namespace ZweedsPesten_Agent;

public class MCTSState {
    public LinkedList<Player> PlayerQueue;
    public List<Player> Players;
    public Pile Pile;
    public Stock Stock;

    public MCTSState(LinkedList<Player> playerQueue, List<Player> players, Pile pile, Stock stock) {
        PlayerQueue = [];
        Players = [];
        foreach (var player in players) {
            var newPlayer = new Player(player);
            Players.Add(newPlayer);
        }

        foreach (var player in playerQueue) {
            int id = player.Id;
            PlayerQueue.AddLast(Players.First(p => p.Id == id));
        }
        Pile = new Pile(pile);
        Stock = new Stock(stock);
    }

    public MCTSState(MCTSState parent) {
        PlayerQueue = [];
        Players = [];
        foreach (var player in parent.Players) {
            var newPlayer = new Player(player);
            Players.Add(newPlayer);
        }

        foreach (var player in parent.PlayerQueue) {
            int id = player.Id;
            PlayerQueue.AddLast(Players.First(p => p.Id == id));
        }
        Pile = new Pile(parent.Pile);
        Stock = new Stock(parent.Stock);
    }
}

