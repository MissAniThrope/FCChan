﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using NodaTime;

	public class HelpService : ServiceBase
	{
		public static string GetTypeName(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: return "boolean";
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal: return "number";
				case TypeCode.DateTime: return "date and time";
				case TypeCode.String: return "string";
			}

			if (type == typeof(SocketTextChannel))
				return "#channel";

			if (type == typeof(IEmote))
				return ":emote:";

			if (type == typeof(IUser))
				return "@user";

			if (type == typeof(IGuildUser))
				return "@user";

			if (type == typeof(Duration))
				return "duration (1d:1h:1m:1s)";

			return type.Name;
		}

		public static string GetParam(ParameterInfo param)
		{
			string? name = param.Name;
			Type type = param.ParameterType;

			if (name == null)
				return "unknown";

			name = Regex.Replace(name, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
			name = "[" + name + "]";

			if (type == typeof(string))
				name = "\"" + name + "\"";

			if (type == typeof(IEmote))
				name = ":" + name + ":";

			if (type == typeof(IUser) || type == typeof(IGuildUser))
				name = "@" + name;

			return name;
		}

		public static Task<Embed> GetHelp(CommandMessage message, string? command = null)
		{
			StringBuilder builder = new StringBuilder();
			Permissions permissions = CommandsService.GetPermissions(message.Author);

			if (command == null)
				command = message.Command;

			builder.AppendLine(GetHelp(message.Guild, command, permissions));

			EmbedBuilder embed = new EmbedBuilder();
			embed.Description = builder.ToString();

			if (string.IsNullOrEmpty(embed.Description))
				throw new UserException("I'm sorry, you don't have permission to use that command.");

			return Task.FromResult(embed.Build());
		}

		public static async Task<bool> GetHelp(CommandMessage message, Permissions permissions)
		{
			EmbedBuilder embed = new EmbedBuilder();
			StringBuilder builder = new StringBuilder();

			List<string> commandStrings = new List<string>(CommandsService.GetCommands());
			commandStrings.Sort();

			int commandCount = 0;
			foreach (string commandString in commandStrings)
			{
				if (commandString == "help")
					continue;

				int count = 0;
				List<Command> commands = CommandsService.GetCommands(commandString);
				foreach (Command command in commands)
				{
					if (command.Permission > permissions)
						continue;

					count++;
				}

				if (count <= 0)
					continue;

				builder.Append("__");
				builder.Append(commandString);
				builder.Append("__ - *+");
				builder.Append(count);
				builder.Append("* - ");
				builder.Append(commands[0].Help);
				builder.AppendLine();

				commandCount++;

				if (commandCount >= 20)
				{
					embed = new EmbedBuilder();
					embed.Description = builder.ToString();
					await message.Channel.SendMessageAsync(null, false, embed.Build());
					builder.Clear();
					commandCount = 0;
				}
			}

			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("To get more information on a command, look it up directly, like `" + CommandsService.GetPrefix(message.Guild) + "help \"time\"` or `" + CommandsService.GetPrefix(message.Guild) + "et ?`");

			embed = new EmbedBuilder();
			embed.Description = builder.ToString();
			await message.Channel.SendMessageAsync(null, false, embed.Build());
			return true;
		}

		[Command("Help", Permissions.Everyone, "really?")]
		public async Task<bool> Help(CommandMessage message)
		{
			Permissions permissions = CommandsService.GetPermissions(message.Author);
			return await GetHelp(message, permissions);
		}

		[Command("Help", Permissions.Everyone, @"really really?")]
		public async Task<Embed> Help(CommandMessage message, string command)
		{
			return await GetHelp(message, command);
		}

		private static string? GetHelp(IGuild guild, string commandStr, Permissions permissions)
		{
			StringBuilder builder = new StringBuilder();
			List<Command> commands = CommandsService.GetCommands(commandStr);

			int count = 0;
			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				count++;
			}

			if (count <= 0)
				return null;

			builder.Append("__");
			////builder.Append(CommandsService.CommandPrefix);
			builder.Append(commandStr);
			builder.AppendLine("__");

			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				builder.Append(Utils.Characters.Tab);
				builder.Append(command.Permission);
				builder.Append(" - *");
				builder.Append(command.Help);
				builder.AppendLine("*");

				List<ParameterInfo> parameters = command.GetNeededParams();

				builder.Append("**");
				builder.Append(Utils.Characters.Tab);
				builder.Append(CommandsService.GetPrefix(guild));
				builder.Append(commandStr);
				builder.Append(" ");

				for (int i = 0; i < parameters.Count; i++)
				{
					if (i != 0)
						builder.Append(" ");

					builder.Append(GetParam(parameters[i]));
				}

				builder.Append("**");
				builder.AppendLine();
				builder.AppendLine();
			}

			return builder.ToString();
		}
	}
}
