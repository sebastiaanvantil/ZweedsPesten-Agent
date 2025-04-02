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
        PlayClosedCard,
        PlayOpenCard,
    }

    public static List<ActionType> GetAllowedActions(Player player, Pile pile, List<Player> opponents) {
        var actions = new List<ActionType>();
        var topCard = new Card(Suit.NonExist, Rank.NonExist);
        
        if (pile.Cards.Count > 0) {
            topCard = pile.Cards.Peek();
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
        if (IsValidPlay(player.GetHighestCard().Rank, topCard.Rank)) {
            actions.Add(ActionType.PlayHighest);
            actions.Add(ActionType.PlayMultipleHighest);
        }
        if (IsValidPlay(player.GetLowestPermittedCard(topCard.Rank).Rank, topCard.Rank)) {
            actions.Add(ActionType.PlayLowestPermitted);
            actions.Add(ActionType.PlayMultipleLowestPermitted);
        }

        if (player.Hand.Count == 0 && player.Open.Count > 0) {
            actions.Clear();
            actions.Add(ActionType.PlayOpenCard);
        }

        if (player.Open.Count == 0 && player.Hand.Count == 0 && player.Closed.Count > 0) {
            actions.Clear();
            actions.Add(ActionType.PlayClosedCard);
        }
        
        if (actions.Count == 0 && pile.Cards.Count > 0) {
            actions.Add(ActionType.PickUpPile);
        }

        return actions;
    }

    private static bool IsValidPlay(Rank cardRank, Rank topCardRank) {
        if (cardRank == Rank.NonExist) {
            return false;
        }
        if (topCardRank == Rank.NonExist) {
            return true;
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

    private static void PlayOpenCard(Player player, Pile pile) {
        var topCard = pile.Cards.Count > 0 ? pile.Cards.Peek() : new Card(Suit.NonExist, Rank.NonExist);
        var cardToPlay = player.GetLowestPermittedCard(topCard.Rank);
        if (IsValidPlay(cardToPlay.Rank, topCard.Rank)) {
            PlayLowestPermitted(player, pile);
        }
        else {
            if (cardToPlay.Rank != Rank.NonExist) {
                player.Open.Remove(cardToPlay);
                player.DrawToHand(cardToPlay);
            }
            PickUpPile(player, pile);
            pile.Cards.Push(new Card(Suit.NonExist, Rank.NonExist));
        }
    }

    private static void PlayClosedCard(Player player, Pile pile) {
        var rankToPlay = player.Closed[0].Rank;
        var topCard = pile.Cards.Count > 0 ? pile.Cards.Peek() : new Card(Suit.NonExist, Rank.NonExist);
        if (IsValidPlay(rankToPlay, topCard.Rank)) {
            var playerCards = player.Closed;
            PlayRank(playerCards, pile, rankToPlay);
        }
        else {
            var cardToPlay = player.Closed[0];
            player.Closed.Remove(cardToPlay);
            player.DrawToHand(cardToPlay);
            PickUpPile(player, pile);
        }
    }

    private static void PlayHighest(Player player, Pile pile) {
        var maxRank = player.GetHighestCard().Rank;
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, maxRank);
    }

    private static void PlayMultipleHighest(Player player, Pile pile) {
        var maxRank = player.GetHighestCard().Rank;
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, maxRank);
    }

    private static void PlayLowestPermitted(Player player, Pile pile) {
        var topCard = new Card(Suit.NonExist, Rank.NonExist);
        var minRank = Rank.Two;
        if (pile.Cards.Count > 0) {
            topCard = pile.Cards.Peek();
        }
        minRank = player.GetLowestPermittedCard(topCard.Rank).Rank;
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, minRank);
    }

    private static void PlayMultipleLowestPermitted(Player player, Pile pile) {
        var topCard = new Card(Suit.NonExist, Rank.NonExist);
        if (pile.Cards.Count > 0) {
            topCard = pile.Cards.Peek();
        }
        var minRank = player.GetLowestPermittedCard(topCard.Rank).Rank;
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, minRank);
    }

    private static void PlayTwo(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Two);
    }

    private static void PlayMultipleTwo(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Two);
    }

    private static void PlayThree(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Three);
    }

    private static void PlayMultipleThree(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Three);
    }

    private static void PlaySeven(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Seven);
    }

    private static void PlayMultipleSeven(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayMultipleRank(playerCards, pile, Rank.Seven);
    }

    private static void PlayTen(Player player, Pile pile) {
        var playerCards = player.GetListOfCards();
        PlayRank(playerCards, pile, Rank.Ten);
        pile.Burn();
    }

    private static void PlayMultipleTen(Player player, Pile pile) {
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
        if (actionType == ActionType.PlayOpenCard)
            PlayOpenCard(player, pile);
        if (actionType == ActionType.PlayClosedCard)
            PlayClosedCard(player, pile);
        if (actionType == ActionType.PlayTwo) 
            PlayTwo(player, pile);
        if (actionType == ActionType.PlayMultipleTwo)
            PlayMultipleTwo(player, pile);
        if (actionType == ActionType.PlayThree)
            PlayThree(player, pile);
        if (actionType == ActionType.PlayMultipleThree)
            PlayMultipleThree(player, pile);
        if (actionType == ActionType.PlaySeven)
            PlaySeven(player, pile);
        if (actionType == ActionType.PlayMultipleSeven)
            PlayMultipleSeven(player, pile);
        if (actionType == ActionType.PlayTen)
            PlayTen(player, pile);
        if (actionType == ActionType.PlayMultipleTen)
            PlayMultipleTen(player, pile);
        if (actionType == ActionType.PlayHighest)
            PlayHighest(player, pile);
        if (actionType == ActionType.PlayMultipleHighest)
            PlayMultipleHighest(player, pile);
        if (actionType == ActionType.PlayLowestPermitted)
            PlayLowestPermitted(player, pile);
        if (actionType == ActionType.PlayMultipleLowestPermitted)
            PlayMultipleLowestPermitted(player, pile);
        if (actionType == ActionType.PickUpPile)
            PickUpPile(player, pile);
    }
}