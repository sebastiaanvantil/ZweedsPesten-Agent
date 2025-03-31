namespace ZweedsPesten_Agent;

public class Stock {
    public readonly Stack<Card> Cards = [];
    
    public Stock() {
        foreach (Suit suit in Enum.GetValues(typeof(Suit))) {
            if (suit != Suit.NonExist) {
                foreach (Rank rank in Enum.GetValues(typeof(Rank))) {
                    if (rank != Rank.NonExist) Cards.Push(new Card(suit, rank));
                }
            }
        }
        ShuffleStock();
    }

    public Stock(Stock stock) {
        // new Stack<Card>(stock.Cards) reverses the order of the stack. We also need to deep copy the cards.
        Cards = new Stack<Card>(stock.Cards.Reverse().Select(card => new Card(card.Suit, card.Rank)));
    }
    
    private void ShuffleStock() {
        var rng = new Random();
        int n = Cards.Count;
        var dummyList = Cards.ToList();
        Cards.Clear();
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            (dummyList[k], dummyList[n]) = (dummyList[n], dummyList[k]);
            Cards.Push(dummyList[n]);
        }
    }
    
    public Card Draw() {
        return Cards.Pop();
    }

    public override string ToString() {
        return string.Join(", ", Cards);
    }
}