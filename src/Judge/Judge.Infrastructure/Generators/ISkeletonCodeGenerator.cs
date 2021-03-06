﻿using Judge.Infrastructure.ProblemsSchema;

namespace Judge.Infrastructure.Generators
{
    /// <summary>
    ///     Generates base code in specified language (depends on implementation)
    /// </summary>
    public interface ISkeletonCodeGenerator
    {
        void GenerateFor(Problem problem);
    }
}