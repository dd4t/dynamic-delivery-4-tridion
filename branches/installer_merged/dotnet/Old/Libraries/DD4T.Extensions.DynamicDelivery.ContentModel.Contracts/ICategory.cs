namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    #region Usings
    using System.Collections.Generic;
    #endregion Usings

    public interface ICategory : IRepositoryLocal
    {
        IList<IKeyword> Keywords { get; }
    }
}
