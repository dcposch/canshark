using System;

namespace SSCP.Telem.Can {
    //Very simple sender interface.
    public interface Sink<T> {
        void Send(T message);
    }
}
