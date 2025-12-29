using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Translations;
using CS2_HitMark.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CS2_HitMark;

public class Helper
{
    public static void AdvancedPlayerPrintToChat(CCSPlayerController player, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString());
        }
        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    player.PrintToChat(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            player.PrintToChat(message);
        }
    }

    public static void AdvancedPlayerPrintToConsole(CCSPlayerController player, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString());
        }
        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    player.PrintToConsole(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            player.PrintToConsole(message);
        }
    }

    public static void AdvancedServerPrintToChatAll(string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString());
        }
        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    Server.PrintToChatAll(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            Server.PrintToChatAll(message);
        }
    }

    public static List<CCSPlayerController> GetPlayersController(bool IncludeBots = false, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        var playerList = Utilities
            .FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller")
            .Where(p => p != null && p.IsValid &&
                        (IncludeBots || (!p.IsBot && !p.IsHLTV)) &&
                        p.Connected == PlayerConnectedState.PlayerConnected &&
                        ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) ||
                        (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) ||
                        (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator)))
            .ToList();

        return playerList;
    }

    public static int GetPlayersCount(bool IncludeBots = false, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        return Utilities.GetPlayers().Count(p =>
            p != null &&
            p.IsValid &&
            p.Connected == PlayerConnectedState.PlayerConnected &&
            (IncludeBots || (!p.IsBot && !p.IsHLTV)) &&
            ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) ||
            (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) ||
            (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator))
        );
    }

    public static void ClearVariables()
    {
        var g_Main = HitMarkPlugin.Instance.g_Main;
        g_Main.Player_Data.Clear();
    }

    public static void DebugMessage(string message, bool prefix = true)
    {
        if (!HitMarkPlugin.Instance.Config.Debug) return;

        Console.ForegroundColor = ConsoleColor.Magenta;
        string Prefix = $"[HitMark]: ";
        Console.WriteLine(prefix ? Prefix : "" + message);

        Console.ResetColor();
    }

    public static CCSGameRules? GetGameRules()
    {
        try
        {
            var gameRulesEntities = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
            return gameRulesEntities.First().GameRules;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsWarmup()
    {
        return GetGameRules()?.WarmupPeriod ?? false;
    }

    public static void InitializePlayerHUD(CCSPlayerController player)
    {
        var g_Main = HitMarkPlugin.Instance.g_Main;
        var config = HitMarkPlugin.Instance.Config;

        if (player == null || !player.IsValid) return;

        string? headSound = ResolveSoundForPlayer(config.HeadshotSounds, player);
        string? bodySound = ResolveSoundForPlayer(config.BodyshotSounds, player);

        try
        {
            g_Main.Player_Data[player] = new Globals.PlayerDataClass(
                player,
                headSound ?? string.Empty,
                bodySound ?? string.Empty
            );
        }
        catch (Exception ex)
        {
            DebugMessage($"Failed to initialize player data for {player.PlayerName}: {ex.Message}");
        }
    }

    public static void StartHitMark(CCSPlayerController player, bool headShot, int damage)
    {
        var g_Main = HitMarkPlugin.Instance.g_Main;
        var config = HitMarkPlugin.Instance.Config;

        if (player == null || !player.IsValid) return;

        if (!g_Main.Player_Data.ContainsKey(player))
        {
            InitializePlayerHUD(player);
        }

        if (!g_Main.Player_Data.TryGetValue(player, out var playerData))
        {
            return;
        }

        try
        {
            if (playerData.HitMarkEnabled && config.HitMarkEnabled)
            {
                TrySpawnHitParticle(player, headShot, config);
            }
            if (playerData.HitMarkEnabled && config.DamageDigitsEnabled)
            {
                TrySpawnDamageParticles(player, damage, headShot, config);
            }

            if (playerData.SoundEnabled && headShot && !string.IsNullOrEmpty(playerData.Sound_HeadShot))
            {
                player.ExecuteClientCommand("play " + playerData.Sound_HeadShot);
            }
            else if (playerData.SoundEnabled && !headShot && !string.IsNullOrEmpty(playerData.Sound_BodyShot))
            {
                player.ExecuteClientCommand("play " + playerData.Sound_BodyShot);
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"Failed to show hitmark: {ex.Message}");
        }
    }

    private static void TrySpawnHitParticle(CCSPlayerController player, bool headShot, Config config)
    {
        string path = headShot ? config.HitMarkHeadshotParticle : config.HitMarkBodyshotParticle;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        float lifetime = headShot ? config.HitMarkHeadshotDuration : config.HitMarkBodyshotDuration;
        SpawnCrosshairParticle(player, path, config.HitMarkDistance, lifetime, config.HitMarkInput);
    }

    private static void TrySpawnDamageParticles(CCSPlayerController player, int damage, bool headShot, Config config)
    {
        var digits = config.DamageDigitParticles;
        if (digits == null || digits.Count < 10)
        {
            DebugMessage("Damage digits list missing or incomplete (need 10 entries).");
            return;
        }

        string damageText = Math.Max(0, damage).ToString(CultureInfo.InvariantCulture);
        if (damageText.Length == 0)
        {
            DebugMessage("Damage text empty, skipping digits.");
            return;
        }

        float lifetime = headShot ? config.DamageHeadshotDuration : config.DamageBodyshotDuration;
        DebugMessage($"Spawning damage digits '{damageText}' (headshot={headShot}).");
        SpawnDamageDigitParticles(player, damageText, digits, config, lifetime);
    }

    public static bool SpawnCrosshairParticle(CCSPlayerController player, string effectName, float distance, float lifetime, string? acceptInput)
    {
        return SpawnParticleAtCrosshair(player, effectName, distance, lifetime, acceptInput, null);
    }

    private static void SpawnDamageDigitParticles(CCSPlayerController player, string damageText, List<string> digits, Config config, float lifetime)
    {
        int count = damageText.Length;
        if (count <= 0)
        {
            return;
        }

        float spacing = MathF.Max(0f, config.DamageSpacing);
        float baseOffset = -((count - 1) * spacing) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            int digitIndex = damageText[i] - '0';
            if (digitIndex < 0 || digitIndex > 9)
            {
                DebugMessage($"Invalid digit '{damageText[i]}' at index {i}.");
                continue;
            }

            string path = digits[digitIndex];
            if (string.IsNullOrWhiteSpace(path))
            {
                DebugMessage($"Digit particle path missing for {digitIndex}.");
                continue;
            }

            float offsetX = baseOffset + (i * spacing) + config.DamageOffsetX;
            float offsetY = config.DamageOffsetY;
            SpawnParticleAtCrosshair(player, path, config.DamageDistance, lifetime, config.DamageInput, new Vector(offsetX, offsetY, 0f));
        }
    }

    private static bool SpawnParticleAtCrosshair(CCSPlayerController player, string effectName, float distance, float lifetime, string? acceptInput, Vector? offset)
    {
        if (player == null || !player.IsValid)
        {
            return false;
        }

        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(effectName))
        {
            return false;
        }

        var plugin = HitMarkPlugin.Instance;
        if (plugin == null)
        {
            return false;
        }

        if (!plugin.CanSpawnParticle(player.Slot, HitMarkPlugin.Instance.Config.MaxActiveParticlesPerPlayer))
        {
            DebugMessage($"Particle cap reached for slot {player.Slot}.", false);
            return false;
        }

        Server.NextFrame(() =>
        {
            if (player == null || !player.IsValid)
            {
                return;
            }

            var framePawn = player.PlayerPawn.Value;
            if (framePawn == null || !framePawn.IsValid)
            {
                return;
            }

            var spawnPos = GetCrosshairPosition(framePawn, distance);
            if (spawnPos == null)
            {
                return;
            }

            if (offset != null)
            {
                var right = RightFromYaw(framePawn.EyeAngles);
                spawnPos = new Vector(
                    spawnPos.X + (right.X * offset.X),
                    spawnPos.Y + (right.Y * offset.X),
                    spawnPos.Z + offset.Y
                );
            }

            var particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system");
            if (particle == null || !particle.IsValid)
            {
                return;
            }

            particle.EffectName = effectName;
            particle.DispatchSpawn();
            particle.Teleport(spawnPos, framePawn.EyeAngles, new Vector());

            if (!string.IsNullOrWhiteSpace(acceptInput) &&
                !acceptInput.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                particle.AcceptInput(acceptInput);
            }

            float safeLifetime = MathF.Max(0.05f, lifetime);
            plugin.AddTimer(safeLifetime, () =>
            {
                if (particle.IsValid)
                {
                    particle.Remove();
                }
                plugin.ReleaseParticleOwner(particle.Index);
            });

            plugin.RegisterParticleOwner(particle.Index, player.Slot);
        });

        return true;
    }

    private static Vector? GetCrosshairPosition(CCSPlayerPawn pawn, float distance)
    {
        var origin = pawn.AbsOrigin;
        if (origin == null)
        {
            return null;
        }

        var viewOffset = pawn.ViewOffset;
        var eyePos = new Vector(
            origin.X + viewOffset.X,
            origin.Y + viewOffset.Y,
            origin.Z + viewOffset.Z
        );

        var forward = ForwardFromAngles(pawn.EyeAngles);
        float spawnDistance = MathF.Max(1f, distance);
        return new Vector(
            eyePos.X + forward.X * spawnDistance,
            eyePos.Y + forward.Y * spawnDistance,
            eyePos.Z + forward.Z * spawnDistance
        );
    }

    private static Vector ForwardFromAngles(QAngle angles)
    {
        float pitchRad = angles.X * (MathF.PI / 180f);
        float yawRad = angles.Y * (MathF.PI / 180f);

        float cp = MathF.Cos(pitchRad);
        float sp = MathF.Sin(pitchRad);
        float cy = MathF.Cos(yawRad);
        float sy = MathF.Sin(yawRad);

        return new Vector(cp * cy, cp * sy, -sp);
    }

    private static Vector RightFromYaw(QAngle angles)
    {
        float yawRad = angles.Y * (MathF.PI / 180f);
        return new Vector(-MathF.Sin(yawRad), MathF.Cos(yawRad), 0f);
    }

    private static string? ResolveSoundForPlayer(List<string> entries, CCSPlayerController player)
    {
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry)) continue;

            return entry.Trim();
        }

        return null;
    }
}
