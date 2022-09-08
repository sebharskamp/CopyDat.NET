using Bogus;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public static class FakerExtensions
{
    public static List<T> AsCleanRelationTable<T, TProp1, TProp2>(this Faker<T> fakerSet, int numToSeed, Expression<Func<T, TProp1>> propertyOne, Expression<Func<T, TProp2>> propertyTwo) where T : class
    {
        return fakerSet.Generate(numToSeed).GroupBy(c => new { propertyOne, propertyTwo }).Select(c => c.FirstOrDefault()).ToList();
    }
}