namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    #region Usings
    using System;
    using System.Collections.Generic;
    #endregion Usings

    public interface IField 
    {
        string CategoryId { get; }
        string CategoryName { get; }
        IList<DateTime> DateTimeValues { get; }
        IList<IDictionary<string, IField>> EmbeddedValues { get; }
        FieldType FieldType { get; }
        IList<IComponent> LinkedComponentValues { get; }
        string Name { get; }
        IList<double> NumericValues { get; }
        IList<string> Values { get; }
        string Value { get; }
    }
}
