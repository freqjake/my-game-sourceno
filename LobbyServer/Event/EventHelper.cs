using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Event
{
    public class EventHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventHelper));

        public static void AddEvents(User user, ref ResGetEventList response)
        {
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = GetLobbyPrivateBannerData(user);
            if (lobbyPrivateBanners.Count == 0) return;

            var eventManagers = GameData.Instance.eventManagers.Values.ToList();
            foreach (var banner in lobbyPrivateBanners)
            {
                List<NetEventData> events = GetEventData(banner, eventManagers);
                AddEvents(user, ref response, events);

                List<EventSystemType> systemTypes = [EventSystemType.PickupGachaEvent, EventSystemType.BoxGachaEvent, EventSystemType.LoginEvent];
                List<NetEventData> gachaEvents = GetEventDataBySystemTypes(banner, eventManagers, systemTypes);
                AddEvents(user, ref response, gachaEvents);

                var challengeEvents = GetChallengeEventData(banner, eventManagers);
                AddEvents(user, ref response, challengeEvents);
            }
            List<NetEventData> dailyMissionEvents = GetDailyMissionEventData(eventManagers);
            AddEvents(user, ref response, dailyMissionEvents);
        }

        public static void AddJoinedEvents(User user, ref ResGetJoinedEvent response)
        {
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = GetLobbyPrivateBannerData(user);
            if (lobbyPrivateBanners.Count == 0) return;

            var eventManagers = GameData.Instance.eventManagers.Values.ToList();
            foreach (var banner in lobbyPrivateBanners)
            {
                var events = GetEventData(banner, eventManagers);
                AddJoinedEvents(user, ref response, events); // Fixed: Properly passes User

                List<EventSystemType> systemTypes = [EventSystemType.PickupGachaEvent, EventSystemType.BoxGachaEvent, EventSystemType.LoginEvent];
                List<NetEventData> gachaEvents = GetEventDataBySystemTypes(banner, eventManagers, systemTypes);
                AddJoinedEvents(user, ref response, gachaEvents); // Fixed: Properly passes User

                List<NetEventData> challengeEvents = GetChallengeEventData(banner, eventManagers);
                AddJoinedEvents(user, ref response, challengeEvents); // Fixed: Properly passes User
            }
            List<NetEventData> dailyMissionEvents = GetDailyMissionEventData(eventManagers);
            AddJoinedEvents(user, ref response, dailyMissionEvents); // Fixed: Properly passes User
        }

        private static List<NetEventData> GetEventData(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers)
        {
            List<NetEventData> events = [];
            if (!eventManagers.Any(em => em.Id == banner.EventId)) return events;
            var mainEvent = eventManagers.First(em => em.Id == banner.EventId);
            events.Add(new NetEventData() { Id = mainEvent.Id, EventSystemType = (int)mainEvent.EventSystemType });
            var childEvents = eventManagers.Where(em => em.ParentsEventId == banner.EventId || em.SetField == banner.EventId).ToList();
            foreach (var childEvent in childEvents)
            {
                events.Add(new NetEventData() { Id = childEvent.Id, EventSystemType = (int)childEvent.EventSystemType });
            }
            return events;
        }

        private static List<NetEventData> GetEventDataBySystemTypes(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers, List<EventSystemType> systemTypes)
        {
            List<NetEventData> events = [];
            List<string> eventBannerResourceTables = [.. eventManagers.Where(em =>
                (em.SetField == banner.EventId || em.ParentsEventId == banner.EventId)
                && em.EventBannerResourceTable.StartsWith("event_")).Select(em => em.EventBannerResourceTable)];
            eventBannerResourceTables = [.. eventBannerResourceTables.Distinct()];
            if (eventBannerResourceTables.Count == 0) return events;

            var gachaEvents = eventManagers.Where(em => eventBannerResourceTables.Contains(em.EventBannerResourceTable) && systemTypes.Contains(em.EventSystemType)).ToList();
            foreach (var gachaEvent in gachaEvents)
            {
                events.Add(new NetEventData() { Id = gachaEvent.Id, EventSystemType = (int)gachaEvent.EventSystemType });
            }
            return events;
        }

        private static List<NetEventData> GetChallengeEventData(LobbyPrivateBannerRecord banner, List<EventManagerRecord> eventManagers)
        {
            List<NetEventData> events = [];
            var challengeEvents = eventManagers.Where(em => em.ParentsEventId == banner.EventId && em.EventSystemType == EventSystemType.ChallengeModeEvent).ToList();
            foreach (var challengeEvent in challengeEvents)
            {
                events.Add(new NetEventData() { Id = challengeEvent.Id, EventSystemType = (int)challengeEvent.EventSystemType });
            }
            return events;
        }

        public static List<LobbyPrivateBannerRecord> GetLobbyPrivateBannerData(User user)
        {
            var lobbyPrivateBannerIds = user.LobbyPrivateBannerIds;
            var lobbyPrivateBannerRecords = GameData.Instance.LobbyPrivateBannerTable.Values;
            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = [];
            if (lobbyPrivateBannerIds is not null && lobbyPrivateBannerIds.Count > 0)
            {
                lobbyPrivateBanners = [.. lobbyPrivateBannerRecords.Where(b => lobbyPrivateBannerIds.Contains(b.Id))];
            }
            else
            {
                lobbyPrivateBanners.Add(lobbyPrivateBannerRecords.OrderBy(b => b.EventId).Last());
            }
            return lobbyPrivateBanners;
        }

        // =========================================================================================
        // MAGIC RELOGIN FIX: Auto-inject saved database claims into NetEventData arrays!
        // =========================================================================================
        private static void InjectSavedClaims(User user, NetEventData eventData)
        {
            if (user.EventMissionInfo.TryGetValue(eventData.Id, out var missionData) && missionData.MissionIdList.Count > 0)
            {
                foreach (PropertyInfo prop in eventData.GetType().GetProperties())
                {
                    // Look for ANY repeated array in the eventData (e.g. DailyEventIds)
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>))
                    {
                        var list = prop.GetValue(eventData) as System.Collections.IList;
                        if (list != null)
                        {
                            var elementType = prop.PropertyType.GetGenericArguments()[0];
                            foreach (var id in missionData.MissionIdList)
                            {
                                try
                                {
                                    var converted = Convert.ChangeType(id, elementType);
                                    if (!list.Contains(converted)) list.Add(converted);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        private static void AddEvents(User user, ref ResGetEventList response, List<NetEventData> eventDatas)
        {
            foreach (var eventData in eventDatas)
            {
                // ==================================================
                // HIDE SPECIFIC EVENTS SO THEY DON'T SHOW UP IN-GAME
                // ==================================================
                if (eventData.Id == 160034 || eventData.Id == 160035 || eventData.Id == 10046 || eventData.Id == 20001 || eventData.Id == 20002)
                {
                    continue; // Skip completely!
                }
                // ==================================================
                if (!response.EventList.Any(e => e.Id == eventData.Id))
                {
                    if (eventData.Id == 10046 || eventData.Id == 20001 || eventData.Id == 20002)
                    {
                        // SAFETY NET FIX: Check if RegisterTime is 0 or a Unix Timestamp
                        long safeStartTime = user.RegisterTime;
                        if (safeStartTime < 600000000000000000) // 600000000000000000 is approx Year 1900 in Ticks
                        {
                            // If invalid, fallback to 14 days ago so the event is fully unlocked today!
                            safeStartTime = DateTime.UtcNow.AddDays(-14).Ticks;
                        }

                        eventData.EventStartDate = safeStartTime;
                        eventData.EventVisibleDate = safeStartTime;
                        eventData.EventDisableDate = new DateTime(safeStartTime).AddDays(40).Ticks;
                        eventData.EventEndDate = new DateTime(safeStartTime).AddDays(40).Ticks;
                    }
                    else
                    {
                        // Standard Events
                        if (eventData.EventStartDate == 0) eventData.EventStartDate = DateTime.UtcNow.AddDays(-1).Ticks;
                        if (eventData.EventVisibleDate == 0) eventData.EventVisibleDate = DateTime.UtcNow.AddDays(-1).Ticks;
                        if (eventData.EventDisableDate == 0) eventData.EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks;
                        if (eventData.EventEndDate == 0) eventData.EventEndDate = DateTime.UtcNow.AddDays(30).Ticks;
                    }

                    // Actually call the Magic Fix here!
                    InjectSavedClaims(user, eventData);

                    response.EventList.Add(eventData);
                }
            }
        }

        // FIX: Added 'User user' to the method signature so it matches the caller
        private static void AddJoinedEvents(User user, ref ResGetJoinedEvent response, List<NetEventData> eventDatas)
        {
            foreach (var eventData in eventDatas)
            {
                if (eventData.Id == 70115) continue;

                // Avoid adding duplicate events
                if (!response.EventWithJoinData.Any(e => e.EventData.Id == eventData.Id))
                {
                    if (eventData.EventStartDate == 0) eventData.EventStartDate = DateTime.UtcNow.AddDays(-1).Ticks;
                    if (eventData.EventVisibleDate == 0) eventData.EventVisibleDate = DateTime.UtcNow.AddDays(-1).Ticks;
                    if (eventData.EventDisableDate == 0) eventData.EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks;
                    if (eventData.EventEndDate == 0) eventData.EventEndDate = DateTime.UtcNow.AddDays(30).Ticks;

                    // Actually call the Magic Fix here!
                    InjectSavedClaims(user, eventData);

                    response.EventWithJoinData.Add(new NetEventWithJoinData()
                    {
                        EventData = eventData,
                        JoinAt = eventData.EventStartDate
                    });
                }
                else
                {
                    log.Debug($"Skipping duplicate event Id: {eventData.Id}");
                }
            }
        }

        private static List<NetEventData> GetDailyMissionEventData(List<EventManagerRecord> eventManagers)
        {
            List<NetEventData> events = [];
            var dailyEventIds = GameData.Instance.DailyMissionEventSettingTable.Values.Select(de => de.EventId).ToList();
            var dailyEvents = eventManagers.Where(em => dailyEventIds.Contains(em.Id)).ToList();
            if (dailyEvents.Count == 0) return events;

            foreach (var dailyEvent in dailyEvents)
            {
                events.Add(new NetEventData()
                {
                    Id = dailyEvent.Id,
                    EventSystemType = (int)dailyEvent.EventSystemType,
                    EventStartDate = DateTime.UtcNow.Ticks,
                    EventVisibleDate = DateTime.UtcNow.Ticks,
                    EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks,
                    EventEndDate = DateTime.UtcNow.AddDays(30).Ticks
                });
            }
            return events;
        }
    }
}