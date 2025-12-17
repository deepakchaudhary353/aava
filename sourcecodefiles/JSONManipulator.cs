
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

public class JSONManipulator
{
    private readonly JsonSerializerOptions _pretty = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    private JsonObject _root;

    /// <summary>
    /// Create a manipulator initialized with the default schema document.
    /// </summary>
    public JSONManipulator()
    {
        _root = CreateDefaultDocument();
    }

    /// <summary>
    /// Resets the internal JSON document to the default schema.
    /// </summary>
    public void ResetToDefault() => _root = CreateDefaultDocument();

    /// <summary>
    /// Loads JSON from string and replaces the internal document.
    /// </summary>
    public void Load(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON input is empty.");

        var node = JsonNode.Parse(json) as JsonObject;
        if (node == null)
            throw new FormatException("Input is not a JSON object.");
        _root = node;
    }

    /// <summary>
    /// Returns compact JSON string.
    /// </summary>
    public string ToJson() => _root.ToJsonString();

    /// <summary>
    /// Returns pretty-printed JSON string.
    /// </summary>
    public string ToPrettyJson() => _root.ToJsonString(_pretty);

    /// <summary>
    /// Gets a value by dot-path (e.g., "owner.email", "metrics.cpu", "metadata.labels.env").
    /// Returns null if path does not exist.
    /// </summary>
    public JsonNode? Get(string path)
    {
        var (parent, key) = TraverseToParent(path, createMissing: false);
        if (parent == null || key == null) return null;
        return parent.ContainsKey(key) ? parent[key] : null;
    }

    /// <summary>
    /// Sets a value by dot-path. Creates intermediate objects as needed.
    /// Examples:
    ///   Set("owner.name", JsonValue.Create("Alice"));
    ///   Set("metrics.cpu", JsonValue.Create(0.42));
    ///   Set("metadata.labels.env", JsonValue.Create("staging"));
    /// </summary>
    public void Set(string path, JsonNode? value)
    {
        var (parent, key) = TraverseToParent(path, createMissing: true);
        if (parent == null || key == null)
            throw new ArgumentException($"Invalid path '{path}'.");
        parent[key] = value;
    }

    /// <summary>
    /// Adds a property at the dot-path only if it doesn't already exist.
    /// Throws if the property exists.
    /// </summary>
    public void Add(string path, JsonNode? value)
    {
        var (parent, key) = TraverseToParent(path, createMissing: true);
        if (parent == null || key == null)
            throw new ArgumentException($"Invalid path '{path}'.");
        if (parent.ContainsKey(key))
            throw new InvalidOperationException($"Property '{path}' already exists.");
        parent[key] = value;
    }

    /// <summary>
    /// Removes a property at the dot-path. Returns true if removed; false if not found.
    /// </summary>
    public bool Remove(string path)
    {
        var (parent, key) = TraverseToParent(path, createMissing: false);
        if (parent == null || key == null) return false;
        return parent.Remove(key);
    }

    /// <summary>
    /// Validates required top-level and nested properties exist and have acceptable types.
    /// Returns a list of validation errors. Empty list means validation passed.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Required top-level properties (12)
        string[] requiredTop = new[]
        {
            "id","name","version","enabled","tags","owner","metrics",
            "schedule","retryPolicy","endpoints","metadata","lastUpdated"
        };

        foreach (var prop in requiredTop)
        {
            if (!HasProperty(_root, prop))
                errors.Add($"Missing required property: {prop}");
        }

        // Type checks (best-effort)
        CheckType(errors, "id", JsonValueKind.String);
        CheckType(errors, "name", JsonValueKind.String);
        CheckType(errors, "version", JsonValueKind.String);
        CheckType(errors, "enabled", JsonValueKind.True, JsonValueKind.False); // boolean
        CheckArray(errors, "tags");
        CheckObject(errors, "owner");
        CheckObject(errors, "metrics");
        CheckObject(errors, "schedule");
        CheckObject(errors, "retryPolicy");
        CheckArray(errors, "endpoints");
        CheckObject(errors, "metadata");
        CheckType(errors, "lastUpdated", JsonValueKind.String); // ISO-8601 expected

        // Nested sub-properties (at least two)
        RequireNested(errors, "owner.name", JsonValueKind.String);
        RequireNested(errors, "owner.email", JsonValueKind.String);
        RequireNested(errors, "metrics.cpu", JsonValueKind.Number);
        RequireNested(errors, "metrics.memoryMB", JsonValueKind.Number);
        RequireNested(errors, "metadata.labels.env", JsonValueKind.String);

