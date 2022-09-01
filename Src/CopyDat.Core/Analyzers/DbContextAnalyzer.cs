using System;
using System.Collections.Generic;
using System.Linq;

public class DbContextAnalyzer
{
    internal static List<Type> GetEntityHierarchy(ref List<Type> entities)
    {
        var levels = entities.ToDictionary(e => e.GenericTypeArguments.First(), e => 0);
        DbContextAnalyzer.AnalyzeEnitityDepth(ref entities, ref levels, 0);
        return levels.OrderBy(l => l.Value).Select(l => l.Key).ToList();
    }

    internal static void AnalyzeEnitityDepth(ref List<Type> entities, ref Dictionary<Type, int> levels, int depth)
    {
        foreach (var entity in entities)
        {
            IEnumerable<Type> properties = entity.GenericTypeArguments.First().GetProperties().Select(tp => tp.PropertyType).ToList();
            var isLevel = 0;
            foreach (var property in properties)
            {
                if (levels.TryGetValue(property, out int level))
                {
                    if (level >= isLevel)
                    {
                        isLevel = level + 1;
                    }
                }
            }
            levels[entity.GenericTypeArguments.First()] = isLevel;
        }

        IOrderedEnumerable<KeyValuePair<Type, int>> orderedLevels = levels.OrderByDescending(l => l.Value);
        var previousLevel = orderedLevels.First().Value;
        bool foundHierarchy = false;
        var currentLevels = orderedLevels.Select(ol => ol.Value).ToList();
        foreach (var l in currentLevels)
        {
            if (Math.Abs(l - previousLevel) > 1 || depth == currentLevels.Max())
            {
                foundHierarchy = true;
            }
            previousLevel = l;
        }

        if (!foundHierarchy)
        {
            entities.Reverse();
            depth = currentLevels.Max();
            AnalyzeEnitityDepth(ref entities, ref levels, depth);
        }
    }
}
