namespace EverlastingEditor.Config.Export
{
    public interface IConfigBinary
    {
        void Write(int i);
        void Write(uint i);
        void Write(long l);
        void Write(ulong l);
        void Write(bool b);
        void Write(float f);
	    void Write(string s);
		Pool GetPool(IPooledColumnType columnType);
	}
}
