using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.SimpleTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_AI_PDT_Plugin
{
    public class RandomController : ThrowRandomController
    {
        public List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> cardsToDraw = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
        public List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> cardsToPick = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
        public List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> cardsToDiscover = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
        private Dictionary<int, int> _entityIdMapping;
        public bool RandomHappened = false;

        public RandomController(ref Dictionary<int, int> entityIdMapping)
        {
            _entityIdMapping = entityIdMapping;
        }

        public RandomController Clone()
        {
            RandomController clone = new RandomController(ref _entityIdMapping);
            clone.cardsToDraw = cardsToDraw;
            clone.cardsToPick = cardsToPick;
            clone.cardsToDiscover = cardsToDiscover;
            return clone;
        }

        public void Reset()
        {
            //cardsToDraw = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
            cardsToPick = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
            cardsToDiscover = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
        }

        public override IPlayable PickDraw(Controller c)
        {
            RandomHappened = true;
            if (c.PlayerId == 1)
            {
                try
                {
                    IPlayable cardDrawn = c.DeckZone.GetAll.Where(ip => ip.Card.Name == cardsToDraw[0].Card.Name).First();
                    if (!_entityIdMapping.ContainsKey(cardsToDraw[0].Id))
                        _entityIdMapping.Add(cardsToDraw[0].Id, cardDrawn.Id);
                    cardsToDraw.RemoveAt(0);
                    return cardDrawn;
                }
                catch (Exception e)
                {
                    throw e;
                }
                
            }
            else
            {
                return c.DeckZone[0];
            }
        }

        public override Card PickCard(RandomCardTask task, IEntity source, IEntity target, List<Card> cards)
        {
            RandomHappened = true;
            try
            {
                Card cardPicked = cards.Where(card => card.Name == cardsToPick[0].Card.Name).First();
                cardsToPick.RemoveAt(0);
                return cardPicked;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override Card PickDiscoverChoices(DiscoverType type, IEntity source, IEntity target, List<Card> cards)
        {
            RandomHappened = true;
            try
            {
                Card cardPicked = cards.Where(card => card.Name == cardsToDiscover[0].Card.Name).First();
                cardsToDiscover.RemoveAt(0);
                return cardPicked;
            }
            catch (Exception e)
            {
                throw e;
            }
}

        public override IPlayable PickTarget(EntityType type, IEntity source, IEntity target, List<IPlayable> entities)
        {
            RandomHappened = true;
            return entities[entities.Count - 1];
        }
    }
}
