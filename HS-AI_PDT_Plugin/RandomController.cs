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
        public List<string> cardsToDraw = new List<string>();
        public List<string> cardsToPick = new List<string>();
        public List<string> cardsToDiscover = new List<string>();

        public override IPlayable PickDraw(Controller c)
        {
            if (c.PlayerId == 1)
            {
                IPlayable cardDrawn = c.DeckZone.GetAll.Where(ip => ip.Card.Name == cardsToDraw[0]).First();
                cardsToDraw.RemoveAt(0);
                return cardDrawn;
            }
            else
            {
                return c.DeckZone[0];
            }
        }

        public override Card PickCard(RandomCardTask task, IEntity source, IEntity target, List<Card> cards)
        {
            Card cardPicked = cards.Where(card => card.Name == cardsToPick[0]).First();
            cardsToPick.RemoveAt(0);
            return cardPicked;
        }

        public override Card PickDiscoverChoices(DiscoverType type, IEntity source, IEntity target, List<Card> cards)
        {
            Card cardPicked = cards.Where(card => card.Name == cardsToDiscover[0]).First();
            cardsToDiscover.RemoveAt(0);
            return cardPicked;
        }

        public override IPlayable PickTarget(EntityType type, IEntity source, IEntity target, List<IPlayable> entities)
        {
            return entities[entities.Count - 1];
        }
    }
}
