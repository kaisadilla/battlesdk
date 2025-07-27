using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.graphics;
public readonly unsafe struct Ptr<T> where T : unmanaged {
    private readonly T* _ptr;

    public Ptr (T* ptr) {
        _ptr = ptr;
    }

    public Ptr (nint raw) {
        _ptr = (T*)raw;
    }

    public T* Raw => _ptr;
    public nint Nint => (nint)_ptr;
    public IntPtr IntPtr => (IntPtr)_ptr;

    public bool IsNull => _ptr == null;
    public ref T Ref => ref *_ptr;

    public override string ToString () {
        return ((nint)_ptr).ToString();
    }
}
