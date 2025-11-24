using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IVariableLogger
    {
        public void LogVariables(IEnumerable variables);
    }
}
