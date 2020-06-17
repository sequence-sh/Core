using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Utilities.Processes
{
    ///// <summary>
    ///// A strings of different error messages.
    ///// </summary>
    //public class ErrorList : IEnumerable<string>
    //{
    //    private readonly ICollection<string> _list;

    //    /// <summary>
    //    /// Create a new ErrorList.
    //    /// </summary>
    //    public ErrorList()
    //    {
    //        _list = new List<string>();
    //    }

    //    /// <summary>
    //    /// Create a new ErrorList.
    //    /// </summary>
    //    /// <param name="list"></param>
    //    public ErrorList(ICollection<string> list)
    //    {
    //        _list = list;
    //    }
    //    /// <summary>
    //    /// Create a new ErrorList.
    //    /// </summary>
    //    /// <param name="strings"></param>
    //    public ErrorList(params string[] strings)
    //    {
    //        _list = strings;
    //    }

    //    /// <inheritdoc />
    //    public IEnumerator<string> GetEnumerator()
    //    {
    //        return _list.GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return ((IEnumerable) _list).GetEnumerator();
    //    }

    //    /// <summary>
    //    /// Add a new error.
    //    /// </summary>
    //    /// <param name="s"></param>
    //    public void Add(string s)
    //    {
    //        _list.Add(s);
    //    }

    //    /// <summary>
    //    /// Add multiple new errors.
    //    /// </summary>
    //    /// <param name="errors"></param>
    //    public void AddRange(IEnumerable<string> errors)
    //    {
    //        foreach (var e in errors) _list.Add(e);
    //    }

    //    /// <inheritdoc />
    //    public override string ToString()
    //    {
    //        if (!_list.Any())
    //            return "Empty";

    //        return string.Join("\r\n", _list);
    //    }

    //    /// <summary>
    //    /// Combine several error lists.
    //    /// </summary>
    //    /// <param name="lists"></param>
    //    /// <returns></returns>
    //    public static ErrorList Compose(IEnumerable<ErrorList> lists)
    //    {
    //        return new ErrorList(lists.SelectMany(x=>x).ToList());
    //    }
    //}
}