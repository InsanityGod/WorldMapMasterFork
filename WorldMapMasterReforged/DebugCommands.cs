using HarmonyLib;
using InsanityLib.Auto.Command;
using InsanityLib.Auto.Command.Argument;
using System;
using System.Drawing;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

#if DEBUG
namespace WorldMapMasterReforged;

internal static class DebugCommands
{
    private static int generatedCount = 0;

    [AutoCommand(Name = "waypoints", Path = "debug/generate")]
    public static TextCommandResult GenerateTestWaypoints([CommandParameter(ContextualSource = EContextualSource.Caller)] IPlayer player, ICoreServerAPI api, int amount, int range = 100)
    {
        var manager = api.ModLoader.GetModSystem<WorldMapManager>();
        var mapLayer = manager.MapLayers.OfType<WaypointMapLayer>().FirstOrDefault();

        if(mapLayer is null || player is not IServerPlayer serverPlayer)
        {
            return TextCommandResult.Error("This command can only be used by a player on the server, and requires the WaypointMapLayer to be loaded.");
        }

        var playerPos = serverPlayer.Entity.Pos;
        
        for (int i = 0; i < amount; i++)
        {
            int x = api.World.Rand.Next(-range, range);
            int z = api.World.Rand.Next(-range, range);

            var waypoint = new Waypoint()
            {
                Guid = Guid.NewGuid().ToString(),
                OwningPlayerUid = player.PlayerUID,
                Title = $"Test-{++generatedCount}",
                Icon = "circle",
                Position = new Vec3d(playerPos.X + x, playerPos.Y, playerPos.Z + z),
                Color = Color.FromArgb(api.World.Rand.Next(256), api.World.Rand.Next(256), api.World.Rand.Next(256)).ToArgb(),
            };

            mapLayer.AddWaypoint(waypoint, serverPlayer);
        }

        return TextCommandResult.Success($"Succesfully created {amount} waypoints");
    }

    [AutoCommand(Name = "waypoints", Path = "debug/clean")]
    public static TextCommandResult CleanWaypoints([CommandParameter(ContextualSource = EContextualSource.Caller)] IPlayer player, ICoreServerAPI api)
    {
        var manager = api.ModLoader.GetModSystem<WorldMapManager>();
        var mapLayer = manager.MapLayers.OfType<WaypointMapLayer>().FirstOrDefault();

        if(mapLayer is null || player is not IServerPlayer serverPlayer)
        {
            return TextCommandResult.Error("This command can only be used by a player on the server, and requires the WaypointMapLayer to be loaded.");
        }

        mapLayer.Waypoints.Clear();
        AccessTools.Method(typeof(WaypointMapLayer), "ResendWaypoints").Invoke(mapLayer, [serverPlayer]);
        generatedCount = 0;

        return TextCommandResult.Success("Succesfully cleared waypoints");
    }
}
#endif