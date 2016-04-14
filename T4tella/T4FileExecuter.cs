using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace T4tella
{
    public class T4FilePartialRecord{
        public string[] ToBeAddedClasses { get; set; }
        public string[] ToAddedReferences { get; set; }
    }
    public class T4FileExecuter
    {
        public static string ProcessTemplate(string templateFileName, 
            string nameSpace,
            string className,
            string methodName,
            Func<string, string ,string , T4FilePartialRecord> partialGenerator, 
            Action<object> objectModifier)
        {
            if (string.IsNullOrWhiteSpace( templateFileName ))
            {
                throw new ArgumentNullException("the file name cannot be null");
            }
            if (!File.Exists(templateFileName))
            {
                throw new FileNotFoundException("the file cannot be found");
            }
            CustomCmdLineHost host = new CustomCmdLineHost();
            Engine engine = new Engine();
            host.TemplateFileValue = templateFileName;
            //Read the text template.
            string input = File.ReadAllText(templateFileName);
            //Transform the text template.
            var fn = Path.GetFileNameWithoutExtension(templateFileName);
            if(string.IsNullOrWhiteSpace(nameSpace)){
                nameSpace =fn + "_ns";
            }
            if (string.IsNullOrWhiteSpace(className))
            {
                className = fn ;
            }
            var lang = "";
            string[] refs = null;
            string output = engine.PreprocessTemplate(input, host, className, nameSpace, out lang, out refs);
            T4FilePartialRecord partial = null;
            if (partialGenerator != null) {
                partial = partialGenerator(templateFileName, className, nameSpace);
            }  
            var addedCodes = new List<string>() { output};
            if (partial != null) {
                if (partial.ToBeAddedClasses != null && partial.ToBeAddedClasses.Length > 0) {
                    addedCodes.AddRange(partial.ToBeAddedClasses.Where(t => !string.IsNullOrWhiteSpace(t)));
                }
            }
            var assembly = Compile(addedCodes.ToArray(),partial.ToAddedReferences);

            Type type = assembly.GetType(nameSpace + "." + className);
            object obj =  Activator.CreateInstance(type);
            if (string.IsNullOrWhiteSpace(methodName)) {
                methodName = "TransformText";
            }
            objectModifier(obj);
            //type.GetProperty("Data").SetValue(obj, );
            var m = type.GetMethod(methodName);
           return (string)m.Invoke(obj, new object[]{});
        }

        public static Assembly Compile(string[] code, string[] extraRefs)
        {
            var hash = string.Join("\r\n", code).GetHashCode().ToString();
            string outputFile = hash + ".dll";
            if (File.Exists(outputFile))
            {
                return Assembly.LoadFrom(outputFile);
            }

            SyntaxTree[] syntaxTree = code.Select(t => CSharpSyntaxTree.ParseText(t)).ToArray();
            string assemblyName = Path.GetRandomFileName();
            List<MetadataReference> references = new List<MetadataReference> 
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };
            references.Add(MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location));
            if (extraRefs != null) {
                foreach (var item in extraRefs)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(item);
                        references.Add(MetadataReference.CreateFromAssembly(assembly));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            foreach (var item in Directory.GetFiles(".", "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(item);
                    references.Add(MetadataReference.CreateFromAssembly(assembly));
                }
                catch (Exception)
                {
                }
            }
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTree,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var arr = ms.ToArray();
                    File.WriteAllBytes(outputFile, arr);
                    return Assembly.Load(ms.ToArray());
                }
            }
            return null;
        }
    }
}
