namespace ProductCatalog.Api.Services;

/// <summary>
/// Configuration for a searchable field on an entity.
/// </summary>
public class SearchFieldConfig<T>
{
    public required Func<T, string> FieldSelector { get; init; }
    public required double Weight { get; init; }
    public required string FieldName { get; init; }
}

/// <summary>
/// Scored search result.
/// </summary>
public class SearchResult<T>
{
    public required T Item { get; init; }
    public double Score { get; init; }
}

/// <summary>
/// Generic in-memory search engine using ONLY core C# features.
/// Supports fuzzy matching (Levenshtein distance) and weighted multi-field scoring.
/// Generic over T so it can be reused for any entity type.
/// </summary>
public class SearchEngine<T>
{
    private readonly List<SearchFieldConfig<T>> _fieldConfigs = new();
    private readonly Dictionary<string, List<SearchResult<T>>> _cache = new();
    private readonly object _cacheLock = new();
    private readonly int _maxCacheSize;
    private List<T> _items = new();

    public SearchEngine(int maxCacheSize = 100)
    {
        _maxCacheSize = maxCacheSize;
    }

    /// <summary>
    /// Register a searchable field with its weight. Fluent API.
    /// </summary>
    public SearchEngine<T> AddField(string fieldName, Func<T, string> selector, double weight)
    {
        _fieldConfigs.Add(new SearchFieldConfig<T>
        {
            FieldName = fieldName,
            FieldSelector = selector,
            Weight = weight
        });
        return this;
    }

    /// <summary>
    /// Replace the searchable dataset. Invalidates cache.
    /// </summary>
    public void UpdateIndex(IEnumerable<T> items)
    {
        _items = items.ToList();
        ClearCache();
    }

    /// <summary>
    /// Search with combined exact, contains, prefix, word-level, and fuzzy matching.
    /// </summary>
    public List<SearchResult<T>> Search(string query, int maxResults = 50, double minScore = 0.1)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<SearchResult<T>>();

        var normalizedQuery = query.Trim().ToLowerInvariant();
        var cacheKey = $"{normalizedQuery}:{maxResults}:{minScore}";

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;
        }

        var results = new List<SearchResult<T>>();

        foreach (var item in _items)
        {
            double totalScore = 0;

            foreach (var config in _fieldConfigs)
            {
                var fieldValue = config.FieldSelector(item)?.ToLowerInvariant() ?? string.Empty;
                if (string.IsNullOrEmpty(fieldValue)) continue;

                double fieldScore = CalculateFieldScore(normalizedQuery, fieldValue);
                totalScore += fieldScore * config.Weight;
            }

            if (totalScore >= minScore)
            {
                results.Add(new SearchResult<T>
                {
                    Item = item,
                    Score = Math.Round(totalScore, 4)
                });
            }
        }

        var sortedResults = results
            .OrderByDescending(r => r.Score)
            .Take(maxResults)
            .ToList();

        // Cache with simple FIFO eviction
        lock (_cacheLock)
        {
            if (_cache.Count >= _maxCacheSize)
            {
                var keysToRemove = _cache.Keys.Take(_cache.Count / 2).ToList();
                foreach (var key in keysToRemove)
                    _cache.Remove(key);
            }
            _cache[cacheKey] = sortedResults;
        }

        return sortedResults;
    }

    /// <summary>
    /// Multi-strategy scoring for a query against a single field value.
    /// </summary>
    private static double CalculateFieldScore(string query, string fieldValue)
    {
        double score = 0;

        // 1. Exact match — highest possible score
        if (fieldValue == query)
            return 1.0;

        // 2. Contains match — scaled by coverage
        if (fieldValue.Contains(query))
        {
            double coverage = (double)query.Length / fieldValue.Length;
            score += 0.7 * coverage + 0.15;
        }

        // 3. Starts-with bonus
        if (fieldValue.StartsWith(query))
            score += 0.2;

        // 4. Word-level matching for multi-word queries
        var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var fieldWords = fieldValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (queryWords.Length > 1)
        {
            int matchedWords = queryWords.Count(qw =>
                fieldWords.Any(fw => fw.Contains(qw)));
            double wordMatchRatio = (double)matchedWords / queryWords.Length;
            score += 0.3 * wordMatchRatio;
        }

        // 5. Fuzzy matching — compare query against each word in the field
        double bestFuzzyScore = 0;
        foreach (var fieldWord in fieldWords)
        {
            int distance = LevenshteinDistance(query, fieldWord);
            int maxLen = Math.Max(query.Length, fieldWord.Length);
            if (maxLen == 0) continue;

            double similarity = 1.0 - ((double)distance / maxLen);

            // Only count if edit distance is within ~40% of query length
            if (distance <= Math.Max(2, query.Length * 0.4))
            {
                bestFuzzyScore = Math.Max(bestFuzzyScore, similarity * 0.5);
            }
        }
        score += bestFuzzyScore;

        return score;
    }

    /// <summary>
    /// Levenshtein distance — minimum edits (insert, delete, substitute) to
    /// transform source into target. Space-optimized to O(n) using two rows.
    /// This is what makes "lptop" match "laptop".
    /// </summary>
    public static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        int sourceLen = source.Length;
        int targetLen = target.Length;

        var previousRow = new int[targetLen + 1];
        var currentRow = new int[targetLen + 1];

        for (int j = 0; j <= targetLen; j++)
            previousRow[j] = j;

        for (int i = 1; i <= sourceLen; i++)
        {
            currentRow[0] = i;

            for (int j = 1; j <= targetLen; j++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                currentRow[j] = Math.Min(
                    Math.Min(
                        currentRow[j - 1] + 1,       // insertion
                        previousRow[j] + 1),          // deletion
                    previousRow[j - 1] + cost);       // substitution
            }

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[targetLen];
    }

    public void ClearCache()
    {
        lock (_cacheLock) { _cache.Clear(); }
    }
}

/// <summary>
/// Product-specific search engine with pre-configured field weights.
/// Registered as Singleton in DI to maintain index across requests.
/// </summary>
public class ProductSearchEngine
{
    private readonly SearchEngine<Models.Product> _engine;

    public ProductSearchEngine()
    {
        _engine = new SearchEngine<Models.Product>(maxCacheSize: 200);

        // Name is most relevant, SKU for exact lookups, Description for broad text
        _engine
            .AddField("Name", p => p.Name, weight: 3.0)
            .AddField("SKU", p => p.SKU, weight: 2.5)
            .AddField("Description", p => p.Description, weight: 1.0);
    }

    public void UpdateIndex(IEnumerable<Models.Product> products)
        => _engine.UpdateIndex(products);

    public List<SearchResult<Models.Product>> Search(string query, int maxResults = 50)
        => _engine.Search(query, maxResults);

    public void ClearCache() => _engine.ClearCache();

    public static int ComputeLevenshteinDistance(string a, string b)
        => SearchEngine<Models.Product>.LevenshteinDistance(a, b);
}