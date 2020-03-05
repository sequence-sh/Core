using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Processes
{
    internal class ErrorList : IEnumerable<string>
    {
        private readonly ICollection<string> _list;


        public ErrorList()
        {
            _list = new List<string>();
        }

        public ErrorList(ICollection<string> list)
        {
            _list = list;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        public void Add(string s)
        {
            _list.Add(s);
        }

        public override string ToString()
        {
            if (!_list.Any())
                return "Empty";

            return string.Join("\r\n", _list);
        }
    }
}