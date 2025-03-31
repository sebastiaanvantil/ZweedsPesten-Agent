namespace ZweedsPesten_Agent;

public class Pile {
    public Stack<Card> Cards = [];
    public Stack<Card> BurnedCards = [];
    
    public Pile() {
        
    }

    public Pile(Pile pile) {
        Cards = new Stack<Card>(pile.Cards.Reverse().Select(card => new Card(card.Suit, card.Rank)));
        BurnedCards = new Stack<Card>(pile.BurnedCards.Select(card => new Card(card.Suit, card.Rank)));
    }
    
    public void Add(List<Card> cardsToAdd) {
        foreach (var card in cardsToAdd.ToList()) {
            Cards.Push(card);
        }
    }
    
    public void Reset(Player player) {
        while (Cards.Count > 0) {
            var card = Cards.Pop();
            player.DrawToHand(card);
        }
    }
    
    public void Burn() {
        Console.WriteLine("burning cards");
        while (Cards.Count > 0) {
            var burnedcard = Cards.Pop();
            BurnedCards.Push(burnedcard);
        }
    }
}