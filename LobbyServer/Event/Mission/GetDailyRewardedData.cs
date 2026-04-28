using EpinelPS.Data;
using EpinelPS.Utils;
using EpinelPS.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Event.Mission
{
    /*[PacketPath("/event/dailymission/getrewardeddata")]
    public class GetDailyRewardedData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            Logging.Warn("==================================================");
            Logging.Warn("[DEBUG] /event/dailymission/getrewardeddata WAS CALLED!");

            var req = await ReadData<ReqGetDailyRewardedData>();
            User user = GetUser();
            ResGetDailyRewardedData response = new();

            try
            {
                int eventId = 20001; // Default fallback for Day-By-Day

                // 1. SAFELY FIND THE REQUESTED EVENT ID
                foreach (PropertyInfo prop in req.GetType().GetProperties())
                {
                    if (prop.PropertyType == typeof(int))
                    {
                        int val = (int)prop.GetValue(req)!;
                        if (val > 10000) eventId = val;
                    }
                }

                Logging.Warn($"[DEBUG] Game Client is asking for claims for Event ID: {eventId}");

                // 2. CHECK IF THE PLAYER HAS SAVED CLAIMS
                if (user.EventMissionInfo.TryGetValue(eventId, out EventMissionData? missionData))
                {
                    Logging.Warn($"[DEBUG] SUCCESS: Found saved data in db.json! Total claims: {missionData.MissionIdList.Count}");

                    // 3. SAFELY INJECT THE CLAIMS INTO THE RESPONSE ARRAY
                    foreach (PropertyInfo prop in response.GetType().GetProperties())
                    {
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>))
                        {
                            var list = prop.GetValue(response) as System.Collections.IList;
                            if (list != null)
                            {
                                var elementType = prop.PropertyType.GetGenericArguments()[0];

                                foreach (var id in missionData.MissionIdList)
                                {
                                    list.Add(Convert.ChangeType(id, elementType));
                                    Logging.Warn($"[DEBUG] Sending Claimed ID back to client: {id}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logging.Warn($"[DEBUG] FAILED: No saved data found in db.json for Event ID {eventId}!");
                }
            }
            catch (Exception ex)
            {
                Logging.Warn($"[DEBUG] ERROR in GetDailyRewardedData: {ex.Message}");
            }

            Logging.Warn("==================================================");
            await WriteDataAsync(response);
        }
    }*/
}