using System;
using System.Collections.Generic;
using System.Text;

namespace BencodeNET.Torrents.Validation
{
    class ValidationData
    {
        public bool isValid;
        public int piecesValidated;
        public long remainder;
        public byte[] buffer;
        public bool validateRemainder;

        public ValidationData(long bufferSize, bool validateReminder)
        {
            piecesValidated = 0;
            isValid = false;
            remainder = 0;
            buffer = new byte[bufferSize];
            this.validateRemainder = validateReminder;
        }
    }
}
