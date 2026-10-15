using System;

namespace ESCPOS_NET
{
    public interface IPrinter
    {
        public PrinterStatusEventArgs GetStatus();
        public void Write(params byte[][] arrays);
        public void Write(byte[] bytes);
    }
}
