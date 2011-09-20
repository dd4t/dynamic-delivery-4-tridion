using DD4T.Utilities;

namespace DD4T.Factories
{
    /// <summary>
    /// Base class for all factories
    /// </summary>
    public abstract class FactoryBase 
    {             
        /// <summary>
        /// Returns the current publicationId
        /// </summary>  
        protected virtual int PublicationId 
        {
            get
            {
                return TridionHelper.PublicationId;
            }
        }

    }
}
