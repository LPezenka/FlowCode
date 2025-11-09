using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IInputHandler
    {
        public string ReadInput(string prompt = "");
    }
}
