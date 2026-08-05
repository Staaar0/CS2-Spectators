using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;

namespace CS2Spectators;

public class SpectatorsConfig : IBasePluginConfig
{
	[JsonIgnore]
	public int Version { get; set; } = 1;

	[JsonPropertyName("HiddenAdminFlag")]
	public string HiddenAdminFlag { get; set; } = "";
}

public class CS2Spectators : BasePlugin, IPluginConfig<SpectatorsConfig>
{
	public override string ModuleName => "CS2-Spectators";
	public override string ModuleVersion => "1.0.0";
	public override string ModuleAuthor => "✪ Stαr";
	public override string ModuleDescription => "Show spectators count under the Minimap.";

	public SpectatorsConfig Config { get; set; } = new();

	private const string HudPrefix = "#Spectators";

	private readonly Dictionary<uint, int> counts = new();

	public void OnConfigParsed(SpectatorsConfig config)
	{
		Config = config;
	}

	public override void Load(bool hotReload)
	{
		AddTimer(0.25f, Refresh, TimerFlags.REPEAT);
	}

	public override void Unload(bool hotReload)
	{
		foreach (var player in Utilities.GetPlayers())
		{
			if (!player.IsValid)
				continue;

			var pawn = player.PlayerPawn.Value;
			if (pawn == null || !pawn.IsValid || !pawn.LastPlaceName.StartsWith(HudPrefix, StringComparison.Ordinal))
				continue;

			pawn.LastPlaceName = "";
			Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_szLastPlaceName");
		}
	}

	private void Refresh()
	{
		var players = Utilities.GetPlayers();

		counts.Clear();

		foreach (var player in players)
		{
			if (!player.IsValid || player.IsBot || player.IsHLTV || player.PawnIsAlive)
				continue;

			if (!string.IsNullOrWhiteSpace(Config.HiddenAdminFlag) && AdminManager.PlayerHasPermissions(player, Config.HiddenAdminFlag))
				continue;

			var services = player.Pawn.Value?.ObserverServices;
			if (services == null)
				continue;

			var mode = (ObserverMode_t)services.ObserverMode;
			if (mode != ObserverMode_t.OBS_MODE_IN_EYE && mode != ObserverMode_t.OBS_MODE_CHASE)
				continue;

			var target = services.ObserverTarget.Value;
			if (target == null || !target.IsValid)
				continue;

			counts.TryGetValue(target.Index, out var current);
			counts[target.Index] = current + 1;
		}

		foreach (var player in players)
		{
			if (!player.IsValid || !player.PawnIsAlive)
				continue;

			var pawn = player.PlayerPawn.Value;
			if (pawn == null || !pawn.IsValid)
				continue;

			counts.TryGetValue(pawn.Index, out var viewers);
			var text = viewers > 0 ? $"{HudPrefix}: {viewers}" : "";

			var current = pawn.LastPlaceName;
			if (current == text)
				continue;

			if (text == "" && !current.StartsWith(HudPrefix, StringComparison.Ordinal))
				continue;

			pawn.LastPlaceName = text;
			Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_szLastPlaceName");
		}
	}
}
