using System;
using System.Collections.Generic;
using System.Linq;

namespace Finite.Commands
{
    internal sealed class CommandMap
    {
        private readonly CommandMapNode _root;

        public CommandMap(IReadOnlyList<ModuleInfo>? modules = null)
        {
            _root = new CommandMapNode();

            void AddCommandsForModule(ModuleInfo module, Stack<ReadOnlyMemory<char>> path)
            {
                foreach (var command in module.Commands)
                {
                    if (command.Aliases.Count == 0)
                        AddCommand(path.Reverse().ToArray(), command);
                    else
                        foreach (var alias in command.Aliases)
                        {
                            path.Push(alias.AsMemory());
                            AddCommand(path.Reverse().ToArray(), command);
                            path.Pop();
                        }
                }

                foreach (var submodule in module.Submodules)
                    AddModule(submodule, path);
            }

            void AddModule(ModuleInfo module, Stack<ReadOnlyMemory<char>> path)
            {
                if (module.Aliases.Count == 0)
                {
                    AddCommandsForModule(module, path);
                }
                else
                {
                    foreach (var moduleAlias in module.Aliases)
                    {
                        path.Push(moduleAlias.AsMemory());
                        AddCommandsForModule(module, path);
                        path.Pop();
                    }
                }
            }

            if (modules != null)
            {
                var pathStack = new Stack<ReadOnlyMemory<char>>();
                foreach (var module in modules)
                    AddModule(module, pathStack);
            }
        }

        public IEnumerable<CommandMatch> GetCommands(ReadOnlyMemory<char>[] commandPath)
            => _root.FindCommands(commandPath, 0);

        public bool AddCommand(ReadOnlyMemory<char>[] path, CommandInfo command)
            => _root.Add(command, path, 0);

        public bool RemoveCommand(ReadOnlyMemory<char>[] path, CommandInfo command)
            => _root.Remove(path, command, 0);

        private sealed class CommandMapNode
        {
            private readonly MultiMap<ReadOnlyMemory<char>, CommandInfo> _commands;
            private readonly Dictionary<ReadOnlyMemory<char>, CommandMapNode> _nodes;

            public CommandMapNode()
            {
                _commands = new MultiMap<ReadOnlyMemory<char>, CommandInfo>(
                    CommandEqualityComparer.Default);
                _nodes = new Dictionary<ReadOnlyMemory<char>, CommandMapNode>(
                    CommandEqualityComparer.Default);
            }

            public IEnumerable<CommandMatch> FindCommands(ReadOnlyMemory<char>[] segments,
                int startIndex)
            {
                if (startIndex >= segments.Length)
                    yield break;

                var segment = segments[startIndex];

                if (_nodes.TryGetValue(segment, out var node))
                {
                    var commands = node.FindCommands(segments,
                        startIndex + 1);
                    foreach (var command in commands)
                        yield return command;
                }

                if (_commands.TryGetValues(segment, out var matches))
                {
                    foreach (var match in matches)
                        yield return new CommandMatch(match,
                            segments.Skip(startIndex + 1).ToArray(),
                            segments.Take(startIndex + 1).ToArray());
                }
            }

            public bool Add(CommandInfo command, ReadOnlyMemory<char>[] segments,
                int startIndex)
            {
                if (startIndex == segments.Length - 1)
                {
                    return _commands.TryAddValue(segments[startIndex], command);
                }
                else
                {
                    var segment = segments[startIndex];
                    var node = _nodes.GetOrAdd(segment,
                        (_) => new CommandMapNode());

                    return node.Add(command, segments, startIndex + 1);
                }
            }

            public bool Remove(ReadOnlyMemory<char>[] segments, CommandInfo command,
                int startIndex)
            {
                if (startIndex == segments.Length - 1)
                {
                    return _commands.TryRemoveValue(segments[startIndex],
                        command);
                }
                else
                {
                    var segment = segments[startIndex];
                    if (_nodes.TryGetValue(segment, out var node))
                        return node.Remove(segments, command,
                            startIndex + 1);
                }

                return false;
            }
        }
    }
}
