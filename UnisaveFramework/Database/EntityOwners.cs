namespace Unisave.Database
{
    /// <summary>
    /// Extension of EntityOwnerIds that can automatically
    /// pull missing data from the database and also works
    /// with UnisavePlayer instances instead of IDs.
    /// </summary>
    public class EntityOwners
    {
        // TODO finish this class
        
        /// <summary>
        /// The underlying owner IDs instance
        /// </summary>
        public EntityOwnerIds OwnerIds { get; }
        
        public EntityOwners()
        {
            OwnerIds = new EntityOwnerIds();
        }
        
        public EntityOwners(EntityOwnerIds ownerIds)
        {
            OwnerIds = ownerIds;
        }
        
        /// <summary>
        /// Add new owner of the entity
        /// </summary>
        public void Add(UnisavePlayer player)
        {
            
        }

        /// <summary>
        /// Remove an owner of the entity
        /// </summary>
        public void Remove(UnisavePlayer player)
        {
            
        }
    }
}