﻿using BoundedContextCanvasGenerator.Domain;
using BoundedContextCanvasGenerator.Domain.Types;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace BoundedContextCanvasGenerator.Infrastructure.Types;

public class SourceCodeAnalyserTypeDefinitionRepository : ITypeDefinitionRepository
{
    private readonly TypeDefinitionFactory _factory;

    public SourceCodeAnalyserTypeDefinitionRepository(TypeDefinitionFactory factory) => _factory = factory;

    public async IAsyncEnumerable<TypeDefinition> GetAll(SolutionPath path)
    {
        var manager = new AnalyzerManager(path.Value);

        var workspace = manager.GetWorkspace();

        var compilations = await workspace.CurrentSolution.Projects
            .Select(x => x.GetCompilationAsync())
            .Pipe(Task.WhenAll);

        foreach (var result in await BuildTypeDefinitions(compilations)) {
            yield return result;
        }
    }

    private async Task<IReadOnlyCollection<TypeDefinition>> BuildTypeDefinitions(Compilation?[] compilations) 
        => await compilations.Pipe(RemoveEmpty).Pipe(_factory.Build);

    private static IEnumerable<Compilation> RemoveEmpty(Compilation?[] compilations) 
        => compilations
            .Where(x => x is not null)
            .Select(x => x!)
            .ToArray();
}