namespace ZweedsPesten_Agent;

public class Action {
    public enum ActionType {
        PlayTwo,
        PlayMultipleTwo,
        PlayThree,
        PlayMultipleThree,
        PlaySeven,
        PlayMultipleSeven,
        PlayTen,
        PlayMultipleTen,
        PlayHighest,
        PlayMultipleHighest,
        PlayLowestPermitted,
        PlayMultipleLowestPermitted,
        PickUpPile,
    }

    public static List<ActionType> GetAllowedActions(Player player, Pile pile, List<Player> opponents) {
        var actions = new List<ActionType>();
        var topCard = new Card(Suit.NonExist, Rank.NonExist);
        
        if (pile.Cards.Count > 0) {
            topCard = pile.Cards.Peek();
        }
        
        if (IsValidPlay(player.GetHighestCard().Rank, topCard.Rank)) {
            actions.Add(ActionType.PlayHighest);
            actions.Add(ActionType.PlayMultipleHighest);
        }

        var lowestPermittedCard = player.GetLowestPermittedCard(topCard.Rank).Rank;
        if (IsValidPlay(lowestPermittedCard, topCard.Rank)) {
            actions.Add(ActionType.PlayLowestPermitted);
            actions.Add(ActionType.PlayMultipleLowestPermitted);
        }

        if (pile.Cards.Count > 0) {
            actions.Add(ActionType.PickUpPile);
        }
        
        if (player.HasTwo()) {
            actions.Add(ActionType.PlayTwo); 
            actions.Add(ActionType.PlayMultipleTwo);
        }
        if (player.HasThree()) {
            actions.Add(ActionType.PlayThree);
            actions.Add(ActionType.PlayMultipleThree);
        }
        if (player.HasSeven() && IsValidPlay(Rank.Seven, topCard.Rank)) {
            actions.Add(ActionType.PlaySeven);
            actions.Add(ActionType.PlayMultipleSeven);
        }
        if (player.HasTen() && IsValidPlay(Rank.Ten, topCard.Rank)) {
            actions.Add(ActionType.PlayTen);
            actions.Add(ActionType.PlayMultipleTen);
        }

        return actions;
    }

    private static bool IsValidPlay(Rank cardRank, Rank topCardRank) {
        if (topCardRank == Rank.NonExist) {
            return true;
        }
        if (cardRank == Rank.NonExist) {
            return false;
        }
        if (cardRank == Rank.Two || cardRank == Rank.Three) {
            return true;
        }
        if (topCardRank != Rank.Seven) {
            return (int)topCardRank <= (int)cardRank;
        }
        
        // if the Rank of the topCard is 7, then return true if card.Rank < topCard.Rank
        return topCardRank > cardRank;
    }

    public static void PlayHighest(Player player, Pile pile) {
        var maxRank = player.GetHighestCard().Rank;
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, maxRank);
    }

    public static void PlayMultipleHighest(Player player, Pile pile) {
        var maxRank = player.GetHighestCard().Rank;
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, maxRank);
    }

    public static void PlayLowestPermitted(Player player, Pile pile) {
        var topCard = new Card(Suit.NonExist, Rank.NonExist);
        var minRank = Rank.Two;
        if (pile.Cards.Count > 0) {
            topCard = pile.Cards.Peek();
            minRank = player.GetLowestPermittedCard(topCard.Rank).Rank;
        }
        else {
            minRank = player.GetLowestPermittedCard(minRank).Rank;
        }
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, minRank);
    }

    public static void PlayMultipleLowestPermitted(Player player, Pile pile) {
        var topCard = new Card(Suit.NonExist, Rank.NonExist);
        if (pile.Cards.Count > 0) {
            topCard = pile.Cards.Peek();
        }
        var minRank = player.GetLowestPermittedCard(topCard.Rank).Rank;
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, minRank);
    }

    public static void PlayRandom(Player player, Pile pile, Random rnd) {
        var playerCards = player.GetListOfCards();
        var randomRank = player.GetRandomCard(rnd).Rank;
        PlayRank(playerCards, pile, randomRank);
    }

    public static void PlayTwo(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Two);
    }

    public static void PlayMultipleTwo(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Two);
    }

    public static void PlayThree(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Three);
    }

    public static void PlayMultipleThree(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Three);
    }

    public static void PlaySeven(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Seven);
    }

    public static void PlayMultipleSeven(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Seven);
    }

    public static void PlayTen(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Ten);
        pile.Burn();
    }

    public static void PlayMultipleTen(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Ten);
        pile.Burn();
    }

    private static void PlayRank(List<Card> playerCards, Pile pile, Rank rank) {
        int index = playerCards.FindIndex(card => card.Rank == rank);
        List<Card> cardToPlay = [playerCards[index]];
        playerCards.RemoveAt(index);
        pile.Add(cardToPlay);
    }
    
    private static void PlayMultipleRank(List<Card> playerCards, Pile pile, Rank rank) {
        var cardsToPlay = playerCards.FindAll(card => card.Rank == rank);
        foreach (var card in cardsToPlay) {
            playerCards.Remove(card);
        }
        pile.Add(cardsToPlay);
    }

    private static void PickUpPile(Player player, Pile pile) {
        pile.Reset(player);
    }

    public static void PlayAction(Player player, Pile pile, ActionType actionType) {
        if (actionType == Action.ActionType.PlayTwo) 
            PlayTwo(player, pile);
        if (actionType == Action.ActionType.PlayMultipleTwo)
            PlayMultipleTwo(player, pile);
        if (actionType == Action.ActionType.PlayThree)
            PlayThree(player, pile);
        if (actionType == Action.ActionType.PlayMultipleThree)
            PlayMultipleThree(player, pile);
        if (actionType == Action.ActionType.PlaySeven)
            PlaySeven(player, pile);
        if (actionType == Action.ActionType.PlayMultipleSeven)
            PlayMultipleSeven(player, pile);
        if (actionType == Action.ActionType.PlayTen)
            PlayTen(player, pile);
        if (actionType == Action.ActionType.PlayMultipleTen)
            PlayMultipleTen(player, pile);
        if (actionType == Action.ActionType.PlayHighest)
            PlayHighest(player, pile);
        if (actionType == Action.ActionType.PlayMultipleHighest)
            PlayMultipleHighest(player, pile);
        if (actionType == Action.ActionType.PlayLowestPermitted)
            PlayLowestPermitted(player, pile);
        if (actionType == Action.ActionType.PlayMultipleLowestPermitted)
            PlayMultipleLowestPermitted(player, pile);
        if (actionType == Action.ActionType.PickUpPile)
            PickUpPile(player, pile);
    }
}