using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Shared.Models;

public record PostResponse(
    string ErrorCode = "",
    string ErrorMsg = "",
    string DocNum = "",
    string EDocNum = "",
    string DocEntry = "");
