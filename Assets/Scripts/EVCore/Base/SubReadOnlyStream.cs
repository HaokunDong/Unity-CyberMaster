using System;
using System.IO;
using UnityEngine;

namespace Everlasting.Base
{
    public class SubReadOnlyStream: Stream
    {
        private readonly long m_offset;
        private readonly bool m_leaveOpen;

        protected long m_length;
        protected Stream m_actualStream;
        protected long m_position;

        public SubReadOnlyStream(Stream actualStream, long offset, long length, bool leaveOpen = false)
        {
            m_actualStream = actualStream;
            m_leaveOpen = leaveOpen;
            m_offset = offset;
            m_position = offset;
            m_length = length;
            actualStream.Seek(offset, SeekOrigin.Begin);
        }

        public override long Length
        {
            get
            {
                ThrowIfDisposed();
                return m_length;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return m_position - m_offset;
            }
            set
            {
                ThrowIfDisposed();
                throw new NotSupportedException();
            }
        }

        public override bool CanRead => m_actualStream.CanRead;

        public override bool CanSeek => m_actualStream.CanSeek;

        public override bool CanWrite => false;

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();

            var endPosition = m_offset + m_length;
            if (m_position + count > endPosition)
            {
                count = (int) (endPosition - m_position);
            }

            if (count <= 0)
            {
                return 0;
            }

            int bytesRead = m_actualStream.Read(buffer, offset, count);
            m_position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();

            if ( origin == SeekOrigin.Begin )
            {
                m_position = m_actualStream.Seek(m_offset + offset, SeekOrigin.Begin);
            }
            else if ( origin == SeekOrigin.End )
            {
                m_position = m_actualStream.Seek(m_offset + Length + offset, SeekOrigin.End);
            }
            else
            {
                m_position = m_actualStream.Seek(offset, SeekOrigin.Current);
            }
            return m_position - m_offset;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        // Close the stream for reading.  Note that this does NOT close the superStream (since
        // the substream is just 'a chunk' of the super-stream
        protected override void Dispose(bool disposing)
        {
            if ( disposing )
            {
                if (m_actualStream != null)
                {
                    if (!m_leaveOpen)
                        m_actualStream.Dispose();

                    m_actualStream = null;
                }
            }


            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (m_actualStream == null)
                throw new ObjectDisposedException(GetType().ToString(), "");
        }
    }
}