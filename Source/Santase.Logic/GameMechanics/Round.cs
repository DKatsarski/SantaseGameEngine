﻿namespace Santase.Logic.GameMechanics
{
    using System.Collections.Generic;

    using Santase.Logic.Cards;
    using Santase.Logic.Players;
    using Santase.Logic.RoundStates;

    internal class Round
    {
        private readonly IGameRules gameRules;

        private readonly IDeck deck;

        private readonly IStateManager stateManager;

        private readonly RoundPlayerInfo firstPlayer;

        private readonly RoundPlayerInfo secondPlayer;

        private PlayerPosition firstToPlay;

        public Round(IPlayer firstPlayer, IPlayer secondPlayer, IGameRules gameRules)
        {
            this.gameRules = gameRules;
            this.deck = new Deck();
            this.stateManager = new StateManager();

            this.firstPlayer = new RoundPlayerInfo(firstPlayer);
            this.secondPlayer = new RoundPlayerInfo(secondPlayer);

            this.firstToPlay = PlayerPosition.FirstPlayer;
        }

        public RoundResult Play()
        {
            this.StartRound(this.firstPlayer);
            this.StartRound(this.secondPlayer);

            while (!this.IsFinished())
            {
                this.PlayTrick();
            }

            this.firstPlayer.Player.EndRound();
            this.secondPlayer.Player.EndRound();

            return new RoundResult(this.firstPlayer, this.secondPlayer);
        }

        private void PlayTrick()
        {


            var trick = this.firstToPlay == PlayerPosition.FirstPlayer
                ? new Trick(this.firstPlayer, this.secondPlayer, this.stateManager, this.deck, this.gameRules)
                : new Trick(this.secondPlayer, this.firstPlayer, this.stateManager, this.deck, this.gameRules);

            var trickWinner = trick.Play();

            // The one who wins the trick should play first
            this.firstToPlay = trickWinner == this.firstPlayer
                                   ? PlayerPosition.FirstPlayer
                                   : PlayerPosition.SecondPlayer;

            if (this.stateManager.State.ShouldDrawCardBeforeTurn)
            {
                // The player who wins last trick takes card first
                if (this.firstToPlay == PlayerPosition.FirstPlayer)
                {
                    this.GiveCardToPlayer(this.firstPlayer);
                    this.GiveCardToPlayer(this.secondPlayer);
                }
                else
                {
                    this.GiveCardToPlayer(this.secondPlayer);
                    this.GiveCardToPlayer(this.firstPlayer);
                }
            }

            this.stateManager.State.PlayHand(this.deck.CardsLeft);
        }

        private bool IsFinished()
        {
            if (this.firstPlayer.RoundPoints >= this.gameRules.RoundPointsForGoingOut)
            {
                return true;
            }

            if (this.secondPlayer.RoundPoints >= this.gameRules.RoundPointsForGoingOut)
            {
                return true;
            }

            // No cards left => round over
            return this.firstPlayer.Cards.Count == 0 && this.secondPlayer.Cards.Count == 0;
        }

        private void StartRound(RoundPlayerInfo player)
        {
            var cards = new List<Card>();

            // TODO: 6 as constant
            for (int i = 0; i < 6; i++)
            {
                var card = this.deck.GetNextCard();
                cards.Add(card);
                player.Cards.Add(card);
            }

            player.Player.StartRound(cards, this.deck.TrumpCard);
        }
    }
}
