﻿#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using FastReport.Code.CodeDom.Compiler;


namespace FastReport.Code.VisualBasic
{
    public class VBCodeProvider : CodeDomProvider
    {
        

        public override CompilerResults CompileAssemblyFromSource(CompilerParameters cp, string code)
        {
            SyntaxTree codeTree = VisualBasicSyntaxTree.ParseText(code);
            VisualBasicCompilationOptions options = new VisualBasicCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                true,
                optimizationLevel: OptimizationLevel.Release,
                generalDiagnosticOption: ReportDiagnostic.Default);

            List<MetadataReference> references = new List<MetadataReference>();


            foreach (string reference in cp.ReferencedAssemblies)
                references.Add(GetReference(reference));

            AddExtraAssemblies(cp.ReferencedAssemblies, references);




            Compilation compilation = VisualBasicCompilation.Create(
                "_" + Guid.NewGuid().ToString("D"), new SyntaxTree[] { codeTree },
                references: references, options: options.WithEmbedVbCoreRuntime(true)
                );

            OnBeforeEmitCompilation(compilation);

            using (MemoryStream ms = new MemoryStream())
            {
                EmitResult results = compilation.Emit(ms);
                if (results.Success)
                {
                    return new CompilerResults()
                    {
                        CompiledAssembly = Assembly.Load(ms.ToArray())
                    };
                }
                else
                {
                    CompilerResults result = new CompilerResults();
                    foreach (Diagnostic d in results.Diagnostics)
                    {
                        if (d.Severity == DiagnosticSeverity.Error)
                        {
                            result.Errors.Add(new CompilerError()
                            {
                                ErrorText = d.GetMessage(),
                                ErrorNumber = d.Id,
                                Line = d.Location.GetLineSpan().StartLinePosition.Line,
                                Column = d.Location.GetLineSpan().StartLinePosition.Character
                            });
                        }
                    }
                    return result;
                }
            }

        }

        public override void Dispose()
        {

        }

       
    }
}
#endif