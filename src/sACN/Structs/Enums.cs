using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sACN.Structs
{
    public enum FramingOptionsType
    {
        E131_OPT_TERMINATED = 6,
        E131_OPT_PREVIEW = 7,

    }

    public enum ErrorType
    {
        E131_ERR_NONE,
        E131_ERR_NULLPTR,
        E131_ERR_PREAMBLE_SIZE,
        E131_ERR_POSTAMBLE_SIZE,
        E131_ERR_ACN_PID,
        E131_ERR_VECTOR_ROOT,
        E131_ERR_VECTOR_FRAME,
        E131_ERR_VECTOR_DMP,
        E131_ERR_TYPE_DMP,
        E131_ERR_FIRST_ADDR_DMP,
        E131_ERR_ADDR_INC_DMP,

    }
}