        return errors;
    }

    /// <summary>
    /// Performs a merge from source JSON string into the current document.
    /// If deep == true, merges nested objects recursively; otherwise shallow overwrite at top level.
    /// </summary>
    public void Merge(string sourceJson, bool deep = true)
    {
        var src = JsonNode.Parse(sourceJson) as JsonObject
                  ?? throw new FormatException("Source JSON must be an object.");
        if (deep)
            DeepMerge(_root, src);
        else
            ShallowMerge(_root, src);
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private static JsonObject CreateDefaultDocument()
    {
        return new JsonObject
        {
            ["id"] = "svc-001",
            ["name"] = "Example Service",
            ["version"] = "1.0.0",
            ["enabled"] = true,
            ["tags"] = new JsonArray("core", "stable"),
            ["owner"] = new JsonObject
            {
                ["name"] = "Deepak",
                ["email"] = "deepak@example.com"
            },
            ["metrics"] = new JsonObject
            {
                ["cpu"] = 0.25,
                ["memoryMB"] = 512
            },
            ["schedule"] = new JsonObject
            {
                ["timezone"] = "Asia/Kolkata",
                ["cron"] = "0 0 * * *"
            },
            ["retryPolicy"] = new JsonObject
            {
                ["maxAttempts"] = 3,
                ["backoffSeconds"] = 10
            },
            ["endpoints"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "http",
                    ["url"] = "https://api.example.com/v1"
                }
            },
            ["metadata"] = new JsonObject
            {
                ["createdBy"] = "system",
                ["labels"] = new JsonObject
                {
                    ["env"] = "prod",
                    ["tier"] = "backend"
                }
            },
            ["lastUpdated"] = DateTime.UtcNow.ToString("o") // ISO-8601
        };
    }

    private static (JsonObject? parent, string? key) TraverseToParent(string path, bool createMissing)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (null, null);

        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return (null, null);

        JsonObject current = _staticRootAccessor!();
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var p = parts[i];

            // If segment exists and is an object, descend
            if (current[p] is JsonObject nextObj)
            {
                current = nextObj;
                continue;
            }

            // If segment missing, create if requested
            if (createMissing)
            {
                var newObj = new JsonObject();
                current[p] = newObj;
                current = newObj;
            }
            else
            {
                return (null, null);
            }
        }
        return (current, parts.Last());
    }

    // Because TraverseToParent needs access to the current _root and we can't pass "this" into a static helper,
    // we bind a delegate at construction time.
    private Func<JsonObject> _staticRootAccessor => () => _root;

    private static bool HasProperty(JsonObject obj, string key) => obj.ContainsKey(key);

    private void CheckType(List<string> errors, string path, params JsonValueKind[] kinds)
    {
        var node = Get(path);
        if (node == null)
        {
            errors.Add($"Missing required property: {path}");
            return;
        }
        if (node is JsonValue val)
        {
            var kind = val.GetValueKindSafe();
            if (!kinds.Contains(kind))
                errors.Add($"Property '{path}' has wrong type: {kind}.");
        }
        else
        {
            // If we expected a primitive but got object/array
            if (kinds.Length == 1 && kinds[0] != JsonValueKind.Object && kinds[0] != JsonValueKind.Array)
                errors.Add($"Property '{path}' must be a primitive value.");
        }
    }

    private void CheckArray(List<string> errors, string path)
    {
        var node = Get(path);
        if (node == null)
        {
            errors.Add($"Missing required property: {path}");
            return;
        }
        if (node is not JsonArray)
            errors.Add($"Property '{path}' must be an array.");
    }

    private void CheckObject(List<string> errors, string path)
    {
        var node = Get(path);
        if (node == null)
        {
            errors.Add($"Missing required property: {path}");
            return;
        }
        if (node is not JsonObject)
            errors.Add($"Property '{path}' must be an object.");
    }

    private void RequireNested(List<string> errors, string path, JsonValueKind expected)
    {
        var node = Get(path);
        if (node == null)
        {
            errors.Add($"Missing required nested property: {path}");
            return;
        }
        if (node is JsonValue val)
        {
            var kind = val.GetValueKindSafe();
            if (kind != expected)
                errors.Add($"Nested property '{path}' has wrong type: {kind} (expected {expected}).");
        }
        else
        {
            errors.Add($"Nested property '{path}' must be a primitive.");
        }
    }

    private static void ShallowMerge(JsonObject target, JsonObject source)
    {
        foreach (var kvp in source)
        {
            target[kvp.Key] = kvp.Value?.DeepClone();
        }
    }

    private static void DeepMerge(JsonObject target, JsonObject source)
    {
        foreach (var kvp in source)
        {
            var key = kvp.Key;
            var srcNode = kvp.Value;

            if (srcNode is JsonObject srcObj && target[key] is JsonObject tgtObj)
            {
                DeepMerge(tgtObj, srcObj);
            }
            else
            {
                target[key] = srcNode?.DeepClone();
            }
        }
    }
}

// -----------------------------
// Extensions for JsonValueKind
// -----------------------------
static class JsonNodeExtensions
{
    public static JsonValueKind GetValueKindSafe(this JsonValue value)
    {
        try
        {
            using var doc = JsonDocument.Parse(value.ToJsonString());
            return doc.RootElement.ValueKind;
        }
        catch
        {
            return JsonValueKind.Undefined;
        }
    }

    public static JsonNode? DeepClone(this JsonNode node)
    {
        // Serialize+Parse for a simple deep clone
               return JsonNode.Parse(node.ToJsonString());
    }
