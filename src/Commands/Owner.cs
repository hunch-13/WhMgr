namespace WhMgr.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Utilities;

    [
        RequireOwner
    ]
    public class Owner : BaseCommandModule
    {
        const string PokemonTrainerClub = "https://sso.pokemon.com/sso/login";
        const string NianticLabs = "https://pgorelease.nianticlabs.com/plfe/version";

        private static readonly IEventLogger _logger = EventLogger.GetLogger("OWNER", Program.LogLevel);

        private readonly WhConfigHolder _whConfig;
        private readonly SubscriptionProcessor _subProcessor;

        public Owner(WhConfigHolder whConfig, SubscriptionProcessor subProcessor)
        {
            _whConfig = whConfig;
            _subProcessor = subProcessor;
        }

        [
            Command("isbanned"),
            Description("Check if IP banned from NianticLabs or Pokemon Trainer Club."),
            Hidden
        ]
        public async Task IsIPBannedAsync(CommandContext ctx)
        {
            var isPtcBanned = NetUtil.IsUrlBlocked(PokemonTrainerClub);
            var isNiaBanned = NetUtil.IsUrlBlocked(NianticLabs);
            var eb = new DiscordEmbedBuilder
            {
                Title = "Banned Status",
                Color = (isPtcBanned || isNiaBanned) ? DiscordColor.Red : DiscordColor.Green,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = ctx.Guild?.IconUrl,
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}"
                }
            };
            eb.AddField("Pokemon.com", isPtcBanned ? "Banned" : "Good", true);
            eb.AddField("NianticLabs.com", isNiaBanned ? "Banned" : "Good", true);
            await ctx.RespondAsync(embed: eb.Build());
        }

        [
            Command("clean-departed"),
            Description("Remove user subscriptions that are no longer donors from the database."),
            Hidden
        ]
        public async Task CleanDepartedAsync(CommandContext ctx)
        {
            _logger.Debug($"Checking if there are any subscriptions for members that are no longer apart of the server...");

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _whConfig.Instance.Servers.ContainsKey(x));

            var removed = 0;
            var users = _subProcessor?.Manager?.Subscriptions;
            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var discordUser = ctx.Client.GetMemberById(guildId, user.UserId);
                var isSupporter = ctx.Client.HasSupporterRole(guildId, user.UserId, _whConfig.Instance.Servers[guildId].DonorRoleIds);
                if (discordUser == null || !isSupporter)
                {
                    _logger.Debug($"Removing user {user.UserId} subscription settings because they are no longer a member of the server.");
                    if (!SubscriptionManager.RemoveAllUserSubscriptions(guildId, user.UserId))
                    {
                        _logger.Warn($"Unable to remove user {user.UserId} subscription settings from the database.");
                        continue;
                    }

                    _logger.Info($"Removed {user.UserId} and subscriptions from database.");
                    removed++;
                }
            }

            await ctx.RespondEmbed(Translator.Instance.Translate("REMOVED_TOTAL_DEPARTED_MEMBERS").FormatText(removed.ToString("N0"), users.Count.ToString("N0")));
        }

        [
            Command("sudo"),
            Description("Run a command as another user."),
            Hidden,
            RequireOwner
        ]
        public async Task SudoAsync(CommandContext ctx,
            [Description("")] DiscordUser user,
            [Description(""), RemainingText] string content)
        {
            var cmd = ctx.CommandsNext.FindCommand(content, out var args);
            var fctx = ctx.CommandsNext.CreateFakeContext(user, ctx.Channel, content, ctx.Prefix, cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
    }
}
/*
       headers={'Host': 'sso.pokemon.com',
                 'Connection': 'close',
                 'Accept': '/*',
                 'User-Agent': 'pokemongo/0 CFNetwork/893.14.2 Darwin/17.3.0',
                 'Accept-Language': 'en-us',
                 'Accept-Encoding': 'br, gzip, deflate',
                 'X-Unity-Version': '2017.1.2f1'},
        background_callback=__proxy_check_completed,
 */