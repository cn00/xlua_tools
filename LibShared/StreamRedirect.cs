using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StreamRedirect
{
    class StreamRedirect: StreamWriter
    {
        List<TextWriter> mList = new List<TextWriter>(){  };


        public override bool AutoFlush {
            get
            {
                return base.AutoFlush;
            }
            set
            {
                base.AutoFlush = value;
                foreach(var i in mList)
                {
                    if(i is StreamWriter)
                        (i as StreamWriter).AutoFlush = value;
                }
            }
        }

        ~StreamRedirect()
        {
            Flush();
            Close();
        }

        public StreamRedirect(string path):base(path)
        {
        }
        public StreamRedirect(Stream stream) : base(stream)
        {
        }

        public StreamRedirect Add(string path)
        {
            Add(new StreamWriter(path));
            return this;
        }

        public StreamRedirect Add(Stream stream)
        {
            Add(new StreamWriter(stream));
            return this;
        }

        public StreamRedirect Add(TextWriter i)
        {
            mList.Add(i);
            if(i is StreamWriter)
                (i as StreamWriter).AutoFlush = AutoFlush;
            return this;
        }

        public StreamRedirect Remove(TextWriter i)
        {
            mList.Remove(i);
            return this;
        }

        #region write value
        public void Write<T>(T v)
        {
            base.Write("{0}", v);// -> Write(string fmt, params object[] args) -> Write(string v)
        }

        public override void Write(string v)
        {
            base.Write(v);
            foreach(var i in mList)
                i.Write(v);
        }
        public override void Write(bool value)
        {
            Write(value);
        }
        public override void Write(char value)
        {
            Write(value);
        }
        public override void Write(char[] buffer)
        {
            Write(buffer);
        }
        public override void Write(double value)
        {
            Write(value);
        }
        public override void Write(float value)
        {
            Write(value);
        }
        public override void Write(int value)
        {
            Write(value);
        }
        public override void Write(long value)
        {
            Write(value);
        }
        public override void Write(uint value)
        {
            Write(value);
        }
        public override void Write(ulong value)
        {
            Write(value);
        }
        public override void Write(string format, params object[] arg)
        {
            base.Write(format, arg);
            foreach(var i in mList)
                i.Write(format, arg);
        }
#endregion write value

#region write line value
        public void WriteLine<T>(T value)
        {
            Write("\n");
            Write(value);
        }

        public override void WriteLine(string value)
        {
            WriteLine(value);
        }
        public override void WriteLine(bool value)
        {
            WriteLine(value);
        }
        public override void WriteLine(char value)
        {
            WriteLine(value);
        }
        public override void WriteLine(double value)
        {
            WriteLine(value);
        }
        public override void WriteLine(float value)
        {
            WriteLine(value);
        }
        public override void WriteLine(int value)
        {
            WriteLine(value);
        }
        public override void WriteLine(long value)
        {
            WriteLine(value);
        }
        public override void WriteLine(uint value)
        {
            WriteLine(value);
        }

        public override void WriteLine(string s, params object[] arg)
        {
            Write("\n" + s, arg);
        }
#endregion write line value

        public override void Flush()
        {
            base.Flush();
            foreach(var i in mList)
                i.Flush();
        }
        public override void Close()
        {
            base.Close();
            foreach(var i in mList)
                i.Close();
        }
        public static StreamRedirect test(string path)
        {
            var instance = new StreamRedirect(path);
            var oldout = Console.Out;
            instance.Add(Console.Out);

            return null;
        }
    }
}
