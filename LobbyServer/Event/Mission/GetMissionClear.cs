using System;
using System.Collections.Generic;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/mission/getclear")]
    public class GetMissionClear : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClear>();
            User user = GetUser();

            ResGetEventMissionClear response = new();

            try
            {
                // THE FULL FIX: Bypass the broken helper and read straight from db.json!
                // 1. Check if the database has any memory of this event
                if (user.ClearedEventMissions != null && user.ClearedEventMissions.ContainsKey(req.EventId))
                {
                    // 2. If it does, grab the list of cleared missions and send it to the game!
                    List<int> savedMissions = user.ClearedEventMissions[req.EventId];
                    // Loop through every number in your saved list
                    // Loop through every number in your saved list
                    foreach (int missionId in savedMissions)
                    {
                        // Wrap the number in the object the list is expecting!
                        response.EventMissionClearList.Add(new NetEventMissionClearData()
                        {
                            EventMissionId = missionId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warn($"GetMissionClear failed: {ex.Message}");
            }

            // The // TODO is gone! Sending the response is the only thing this packet needs to do.
            await WriteDataAsync(response);
        }
    }
}