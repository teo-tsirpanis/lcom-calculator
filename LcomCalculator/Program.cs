// Copyright (c) 2021 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LcomCalculator.Core;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace LcomCalculator
{
    class Program
    {
        private string AssemblyFile { get; }
        private string? OutputCsv { get; init; }
        private bool NoInternal { get; init; }
        private bool NoCompilerGenerated { get; init; }

        public Program(string assemblyFile) {
            AssemblyFile = assemblyFile;
        }

        private List<(string, int)> GetLcomOfAssemblyTypes(AssemblyDefinition asm)
        {
            var typesQuery =
                asm.Modules
                    .SelectMany(ModuleDefinitionRocks.GetAllTypes)
                    .AsParallel().AsOrdered();
            typesQuery = typesQuery.Where(type => type.FullName != "<Module>");
            typesQuery = typesQuery.Where(type => !type.IsInterface);
            if (NoInternal)
                typesQuery = typesQuery.Where(type => type.IsPublic);
            if (NoCompilerGenerated)
                typesQuery = typesQuery.Where(type =>
                    type.CustomAttributes.All(attr =>
                        attr.AttributeType.FullName != typeof(CompilerGeneratedAttribute).FullName));
            return
                typesQuery
                    .Select(type => (type.FullName, Calculator.CalculateLackOfCohesion(type)))
                    .ToList();
        }

        private int Run()
        {
            if (!File.Exists(AssemblyFile))
            {
                Console.WriteLine($"Error: '{AssemblyFile}' does not exist.");
                return 1;
            }

            List<(string, int)> results;
            using (var asm = AssemblyDefinition.ReadAssembly(AssemblyFile))
                results = GetLcomOfAssemblyTypes(asm);

            var outputSb = new StringBuilder("TypeName,LCOM");
            outputSb.AppendLine();
            foreach (var (typeName, lcom) in results)
                outputSb.AppendLine($"{typeName},{lcom}");

            if (OutputCsv is null)
                Console.WriteLine(outputSb);
            else
            {
                var output = Path.GetFullPath(OutputCsv);
                Console.Error.WriteLine($"Saving output to {output}...");
                File.WriteAllText(output, outputSb.ToString());
            }

            return 0;
        }

        /// <summary>
        /// Calculates the Lack of Cohesion of Methods (LCOM) of all classes of a .NET assembly.
        /// </summary>
        /// <param name="assembly">The assembly file.</param>
        /// <param name="output">The path of the CSV file that will contain the results.
        /// If not specified, results will be printed on the console.</param>
        /// <param name="noInternal">Whether non-public types will be excluded.</param>
        /// <param name="noCompilerGenerated">Whether compiler-generated types will be excluded.</param>
        static int Main(string assembly, string? output = null, bool noInternal = false,
            bool noCompilerGenerated = true)
        {
            var program = new Program(assembly)
            {
                OutputCsv = output, NoInternal = noInternal,
                NoCompilerGenerated = noCompilerGenerated
            };
            return program.Run();
        }
    }
}
