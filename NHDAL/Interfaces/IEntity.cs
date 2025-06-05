using System;

namespace NHDAL.Interfaces
{
    /// <summary>
    /// Represents a generic entity with a unique identifier and a timestamp version.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the unique identifier for the entity. 
    /// Must be a value type (struct), such as Guid, int, long, etc.
    /// </typeparam>
    public interface IEntity<TKey> where TKey : struct
    {
        /// <summary>
        /// Gets the unique identifier of the entity.
        /// This property is read-only to ensure the identity remains immutable.
        /// </summary>
        TKey Id { get; }

        /// <summary>
        /// Gets the timestamp indicating when the entity was created or last updated.
        /// Acts as version in optimistic locking, handled by NH
        /// </summary>
        DateTime Timestamp { get; }
    }
}