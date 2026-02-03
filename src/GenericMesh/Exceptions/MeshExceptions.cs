namespace GenericMesh.Exceptions;

/// <summary>
/// Exception thrown when attempting to modify a locked mesh.
/// </summary>
public class MeshLockedException : InvalidOperationException
{
    public MeshLockedException()
        : base("The mesh is locked and cannot be modified.") { }

    public MeshLockedException(string message)
        : base(message) { }
}

/// <summary>
/// Exception thrown when attempting to access an unlocked mesh that should be locked.
/// </summary>
public class MeshNotLockedException : InvalidOperationException
{
    public MeshNotLockedException()
        : base("The mesh is not locked.") { }

    public MeshNotLockedException(string message)
        : base(message) { }
}

/// <summary>
/// Exception thrown for invalid primitive type operations.
/// </summary>
public class MeshPrimitiveTypeException : InvalidOperationException
{
    public MeshPrimitiveTypeException()
        : base("Invalid primitive type for this operation.") { }

    public MeshPrimitiveTypeException(string message)
        : base(message) { }
}
