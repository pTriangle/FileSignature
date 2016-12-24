using System;
using System.IO;
using System.Threading;

namespace FileSignature
{
	internal class BufferedTextWriter
	{
		private readonly TextWriter _stream;
		private int _nextBlockNumber;
		private readonly string[] _buffer;
		private readonly object _locker;

		public BufferedTextWriter(TextWriter stream, int bufferSize)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (bufferSize < 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

			_stream = stream;
			_buffer = new string[bufferSize];
			_locker = new object();
		}

		/// <remarks>
		/// Усыпляет вызывающий поток, если значение blockNumber слишком большое
		/// </remarks>
		public void Write(int blockNumber, string blockValue)
		{
			if (blockNumber < _nextBlockNumber) throw new ArgumentOutOfRangeException(nameof(blockNumber));
			if (blockValue == null) throw new ArgumentNullException(nameof(blockValue));

			lock (_locker)
			{
				int maxBlockNumber = _nextBlockNumber + _buffer.Length;
				while (blockNumber > maxBlockNumber || blockNumber == maxBlockNumber && _buffer.Length != 0)
				{
					Monitor.Wait(_locker);
					maxBlockNumber = _nextBlockNumber + _buffer.Length;
				}

				if (_buffer.Length == 0)
				{
					_stream.Write(blockValue);
					return;
				}

				_buffer[blockNumber - _nextBlockNumber] = blockValue;
				if (blockNumber == _nextBlockNumber)
				{
					int shift = CalculateShift();
					WriteBlocks(shift);
					ShiftBlocks(shift);
				}
			}
		}

		private void ShiftBlocks(int shift)
		{
			for (int i = 0; i < _buffer.Length; ++i)
			{
				if (i >= shift)
					_buffer[i - shift] = _buffer[i];
				_buffer[i] = null;
			}
			_nextBlockNumber += shift;
			Monitor.PulseAll(_locker);
		}

		private void WriteBlocks(int shift)
		{
			for (int i = 0; i < shift; ++i)
				_stream.Write(_buffer[i]);
		}

		private int CalculateShift()
		{
			int shift = 0;
			while (shift < _buffer.Length && _buffer[shift] != null)
				++shift;
			return shift;
		}
	}
}
