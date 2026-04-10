using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;

namespace VirtualMicroConsole
{

    class ScriptReader
    {
        public static string path = "";
        public static VirtualMicroConsole LoadScripts(Game game)
        {
            SaveSystem.LoadFileCodePath(ref path);
            if (!File.Exists(path))
            {
                Console.WriteLine("Fichier introuvable : " + path);
                return new VirtualMicroConsole1();
            }

            return CompileAndCreate(path, game);
        }
        private static VirtualMicroConsole CompileAndCreate(string path, Game game)
        {
            string code = File.ReadAllText(path);
            if (!code.Contains("namespace"))
            {
                code = "namespace VirtualMicroConsole { " + code + " }";
            }
            var tree = CSharpSyntaxTree.ParseText(code);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            var compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                new[] { tree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                string log = "";

                log += $"{Environment.NewLine}Erreur dans {path} ------------------------------------------------------------------------------------------------" + Environment.NewLine;
                foreach (var diagnostic in result.Diagnostics)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();

                    var line = lineSpan.StartLinePosition.Line + 1;
                    var column = lineSpan.StartLinePosition.Character + 1;
                    var lines = code.Split(Environment.NewLine);
                    log += ">>> " + lines[line - 1] + Environment.NewLine;
                    log +=$"Erreur ({line},{column}) : {diagnostic.GetMessage()}" + Environment.NewLine;
                }
                log += Environment.NewLine + "-----------------------------------------------------------------------------------------------------------";
                Utils.debug(log);
                string logPath = Path.GetDirectoryName(path) ?? "";
                logPath += "\\log.txt";
                File.WriteAllText(logPath, log);

                return new VirtualMicroConsole1();
            }           

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(VirtualMicroConsole).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            if (type == null)
            {
                Utils.debug($"Aucun script valide dans {path}");
                return new VirtualMicroConsole1();
            }

            var script = (VirtualMicroConsole)Activator.CreateInstance(type);
            return script;
        }
    }
}
