using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace JMC.Extension.Server.Datas.Workspace
{
    internal class WorkspaceContainer : List<Workspace>
    {
        public class JMCTokenQueryResult
        {
            public Workspace Workspace { get; set; }
            public DocumentUri DocumentUri { get; set; }
            public List<JMCToken> Tokens { get; set; }
            public JMCTokenQueryResult(Workspace workspace, DocumentUri documentUri, List<JMCToken> tokens)
            {
                Workspace = workspace;
                DocumentUri = documentUri;
                Tokens = tokens;
            }
        }

        /// <summary>
        /// Get a <see cref="JMCFile"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public JMCFile? GetJMCFile(DocumentUri uri)
        {
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var f = item.FindJMCFile(uri);
                if (f != null)
                {
                    return f;
                }
            }
            return null;
        }

        /// <summary>
        /// Add a <see cref="JMCFile"/>
        /// </summary>
        /// <param name="uri"></param>
        public void AddJMCFile(DocumentUri uri)
        {
            var workspaces = ToArray().AsSpan();
            for (var i = 0; i < workspaces.Length; i++)
            {
                ref var workspace = ref workspaces[i];
                var wsPath = workspace.DocumentUri.GetFileSystemPath();
                var filePath = uri.GetFileSystemPath();
                if (filePath == null || wsPath == null) continue;
                if (!filePath.IsSubDirectoryOf(wsPath)) continue;

                var jmcFile = new JMCFile(uri);
                this[i].JMCFiles.Add(jmcFile);
            }
        }

        /// <summary>
        /// Remove a <see cref="JMCFile"/>
        /// </summary>
        /// <param name="uri"></param>
        public void RemoveJMCFile(DocumentUri uri)
        {
            var workspaces = ToArray().AsSpan();
            for (var i = 0; i < workspaces.Length; i++)
            {
                ref var workspace = ref workspaces[i];
                var wsPath = workspace.DocumentUri.GetFileSystemPath();
                var filePath = uri.GetFileSystemPath();
                if (filePath == null || wsPath == null) continue;

                var item = this[i].JMCFiles.FirstOrDefault(v => v.DocumentUri.Equals(uri));
                if (item == null) continue;
                this[i].JMCFiles.Remove(item);
            }
        }

        /// <summary>
        /// Get all variables of all <see cref="JMCFile"/>
        /// </summary>
        /// <returns></returns>
        public List<JMCTokenQueryResult> GetJMCVariables()
        {
            var tokens = new List<JMCTokenQueryResult>();
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var files = item.JMCFiles.ToArray().AsSpan();
                for (var j = 0; j < files.Length; j++)
                {
                    ref var file = ref files[j];
                    var result = new JMCTokenQueryResult(item, file.DocumentUri, file.Lexer.Variables.ToList());
                    tokens.Add(result);
                }
            }
            return tokens;
        }

        /// <summary>
        /// Get all function calls of all <see cref="JMCFile"/>
        /// </summary>
        /// <returns></returns>
        public List<JMCTokenQueryResult> GetJMCFunctionCalls()
        {
            var tokens = new List<JMCTokenQueryResult>();
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var files = item.JMCFiles.ToArray().AsSpan();
                for (var j = 0; j < files.Length; j++)
                {
                    ref var file = ref files[j];
                    var result = new JMCTokenQueryResult(item, file.DocumentUri, file.Lexer.FunctionCalls.ToList());
                    tokens.Add(result);
                }
            }
            return tokens;
        }

        /// <summary>
        /// Get all function defines of all <see cref="JMCFile"/>
        /// </summary>
        /// <returns></returns>
        public List<JMCTokenQueryResult> GetJMCFunctionDefines()
        {
            var tokens = new List<JMCTokenQueryResult>();
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var files = item.JMCFiles.ToArray().AsSpan();
                for (var j = 0; j < files.Length; j++)
                {
                    ref var file = ref files[j];
                    var result = new JMCTokenQueryResult(item, file.DocumentUri, file.Lexer.FunctionDefines.ToList());
                    tokens.Add(result);
                }
            }
            return tokens;
        }
    }
}
