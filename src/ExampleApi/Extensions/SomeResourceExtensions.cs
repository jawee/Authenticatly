﻿using ExampleApi.Dtos;
using ExampleApi.Models;

namespace ExampleApi.Extensions;

public static class SomeResourceExtensions
{
    public static SomeResourceDto ToDto(this SomeResource model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name
        };
    }
}
