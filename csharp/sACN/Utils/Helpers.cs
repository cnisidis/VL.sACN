using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sACN.Utils;

namespace sACN.Utils
{
    public static class Helpers
    {
        public static ushort CalculatePreambleSize(ushort txLength, ushort preambleConfig)
        {
            // A TX_LENGTH of 0 results in skipping the preamble.
            if (txLength == 0)
            {
                return 0;
            }

            ushort calculatedSize;

            // Check the LENGTH_CONFIG bit (bit 0)
            bool isBytes = (preambleConfig & Constants.LENGTH_CONFIG_MASK) == 1;

            if (isBytes)
            {
                // LENGTH_CONFIG = 1, TX_LENGTH is in bytes
                calculatedSize = txLength;
            }
            else
            {
                // LENGTH_CONFIG = 0, TX_LENGTH is in nibbles
                // 2 nibbles per byte, so divide by 2
                calculatedSize = (ushort)(txLength / 2);
            }

            // Clamp the result to the maximum allowed length
            if (calculatedSize > Constants.MAX_PREAMBLE_BYTES)
            {
                return Constants.MAX_PREAMBLE_BYTES;
            }

            return calculatedSize;
        }
    }
}
