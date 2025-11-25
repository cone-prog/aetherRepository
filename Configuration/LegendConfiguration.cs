// Configuration/LegendConfiguration.cs
using UrbanLayoutGenerator.Models;
using System.Collections.Generic;

using System;

namespace UrbanLayoutGenerator.Configuration
{
    public static class LegendConfiguration
    {
        public static readonly Dictionary<string, ElementType> ColorMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            ["#FF0000"] = ElementType.RedLine,           // Красные линии
            ["#FF0000FF"] = ElementType.RedLine,
            ["#000000"] = ElementType.SiteBoundary,      // Граница участка
            ["#000000FF"] = ElementType.SiteBoundary,
            ["#00FF00"] = ElementType.LandscapingBoundary, // Граница благоустройства
            ["#00FF00FF"] = ElementType.LandscapingBoundary,
            ["#0000FF"] = ElementType.Building,          // Проектируемое здание
            ["#0000FFFF"] = ElementType.Building,
            ["#FFFF00"] = ElementType.UndergroundFloor,  // Граница подземного этажа
            ["#FFFF00FF"] = ElementType.UndergroundFloor,
            ["#FF00FF"] = ElementType.PlaygroundChildren, // Детская площадка
            ["#FF00FFFF"] = ElementType.PlaygroundChildren,
            ["#00FFFF"] = ElementType.PlaygroundSports,  // Спортивная площадка
            ["#00FFFFFF"] = ElementType.PlaygroundSports,
            ["#FFA500"] = ElementType.PlaygroundAdults,  // Площадка для отдыха взрослых
            ["#FFA500FF"] = ElementType.PlaygroundAdults,
            ["#A52A2A"] = ElementType.SocialKindergarten, // Детский сад
            ["#A52A2AFF"] = ElementType.SocialKindergarten,
            ["#8B4513"] = ElementType.SocialSchool,      // Школа
            ["#8B4513FF"] = ElementType.SocialSchool
        };
    }
}