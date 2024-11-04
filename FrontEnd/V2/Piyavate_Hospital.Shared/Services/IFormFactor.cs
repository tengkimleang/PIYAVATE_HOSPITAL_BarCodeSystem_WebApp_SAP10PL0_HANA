using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Shared.Services
{
    public interface IFormFactor
    {
        public string GetFormFactor();
        public string GetPlatform();
    }
}
