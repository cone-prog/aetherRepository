using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UrbanLayoutGenerator.Core.Models;

namespace UrbanLayoutGenerator.Configuration;

public class SymbolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ElementType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> FillColors { get; set; } = new();
    public List<string> StrokeColors { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> StrokeDashArrays { get; set; } = new();
    public double? MinStrokeWidth { get; set; }
    public double? MaxStrokeWidth { get; set; }
    public List<string> TextMarkers { get; set; } = new();

    public double? TypicalArea { get; set; }
    public double? AspectRatio { get; set; }

    public double? MinArea { get; set; }
    public double? MaxArea { get; set; }
}

public class LibraryPassport
{
    public string Name { get; set; } = "Паспорт библиотеки условных обозначений СПОЗУ";
    public string Version { get; set; } = "1.0";
    public DateTime Created { get; set; } = DateTime.Now;
    public string Description { get; set; } = "Карта соответствий для автоматической классификации объектов";

    public Dictionary<string, SymbolDefinition> Symbols { get; set; } = new();

    public static LibraryPassport CreateDefault()
    {
        var passport = new LibraryPassport();

        passport.Symbols["site_boundary"] = new SymbolDefinition
        {
            Name = "Граница земельного участка",
            Type = ElementType.SiteBoundary,
            Description = "Внешняя граница земельного участка",
            StrokeColors = new List<string>
            {
                "#000000", "black", "#000000ff", "rgb(0,0,0)",
                "#000001", "#010101"
            },
            MinStrokeWidth = 0.8,
            MaxStrokeWidth = 2.5,
            TextMarkers = new List<string> { "участок", "граница", "территория" }
        };

        passport.Symbols["red_line"] = new SymbolDefinition
        {
            Name = "Красные линии",
            Type = ElementType.RedLine,
            Description = "Линии градостроительного регулирования",
            StrokeColors = new List<string>
            {
                "#ff0000", "red", "#ff0000ff", "rgb(255,0,0)",
                "#fe0000", "#ff0101"
            },
            StrokeDashArrays = new List<string>
            {
                "5,5", "10,5", "8,8,8", "6,6", "7,3"
            },
            MinStrokeWidth = 0.5,
            MaxStrokeWidth = 1.8,
            TextMarkers = new List<string> { "красная", "линия", "КЛ" }
        };


        passport.Symbols["landscaping"] = new SymbolDefinition
        {
            Name = "Граница благоустройства",
            Type = ElementType.LandscapingBoundary,
            Description = "Граница зоны озеленения и благоустройства",
            FillColors = new List<string>
            {
                "#90ee90", "#98fb98", "rgb(144,238,144)",
                "#8fed8f", "#92f592"
            },
            StrokeColors = new List<string>
            {
                "#006400", "#008000", "#228b22"
            },
            TypicalArea = 500,
            TextMarkers = new List<string> { "благоустройство", "озеленение", "газон" }
        };

        passport.Symbols["building"] = new SymbolDefinition
        {
            Name = "Проектируемые здания",
            Type = ElementType.Building,
            Description = "Контуры проектируемых жилых зданий",
            FillColors = new List<string>
            {
                "#d3d3d3", "#c0c0c0", "gray", "rgb(211,211,211)",
                "#cccccc", "#d9d9d9"
            },
            StrokeColors = new List<string>
            {
                "#000000", "#696969", "#808080"
            },
            MinStrokeWidth = 0.3,
            MaxStrokeWidth = 1.2,
            TypicalArea = 400,
            MinArea = 50,
            MaxArea = 2000,
            TextMarkers = new List<string>
            {
                "секция", "жилой дом", "корп.", "Сек.", "лит.",
                "этаж", "подъезд", "квартира"
            }
        };

        passport.Symbols["underground"] = new SymbolDefinition
        {
            Name = "Граница подземного этажа",
            Type = ElementType.UndergroundFloor,
            Description = "Контуры подземных сооружений и этажей",
            StrokeColors = new List<string>
            {
                "#0000ff", "blue", "#0000ffff", "rgb(0,0,255)",
                "#0000fe", "#0101ff"
            },
            StrokeDashArrays = new List<string>
            {
                "3,3", "5,2,2,2", "4,4", "2,2"
            },
            MinStrokeWidth = 0.3,
            MaxStrokeWidth = 1.2,
            TextMarkers = new List<string>
            {
                "-1 эт", "подзем", "цоколь", "тех.эт",
                "подвал", "паркинг", "П1", "П2"
            }
        };

        passport.Symbols["playground_children"] = new SymbolDefinition
        {
            Name = "Детская площадка",
            Type = ElementType.PlaygroundChildren,
            Description = "Зона детских игровых площадок",
            FillColors = new List<string>
            {
                "#ffb6c1", "#ff69b4", "rgb(255,182,193)",
                "#ffc0cb", "#ffa8b8"
            },
            TypicalArea = 150,
            MinArea = 50,
            MaxArea = 500,
            TextMarkers = new List<string>
            {
                "детск", "игров", "площадка", "для детей",
                "игровая", "качели", "горка"
            }
        };

        passport.Symbols["playground_sports"] = new SymbolDefinition
        {
            Name = "Спортивная площадка",
            Type = ElementType.PlaygroundSports,
            Description = "Зона спортивных сооружений",
            FillColors = new List<string>
            {
                "#87ceeb", "#add8e6", "rgb(135,206,235)",
                "#b0e0e6", "#afeeee"
            },
            TypicalArea = 300,
            MinArea = 100,
            MaxArea = 1000,
            AspectRatio = 1.5,
            TextMarkers = new List<string>
            {
                "спорт", "тренаж", "баскетбол", "футбол",
                "волейбол", "турник", "спортивная"
            }
        };

        passport.Symbols["playground_adults"] = new SymbolDefinition
        {
            Name = "Площадка для отдыха взрослых",
            Type = ElementType.PlaygroundAdults,
            Description = "Зона отдыха для взрослого населения",
            FillColors = new List<string>
            {
                "#daa520", "#f0e68c", "rgb(218,165,32)",
                "#eee8aa", "#fafad2"
            },
            TypicalArea = 100,
            MinArea = 30,
            MaxArea = 300,
            TextMarkers = new List<string>
            {
                "отдых", "взросл", "беседка", "скамейк",
                "отдыха", "рекреация"
            }
        };

        passport.Symbols["kindergarten"] = new SymbolDefinition
        {
            Name = "Детский сад (ДОУ)",
            Type = ElementType.SocialKindergarten,
            Description = "Здание детского дошкольного учреждения",
            FillColors = new List<string>
            {
                "#ffa500", "#ff8c00", "orange", "rgb(255,165,0)",
                "#ffb347", "#ffa54f"
            },
            TypicalArea = 1200,
            MinArea = 800,
            MaxArea = 3000,
            TextMarkers = new List<string>
            {
                "ДОУ", "детсад", "сад", "детский сад",
                "ясли", "дошкольное", "группа"
            }
        };

        passport.Symbols["school"] = new SymbolDefinition
        {
            Name = "Школа (СОУ)",
            Type = ElementType.SocialSchool,
            Description = "Здание общеобразовательной школы",
            FillColors = new List<string>
            {
                "#8b4513", "#a0522d", "brown", "rgb(139,69,19)",
                "#cd853f", "#d2691e"
            },
            TypicalArea = 5000,
            MinArea = 2000,
            MaxArea = 15000,
            TextMarkers = new List<string>
            {
                "Школа", "СОУ", "образован", "учебное",
                "класс", "спортзал", "столовая"
            }
        };

        return passport;
    }

    public string ToJson(bool indented = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(this, options);
    }

    public void SaveToFile(string filePath)
    {
        var json = ToJson();
        File.WriteAllText(filePath, json);
        Console.WriteLine($"Паспорт библиотеки сохранен: {filePath}");
    }

    public static LibraryPassport LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл паспорта не найден: {filePath}");

        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<LibraryPassport>(json, options)
                ?? throw new InvalidOperationException("Не удалось загрузить паспорт из файла");
    }
}
