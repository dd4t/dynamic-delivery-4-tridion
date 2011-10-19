using DD4T.Utilities;

namespace DD4T.Factories
{
    /// <summary>
    /// Base class for all factories
    /// </summary>
    public abstract class FactoryBase 
    {

        private int? _publicationId = null;

        /// <summary>
        /// Returns the current publicationId
        /// </summary>  
        protected virtual int PublicationId 
        {
            get
            {
                if (_publicationId == null)
                    _publicationId = TridionHelper.PublicationId;
                return (int)_publicationId;
            }
            set
            {
                _publicationId = value;
            }
        }

    }
}
