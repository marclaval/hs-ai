using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;

namespace HSAI.Deck
{
    class DeckBuilder
    {

        public static Deck DeserializeDeckString(string deckString)
        {
            var deck = new Deck();
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(deckString);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Input is not a valid deck string.", e);
            }
            var offset = 0;
            ulong Read()
            {
                if (offset > bytes.Length)
                    throw new ArgumentException("Input is not a valid deck string.");
                var value = VarInt.ReadNext(bytes.Skip(offset).ToArray(), out var length);
                offset += length;
                return value;
            }

            //Zero byte
            offset++;

            //Version - always 1
            Read();

            deck.Format = (FormatType)Read();

            //Num Heroes - always 1
            Read();

            deck.HeroClass = Cards.FromAssetId((int)Read()).Class;

            void AddCard(int? dbfId = null, int count = 1)
            {
                dbfId = dbfId ?? (int)Read();
                for (var i = 0; i < count; i++) {
                   deck.CardList.Add(Cards.FromAssetId(dbfId.Value));
                }
            }

            var numSingleCards = (int)Read();
            for (var i = 0; i < numSingleCards; i++)
                AddCard();

            var numDoubleCards = (int)Read();
            for (var i = 0; i < numDoubleCards; i++)
                AddCard(count: 2);

            var numMultiCards = (int)Read();
            for (var i = 0; i < numMultiCards; i++)
            {
                var dbfId = (int)Read();
                var count = (int)Read();
                AddCard(dbfId, count);
            }

            return deck;
        }
    }

    static class VarInt
	{
		public static byte[] GetBytes(ulong value)
		{
			using(var ms = new MemoryStream())
			{
				while(value != 0)
				{
					var b = value & 0x7f;
					value >>= 7;
					if(value != 0)
						b |= 0x80;
					ms.WriteByte((byte)b);
				}
				return ms.ToArray();
			}
		}

		public static ulong ReadNext(byte[] bytes, out int length)
		{
			length = 0;
			ulong result = 0;
			foreach(var b in bytes)
			{
				var value = (ulong)b & 0x7f;
				result |= value << length * 7;
				if((b & 0x80) != 0x80)
					break;
				length++;
			}
			length++;
			return result;
		}

	}
}
