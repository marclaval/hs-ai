using System.Threading.Tasks;
using SabberStoneCore.Model;

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces
{
	public interface IHsLogReader
	{
		/// <summary>
		/// Start tracking gamelogs with default impelementaion of GameEventHandler
		/// </summary>
		void Start(Game game);

		/// <summary>
		/// Start tracking gamelogs with custom impelementaion of GameEventHandler
		/// </summary>
		/// <param name="gh"> Custom Game handler implementation </param>
		void Start(IGameHandler gh, Game game);

		void Stop();
		void ClearLog();
		Task<bool> RankedDetection(int timeoutInSeconds = 3);
		//void GetCurrentRegion();
		void Reset(bool full);
	}
}
