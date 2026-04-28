using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;
using System.Linq;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    public static class EventStoryHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventStoryHelper));

        // Redirects Hard Mode (40103) to Normal Mode (40102) so they share tickets
        private static int GetRealEventId(int eventId)
        {
            var em = GameData.Instance.eventManagers.Values.FirstOrDefault(e => e.Id == eventId);
            if (em != null && em.ParentsEventId != 0)
            {
                return em.ParentsEventId;
            }
            return eventId;
        }

        public static int GetTicket(User user, int eventId)
        {
            eventId = GetRealEventId(eventId);

            // Safely create the shared event bucket if it doesn't exist
            if (!user.EventInfo.TryGetValue(eventId, out var eventData))
            {
                eventData = new() { LastDay = 0, FreeTicket = 0 };
                user.EventInfo[eventId] = eventData;
            }

            int freeTicket = eventData.FreeTicket;

            (DbItemData itemTicket, int freeTicketMax) = GetItemTicket(user, eventId);

            int remainItemTicket = itemTicket?.Count ?? 0;

            if (freeTicket > freeTicketMax) freeTicket = freeTicketMax;

            int dateDay = user.GetDateDay();
            if (dateDay > eventData.LastDay)
            {
                log.Debug($"GetTicket ResetFreeTicket DateDay: {dateDay}, LastDay: {eventData.LastDay}, FreeTicketMax: {freeTicketMax}");

                freeTicket = freeTicketMax;
                user.EventInfo[eventId].FreeTicket = freeTicket;
                user.EventInfo[eventId].LastDay = dateDay;
            }

            int remainTicket = freeTicket + remainItemTicket;

            log.Debug($"GetTicket EventId: {eventId}, FreeTicket: {freeTicket}, ItemTicket: {remainItemTicket}, RemainTicket: {remainTicket}");
            return remainTicket;
        }

        public static int SubtractTicket(User user, int eventId, int val)
        {
            eventId = GetRealEventId(eventId);

            if (!user.EventInfo.TryGetValue(eventId, out var eventData))
            {
                eventData = new() { LastDay = 0, FreeTicket = 0 };
                user.EventInfo[eventId] = eventData;
            }

            int freeTicket = eventData.FreeTicket;

            (DbItemData itemTicket, _) = GetItemTicket(user, eventId);
            int remainItemTicket = itemTicket?.Count ?? 0;

            if (freeTicket >= val)
            {
                freeTicket -= val;
                user.EventInfo[eventId].FreeTicket = freeTicket;

                int remainTicket = freeTicket + remainItemTicket;
                log.Debug($"SubtractTicket Value: {val}, FreeTicket: {freeTicket}, ItemTicket: {remainItemTicket}, RemainTicket: {remainTicket}");
                return remainTicket;
            }
            else
            {
                int SubtractItemTicket = val - freeTicket;
                user.EventInfo[eventId].FreeTicket = 0;

                if (itemTicket is not null)
                {
                    user.RemoveItemBySerialNumber(itemTicket.Isn, SubtractItemTicket);
                }

                freeTicket = 0;
                int remainTicket = freeTicket + remainItemTicket;
                log.Debug($"SubtractTicket Value: {val}, FreeTicket: {freeTicket}, ItemTicket: {remainItemTicket}, RemainTicket: {remainTicket}");
                return remainTicket;
            }
        }

        private static (DbItemData itemTicket, int freeTicketMax) GetItemTicket(User user, int eventId)
        {
            // RESTORED TO 5: This fixes the missing tickets!
            int freeTicketMax = 5;

            var eventStory = GameData.Instance.EventStoryTable.Values.FirstOrDefault(x => x.EventId == eventId);

            if (eventStory is null || eventStory.AutoChargeId == 0) return (null, freeTicketMax);
            log.Debug($"GetItemTicket EventId: {eventId}, EventStory: {JsonConvert.SerializeObject(eventStory)}");

            if (!GameData.Instance.AutoChargeTable.TryGetValue(eventStory.AutoChargeId, out var autoCharge)) return (null, freeTicketMax);
            log.Debug($"GetItemTicket AutoChargeId: {eventStory.AutoChargeId}, AutoCharge: {JsonConvert.SerializeObject(autoCharge)}");

            if (autoCharge.AutoChargeMax == 0) return (null, freeTicketMax);

            freeTicketMax = autoCharge.AutoChargeMax;

            var userItem = user.Items.FirstOrDefault(x => x.ItemType == autoCharge.ItemId);
            log.Debug($"GetItemTicket UserItem: {(userItem is not null ? JsonConvert.SerializeObject(userItem) : null)}");
            return (userItem, freeTicketMax);
        }
    }
}