using System.Collections.Generic;
using System.Linq;
using System.Text;
using Netsphere.Network;

namespace Netsphere.Commands
{
    // Season 2 has no clan create/kick/rank messages, so everything that is not
    // join, leave or notice has to be driven from here.
    internal class ClanCommands : ICommand
    {
        public string Name { get; }
        public bool AllowConsole { get; }
        public SecurityLevel Permission { get; }
        public IReadOnlyList<ICommand> SubCommands { get; }

        public ClanCommands()
        {
            Name = "clan";
            AllowConsole = true;
            Permission = SecurityLevel.GameMaster;
            SubCommands = new ICommand[]
            {
                new CreateCommand(), new DisbandCommand(), new ListCommand(),
                new AddCommand(), new KickCommand(), new RankCommand(), new NoticeCommand()
            };
        }

        public bool Execute(GameServer server, Player plr, string[] args)
        {
            return false;
        }

        public string Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Name);
            foreach (var command in SubCommands)
                sb.AppendLine(command.Help());
            return sb.ToString();
        }

        private static void Say(Player plr, string message)
        {
            if (plr == null)
                System.Console.WriteLine(message);
            else
                plr.SendConsoleMessage(message);
        }

        private class CreateCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public CreateCommand()
            {
                Name = "create";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 2)
                    return false;

                var name = args[0];
                var master = server.PlayerManager.Get(args[1]);
                if (master == null)
                {
                    Say(plr, $"Player {args[1]} is not online");
                    return true;
                }

                if (server.ClubManager.GetClubByName(name) != null)
                {
                    Say(plr, $"Clan {name} already exists");
                    return true;
                }

                if (master.Club != null)
                {
                    Say(plr, $"{master.Account.Nickname} is already in a clan");
                    return true;
                }

                var icon = args.Length > 2 ? args[2] : "1-1-1";
                var club = server.ClubManager.Create(name, icon, (int)master.Account.Id);
                Say(plr, $"Created clan {club.Name} (id {club.Id}) with master {master.Account.Nickname}");
                return true;
            }

            public string Help()
            {
                return $"{Name} <name> <masterNickname> [icon]";
            }
        }

        private class DisbandCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public DisbandCommand()
            {
                Name = "disband";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 1)
                    return false;

                var club = server.ClubManager.GetClubByName(args[0]);
                if (club == null)
                {
                    Say(plr, $"Clan {args[0]} does not exist");
                    return true;
                }

                server.ClubManager.Remove(club);
                Say(plr, $"Disbanded clan {args[0]}");
                return true;
            }

            public string Help()
            {
                return $"{Name} <name>";
            }
        }

        private class ListCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public ListCommand()
            {
                Name = "list";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (server.ClubManager.Count == 0)
                {
                    Say(plr, "No clans");
                    return true;
                }

                foreach (var club in server.ClubManager)
                    Say(plr, $"[{club.Id}] {club.Name} - {club.Count} members - level {club.Level}");

                return true;
            }

            public string Help()
            {
                return Name;
            }
        }

        private class AddCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public AddCommand()
            {
                Name = "add";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 2)
                    return false;

                var club = server.ClubManager.GetClubByName(args[0]);
                if (club == null)
                {
                    Say(plr, $"Clan {args[0]} does not exist");
                    return true;
                }

                var target = server.PlayerManager.Get(args[1]);
                if (target == null)
                {
                    Say(plr, $"Player {args[1]} is not online");
                    return true;
                }

                if (target.Club != null)
                {
                    Say(plr, $"{target.Account.Nickname} is already in a clan");
                    return true;
                }

                server.ClubManager.AddPlayer(club, (int)target.Account.Id, ClubRank.Member);
                Say(plr, $"Added {target.Account.Nickname} to {club.Name}");
                return true;
            }

            public string Help()
            {
                return $"{Name} <clanName> <nickname>";
            }
        }

        private class KickCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public KickCommand()
            {
                Name = "kick";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 1)
                    return false;

                var target = server.PlayerManager.Get(args[0]);
                if (target == null)
                {
                    Say(plr, $"Player {args[0]} is not online");
                    return true;
                }

                var club = target.Club;
                if (club == null)
                {
                    Say(plr, $"{target.Account.Nickname} is not in a clan");
                    return true;
                }

                server.ClubManager.RemovePlayer(club, (int)target.Account.Id);
                Say(plr, $"Removed {target.Account.Nickname} from {club.Name}");
                return true;
            }

            public string Help()
            {
                return $"{Name} <nickname>";
            }
        }

        private class RankCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public RankCommand()
            {
                Name = "rank";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 2)
                    return false;

                var target = server.PlayerManager.Get(args[0]);
                if (target == null)
                {
                    Say(plr, $"Player {args[0]} is not online");
                    return true;
                }

                var club = target.Club;
                if (club == null)
                {
                    Say(plr, $"{target.Account.Nickname} is not in a clan");
                    return true;
                }

                ClubRank rank;
                if (!System.Enum.TryParse(args[1], true, out rank))
                {
                    Say(plr, "Rank must be Master, Staff or Member");
                    return true;
                }

                var member = club[(int)target.Account.Id];
                member.Rank = rank;

                server.ClubManager.RemovePlayer(club, member.PlayerId);
                server.ClubManager.AddPlayer(club, member.PlayerId, rank);

                if (rank == ClubRank.Master)
                {
                    club.MasterId = member.PlayerId;
                    server.ClubManager.Save(club);
                }

                Say(plr, $"{target.Account.Nickname} is now {rank} of {club.Name}");
                return true;
            }

            public string Help()
            {
                return $"{Name} <nickname> <Master|Staff|Member>";
            }
        }

        private class NoticeCommand : ICommand
        {
            public string Name { get; }
            public bool AllowConsole { get; }
            public SecurityLevel Permission { get; }
            public IReadOnlyList<ICommand> SubCommands { get; }

            public NoticeCommand()
            {
                Name = "notice";
                AllowConsole = true;
                Permission = SecurityLevel.GameMaster;
                SubCommands = new ICommand[0];
            }

            public bool Execute(GameServer server, Player plr, string[] args)
            {
                if (args.Length < 2)
                    return false;

                var club = server.ClubManager.GetClubByName(args[0]);
                if (club == null)
                {
                    Say(plr, $"Clan {args[0]} does not exist");
                    return true;
                }

                club.Notice = string.Join(" ", args.Skip(1));
                server.ClubManager.Save(club);
                Say(plr, $"Notice of {club.Name} set to: {club.Notice}");
                return true;
            }

            public string Help()
            {
                return $"{Name} <clanName> <text>";
            }
        }
    }
}
