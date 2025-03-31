using System.Security.AccessControl;

namespace ZweedsPesten_Agent;

public class Player(int id) {
    public int Id { get; set; } = id;
    public List<Card> Closed { get; set; } = [];
    public List<Card> Open { get; set; } = [];
    public List<Card> Hand { get; set; } = [];

    public Player(Player player) : this(player.Id) {
        Id = player.Id;
        Closed = new List<Card>(player.Closed.Select(card => new Card(card.Suit, card.Rank)));
        Open = new List<Card>(player.Open.Select(card => new Card(card.Suit, card.Rank)));
        Hand = new List<Card>(player.Hand.Select(card => new Card(card.Suit, card.Rank)));
    }

    public void DrawToHand(Card card) {
        Hand.Add(card);
    }

    public void DrawToClosed(Card card) {
        Closed.Add(card);
    }

    public void DrawToOpen(Card card) {
        Open.Add(card);
    }
    
    public List<Card> GetListOfCards() {
        if (Hand.Count > 0) {
            return Hand;
        }

        if (Open.Count > 0) {
            return Open;
        }

        if (Closed.Count > 0) {
            return Closed;
        }
        return [];
    }
    
    public Card GetHighestCard() {
        var cards = GetListOfCards();
        var highestCard = cards.Count > 0 ? cards
            .Where(card => Enum.IsDefined(typeof(NonSpecialRank), (int)card.Rank))
            .MaxBy(card => card.Rank) : new Card(Suit.NonExist, Rank.NonExist);
        if (highestCard == null) {
            highestCard = cards
                .Where(card => Enum.IsDefined(typeof(Rank), (int)card.Rank))
                .MaxBy(card => card.Rank);
        }
        
        return highestCard ?? new Card(Suit.NonExist, Rank.NonExist);
    }
    
    public Card GetLowestCard() {
        var cards = GetListOfCards();
        var lowestCard = cards.Count > 0 ? cards
            .Where(card => Enum.IsDefined(typeof(NonSpecialRank), (int)card.Rank))
            .MinBy(card => card.Rank) : new Card(Suit.NonExist, Rank.NonExist);
        if (lowestCard == null || lowestCard.Suit == Suit.NonExist) {
            lowestCard = cards
                .Where(card => Enum.IsDefined(typeof(Rank), (int)card.Rank))
                .MinBy(card => card.Rank);
        }

        return lowestCard ?? new Card(Suit.NonExist, Rank.NonExist);
    }
    
    public Card GetLowestPermittedCard(Rank topCardRank) {
        if (topCardRank == Rank.NonExist) {
            return GetLowestCard();
        }
        var cards = GetListOfCards();
        if (topCardRank != Rank.Seven) {
            var lowestPermittedCard = cards.Count > 0 ? cards
                .Where(card => Enum.IsDefined(typeof(NonSpecialRank), (int)card.Rank))    
                .Where(card => card.Rank >= topCardRank)
                .MinBy(card => card.Rank) : new Card(Suit.NonExist, Rank.NonExist);
            if (lowestPermittedCard == null) {
                lowestPermittedCard = cards.Count > 0 ? cards
                    .Where(card => Enum.IsDefined(typeof(Rank), (int)card.Rank))    
                    .Where(card => card.Rank >= topCardRank)
                    .MinBy(card => card.Rank) : new Card(Suit.NonExist, Rank.NonExist);
            }
            

            return lowestPermittedCard ?? new Card(Suit.NonExist, Rank.NonExist);
        }

        var lowestPermittedCard2 = cards.Count > 0
            ? cards
                .Where(card => Enum.IsDefined(typeof(NonSpecialRank), (int)card.Rank))
                .Where(card => card.Rank < Rank.Seven)
                .MinBy(card => card.Rank)
            : new Card(Suit.NonExist, Rank.NonExist);
        if (lowestPermittedCard2 == null) {
            lowestPermittedCard2 = cards.Count > 0
                ? cards
                    .Where(card => Enum.IsDefined(typeof(Rank), (int)card.Rank))
                    .Where(card => card.Rank < Rank.Seven)
                    .MinBy(card => card.Rank) ?? new Card(Suit.NonExist, Rank.NonExist)
                : new Card(Suit.NonExist, Rank.NonExist);
        }

        return lowestPermittedCard2;
    }

    public Card GetRandomCard(Random rnd) {
        var cards = GetListOfCards();
        return cards[rnd.Next(0, cards.Count)];
    }

    public bool HasTwo() {
        var cards = GetListOfCards();
        return cards.Any(card => (int)card.Rank == 2);
    }

    public bool HasThree() {
        var cards = GetListOfCards();
        return cards.Any(card => (int)card.Rank == 3);
    }

    public bool HasSeven() {
        var cards = GetListOfCards();
        return cards.Any(card => (int)card.Rank == 7);
    }

    public bool HasTen() {
        var cards = GetListOfCards();
        return cards.Any(card => (int)card.Rank == 10);
    }

    public Card GetLowestValueCard() {
        var cards = GetListOfCards();
        var lowestValueCard = cards
            .Where(card => Enum.IsDefined(typeof(NonSpecialRank), (int)card.Rank))
            .MinBy(card => card.Rank);

        if (lowestValueCard == null) {
            lowestValueCard = cards
                .Where(card => Enum.IsDefined(typeof(Rank), (int)card.Rank))
                .MinBy(card => card.Rank);
        }

        return lowestValueCard;
    }

    public int GetNumCards() {
        return GetListOfCards().Count;
    }

    public List<int> GetNumCardsOpps(List<Player> opponents) {
        List<int> numCardsOpps = [];
        numCardsOpps.AddRange(opponents.Select(opponent => opponent.GetListOfCards().Count));
        return numCardsOpps;
    }

    public override string ToString() {
        string result = "Player " + Id + '\n' +
                        "Closed Cards: " + String.Join(", ", Closed) + '\n' +
                        "Open Cards: " + String.Join(", ", Open) + '\n' +
                        "Cards in Hand: " + String.Join(", ", Hand) + '\n';
        return result;
    }
    
}