using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server;

public class ParseCommandException(string message) : Exception(message)
{
}

public class Command
{
    public required string Name { get; init; }

    public Dictionary<string, string> Arguments { get; init; } = [];

    public static Command Parse(string data)
    {
        data = data.Trim();
        if (data.Length < 1) throw new ParseCommandException("Command line string is empty");

        var tokens = data.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 1) throw new ParseCommandException("Command line string is empty");

        var command = new Command { Name = tokens[0] };
        if (tokens.Length < 2) return command;

        string key = "";
        StringBuilder value = new();
        for (int i = 1; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("--"))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    // push previous value and key
                    command.Arguments.Add(key, value.ToString().Trim());
                    key = "";
                    value.Clear();
                }

                // key
                key = token[2..];
                if (key.Length < 1)
                {
                    throw new ParseCommandException($"Key \"{key}\" is invalid");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ParseCommandException("Values was provided without a key");
                }
                // key exists

                value.Append(token);
                value.Append(' ');
            }
        }

        if (!string.IsNullOrEmpty(key))
        {
            // push last value and key
            command.Arguments.Add(key, value.ToString().Trim());
            key = "";
            value.Clear();
        }

        return command;
    }
}