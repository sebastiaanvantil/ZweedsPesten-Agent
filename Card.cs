using System.Globalization;

namespace ZweedsPesten_Agent;

public enum Suit { Hearts = 0, Spades = 1, Diamonds = 2, Clubs = 3, NonExist = -1 }
public enum Rank { Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14, NonExist = -1 }
public enum SpecialRank { Two, Three, Seven, Ten }
public enum NonSpecialRank { Four = 4, Five = 5, Six = 6, Eight = 8, Nine = 9, Jack = 11, Queen = 12, King = 13, Ace = 14 }

public class Card {
    public Suit Suit { get; set; }
    public Rank Rank { get; set; }

    public Card(Suit suit, Rank rank) {
        Suit = suit;
        Rank = rank;
    }

    public override string ToString() => $"{Rank} of {Suit}";
}