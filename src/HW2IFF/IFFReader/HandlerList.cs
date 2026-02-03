namespace HW2IFF;

/// <summary>
/// List of handlers for reading an IFF file.
/// </summary>
internal class HandlerList
{
    private readonly IFFReader _iffReader;
    private readonly List<HandlerNode> _handlers;

    /// <summary>The handler to use when no other handler is available.</summary>
    public ChunkHandler? DefaultHandler;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="iffReader">IFF Reader this instance will be associated with.</param>
    public HandlerList(IFFReader iffReader)
    {
        _iffReader = iffReader;
        _handlers = new List<HandlerNode>();
    }

    /// <summary>
    /// Adds a handler for the specified chunk.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="handler">Handler to set.</param>
    /// <param name="version">Version of the chunk.</param>
    /// <remarks>
    /// This removes any existing handler for the same ID, Type, and Version combination.
    /// </remarks>
    public void AddHandler(string id, ChunkType type, ChunkHandler handler, uint version = 0)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var hn = MakeHandlerNode(id, type, version);

        if (string.IsNullOrEmpty(hn.ID))
            return;

        // Try to find an existing handler. If present, then remove it.
        if (FindHandler(id, type, version) != null)
            _handlers.RemoveAll(h => CompareHandlers(h, hn));

        hn.Handler = handler;
        _handlers.Add(hn);
    }

    /// <summary>
    /// Finds a handler for the specified type of chunk.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="version">Version of the chunk.</param>
    /// <returns>The handler if set, or null.</returns>
    public ChunkHandler? FindHandler(string id, ChunkType type, uint version = 0)
    {
        if (_handlers.Count == 0)
            return DefaultHandler;

        var hn = MakeHandlerNode(id, type, version);

        if (string.IsNullOrEmpty(hn.ID))
            return null;

        var found = _handlers.Find(h => CompareHandlers(h, hn));
        return found.Handler ?? DefaultHandler;
    }

    /// <summary>
    /// Compares two handler nodes.
    /// </summary>
    private static bool CompareHandlers(HandlerNode h1, HandlerNode h2)
    {
        return h1.ID == h2.ID &&
               h1.Type == h2.Type &&
               (h1.Type != ChunkType.Normal || h1.Version == h2.Version);
    }

    /// <summary>
    /// Verifies and makes a handler node with the given information.
    /// </summary>
    private static HandlerNode MakeHandlerNode(string id, ChunkType type, uint version = 0)
    {
        // Validate by creating ChunkAttributes (will throw on invalid input)
        _ = new ChunkAttributes(id, 0, type, version);

        return new HandlerNode
        {
            ID = id,
            Type = type,
            Version = version
        };
    }
}
