﻿using BoundedContextCanvasGenerator.Domain.Configuration;
using BoundedContextCanvasGenerator.Domain.Configuration.Predicates;
using BoundedContextCanvasGenerator.Domain.Types;

namespace BoundedContextCanvasGenerator.Infrastructure.Configuration.Parsing;

public class CommandConfigurationDto
{
    public string? Type { get; set; }
    public ImplementingConfigurationDto? Implementing { get; set; }

    public IEnumerable<ITypeDefinitionPredicate> Build()
    {
        if (Type is not null) {
            yield return new OfType(Type.ToTypeKind());
        }
        if (Implementing is not null) {
            yield return Implementing.Build();
        }
    }
}