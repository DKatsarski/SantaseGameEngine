﻿namespace Santase.UI.WindowsUniversal
{
    using System;
    using System.Collections.Generic;

    using Santase.Logic.Cards;
    using Santase.Logic.Players;

    public class UiPlayer : BasePlayer
    {
        private PlayerTurnContext currentContext;

        private PlayerAction userAction;

        private bool iAmFirstThisTurn = false;

        public event EventHandler<ICollection<Card>> RedrawCards;

        public event EventHandler<Card> RedrawTrumpCard;

        public event EventHandler<Card> RedrawPlayerPlayedCard;

        public event EventHandler<Card> RedrawOtherPlayerPlayedCard;

        public event EventHandler<int> RedrawNumberOfCardsLeftInDeck;

        public event EventHandler<Tuple<int, int>> RedrawCurrentAndOtherPlayerRoundPoints;

        public event EventHandler<Tuple<int, int>> RedrawCurrentAndOtherPlayerTotalPoints;

        public event EventHandler<Tuple<Card, Card>> RedrawPlayedCards;

        public override string Name => "UI Player";

        public override void StartRound(ICollection<Card> cards, Card trumpCard, int myTotalPoints, int opponentTotalPoints)
        {
            this.RedrawCurrentAndOtherPlayerTotalPoints?.Invoke(this, new Tuple<int, int>(myTotalPoints, opponentTotalPoints));
            this.RedrawCards?.Invoke(this, cards);
            this.RedrawTrumpCard?.Invoke(this, trumpCard);
            base.StartRound(cards, trumpCard, myTotalPoints, opponentTotalPoints);
        }

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            this.iAmFirstThisTurn = context.IsFirstPlayerTurn;
            this.UpdateContextInfo(context);
            this.currentContext = context;
            while (this.userAction == null)
            {
            }

            lock (this.userAction)
            {
                PlayerAction action;
                switch (this.userAction.Type)
                {
                    case PlayerActionType.PlayCard:
                        action = this.PlayCard(this.userAction.Card);
                        this.RedrawCards?.Invoke(this, this.Cards);
                        this.RedrawPlayerPlayedCard?.Invoke(this, this.userAction.Card);
                        break;
                    case PlayerActionType.ChangeTrump:
                        action = this.ChangeTrump(context.TrumpCard);
                        this.RedrawCards?.Invoke(this, this.Cards);
                        break;
                    case PlayerActionType.CloseGame:
                        action = this.CloseGame();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(this.userAction.Type));
                }

                this.userAction = null;
                return action;
            }
        }

        public override void EndTurn(PlayerTurnContext context)
        {
            this.UpdateContextInfo(context);
            var playedCards = this.iAmFirstThisTurn
                                  ? new Tuple<Card, Card>(context.FirstPlayedCard, context.SecondPlayedCard)
                                  : new Tuple<Card, Card>(context.SecondPlayedCard, context.FirstPlayedCard);
            this.RedrawPlayedCards?.Invoke(this, playedCards);
            base.EndTurn(context);
        }

        public override void AddCard(Card card)
        {
            base.AddCard(card);
            this.RedrawCards?.Invoke(this, this.Cards);
        }

        public void Action(PlayerAction playerAction)
        {
            if (!this.PlayerActionValidator.IsValid(playerAction, this.currentContext, this.Cards))
            {
                // Invalid player action
                return;
            }

            this.userAction = playerAction;
        }

        private void UpdateContextInfo(PlayerTurnContext context)
        {
            var roundPointsInfo =
                new Tuple<int, int>(
                    context.IsFirstPlayerTurn ? context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints,
                    context.IsFirstPlayerTurn ? context.SecondPlayerRoundPoints : context.FirstPlayerRoundPoints);
            this.RedrawCurrentAndOtherPlayerRoundPoints?.Invoke(this, roundPointsInfo);
            this.RedrawTrumpCard?.Invoke(this, context.TrumpCard);
            this.RedrawNumberOfCardsLeftInDeck?.Invoke(this, context.CardsLeftInDeck);
            this.RedrawOtherPlayerPlayedCard?.Invoke(this, context.FirstPlayedCard);
        }
    }
}
