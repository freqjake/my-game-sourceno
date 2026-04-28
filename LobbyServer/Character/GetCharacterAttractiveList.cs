using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/get")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetAttractiveList req = await ReadData<ReqGetAttractiveList>();
            User user = GetUser();

            // 1. Count how many characters are locked in your database
            int usedTickets = user.BondInfo.Count(x => x.CanCounselToday == false);

            ResGetAttractiveList response = new()
            {
                // 2. Subtract the locked characters to get your remaining tickets!
                CounselAvailableCount = Math.Max(0, 3 - usedTickets)
            };

            // 3. Clear any default data just to be safe
            response.Attractives.Clear();

            // 4. Send the perfect, natively saved database directly to the game.
            // NO foreach loops, and NO forcing CanCounselToday to true!
            response.Attractives.AddRange(user.BondInfo);

            await WriteDataAsync(response);
        }
    }
}