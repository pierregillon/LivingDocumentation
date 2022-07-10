using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LivingDocumentation.BoundedContextCanvas.Domain;
using LivingDocumentation.BoundedContextCanvas.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LivingDocumentation.BoundedContextCanvas.Tests
{
    public class SourceCodeVisitorTests
    {
        [Fact]
        public void Extract_class_name_from_source_code()
        {
            const string sourceCode = @"public class CreateUser { }";

            var typeDefinitions = Visit(sourceCode);

            typeDefinitions.Should().BeEquivalentTo(new[] {
                new TypeDefinition(new TypeFullName("CreateUser"), Array.Empty<TypeFullName>())
            });
        }

        [Fact]
        public void Extract_class_namespace_from_source_code()
        {
            const string sourceCode = @"
    namespace Test
    {
        public class CreateUser { }
    }
";
            var typeDefinitions = Visit(sourceCode);

            typeDefinitions.Should().BeEquivalentTo(new[] {
                new TypeDefinition(new TypeFullName("Test.CreateUser"), Array.Empty<TypeFullName>())
            });
        }

        [Fact]
        public void Extract_class_implemented_interfaces_from_source_code()
        {
            const string sourceCode = @"
    namespace Test
    {
        public interface ICommand { }
        public interface IDisposable { }
        public class CreateUser : ICommand, IDisposable { }
    }
";
            var typeDefinitions = Visit(sourceCode);

            typeDefinitions.Should().BeEquivalentTo(new[] {
                new TypeDefinition(new TypeFullName("Test.CreateUser"), new [] {
                    new TypeFullName("Test.ICommand"),
                    new TypeFullName("Test.IDisposable")
                })
            });
        }

        [Fact]
        public void Extract_record_name_from_source_code()
        {
            const string sourceCode = @"public record CreateUser;";

            var typeDefinitions = Visit(sourceCode);

            typeDefinitions.Should().BeEquivalentTo(new []{
                new TypeDefinition(
                    new TypeFullName("CreateUser"),
                    new[] { new TypeFullName("System.IEquatable<CreateUser>") }
                )
            });
        }

        [Fact]
        public void Extract_record_namespace_from_source_code()
        {
            const string sourceCode = @"
    namespace Test
    {
        public record CreateUser;
    }
";
            var typeDefinitions = Visit(sourceCode);

            typeDefinitions.Should().BeEquivalentTo(new[] {
                new TypeDefinition(
                    new TypeFullName("Test.CreateUser"), 
                    new [] { new TypeFullName("System.IEquatable<Test.CreateUser>") }
                )
            });
        }

        [Fact]
        public void Extract_record_implemented_interfaces_from_source_code()
        {
            const string sourceCode = @"
    namespace Test
    {
        public interface ICommand { }
        public interface IDisposable { }
        public record CreateUser : ICommand, IDisposable;
    }
";
            var typeDefinitions = Visit(sourceCode);

            typeDefinitions.Should().BeEquivalentTo(new[] {
                new TypeDefinition(new TypeFullName("Test.CreateUser"), new [] {
                    new TypeFullName("System.IEquatable<Test.CreateUser>"),
                    new TypeFullName("Test.ICommand"),
                    new TypeFullName("Test.IDisposable")
                })
            });
        }

        public static IEnumerable<TypeDefinition> Visit(string source, params string[] ignoreErrorCodes)
        {
            source
                .Should()
                .NotBeNullOrWhiteSpace("without source code there is nothing to test");

            var syntaxTree = CSharpSyntaxTree.ParseText(source.Trim());
            
            var compilation = CSharpCompilation.Create("Test")
                .WithOptions(
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        .WithAllowUnsafe(true)
                )
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            compilation
                .GetDiagnostics()
                .Where(d => !ignoreErrorCodes.Contains(d.Id))
                .Should()
                .HaveCount(0, "there shoudn't be any compile errors");

            var semanticModel = compilation.GetSemanticModel(syntaxTree, true);

            var types = new List<TypeDefinition>();
            var visitor = new SourceCodeVisitor(semanticModel, types);
            
            visitor.Visit(syntaxTree.GetRoot());
            
            return types;
        }
    }
}