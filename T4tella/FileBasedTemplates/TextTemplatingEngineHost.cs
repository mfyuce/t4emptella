using Microsoft.VisualStudio.TextTemplating;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace T4tella
{
    public class TextTemplatingEngineHost : MarshalByRefObject, ITextTemplatingEngineHost
    {
        public virtual object GetHostOption(string optionName)
        {
            return (optionName == "CacheAssemblies") ? (object)true : null;
        }

        public virtual bool LoadIncludeText(string requestFileName,
                 out string content, out string location)
        {
            content = location = String.Empty;

            if (File.Exists(requestFileName))
            {
                content = File.ReadAllText(requestFileName);
                return true;
            }
            return false;
        }

        public virtual void LogErrors(CompilerErrorCollection errors)
        {
        }

        public virtual AppDomain ProvideTemplatingAppDomain(string content)
        {
            return AppDomain.CreateDomain("TemplatingHost AppDomain");
        }

        public virtual string ResolveAssemblyReference(string assemblyReference)
        {
            if (File.Exists(assemblyReference))
            {
                return assemblyReference;
            }

            string candidate = Path.Combine(Path.GetDirectoryName(TemplateFile),
                  assemblyReference);
            return File.Exists(candidate) ? candidate : String.Empty;
        }

        public virtual Type ResolveDirectiveProcessor(string processorName)
        {
            throw new Exception("Directive Processor not found");
        }

        public virtual string ResolveParameterValue(string directiveId,
                 string processorName, string parameterName)
        {
            if (directiveId == null)
            {
                throw new ArgumentNullException("directiveId");
            }
            if (processorName == null)
            {
                throw new ArgumentNullException("processorName");
            }
            if (parameterName == null)
            {
                throw new ArgumentNullException("parameterName");
            }

            return String.Empty;
        }

        public virtual string ResolvePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (File.Exists(path))
            {
                return path;
            }
            string candidate = Path.Combine(Path.GetDirectoryName(TemplateFile), path);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            return path;
        }

        public virtual void SetFileExtension(string extension)
        {
        }

        public virtual void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
        }

        public virtual IList<string> StandardAssemblyReferences
        {
            // bare minimum, returns the location of the System assembly
            get { return new[] { typeof(String).Assembly.Location }; }
        }

        public virtual IList<string> StandardImports
        {
            get { return new[] { "System" }; }
        }

        public string TemplateFile { get; set; }
    }
}