using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ICallStack
    {
        public void Push(string functionName);
        public void Pop();
    }
}
