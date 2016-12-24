using System;
using System.IO;

namespace FileSignature
{
	internal delegate void BlockHandler(Block block);

	internal class AdvancedStreamReader
	{
		private readonly Stream _input;
		private readonly int _blockSize;
		private readonly SemaphoreSlim _readLimimiter;

		public AdvancedStreamReader(Stream input, int blockSize, SemaphoreSlim readLimimiter)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));
			if (blockSize <= 0) throw new ArgumentOutOfRangeException(nameof(blockSize));
			if (readLimimiter == null) throw new ArgumentNullException(nameof(readLimimiter));

			_input = input;
			_blockSize = blockSize;
			_readLimimiter = readLimimiter;
		}

		public event BlockHandler BlockReaded;
		public event Action EndOfStream;

		public void ProcessRead()
		{
			int blockNumber = 0;
			while (true)
			{
				_readLimimiter.WaitOne();
				var buffer = new byte[_blockSize];
				int size = _input.Read(buffer, 0, _blockSize);
				if (size == 0)
				{
					_readLimimiter.Release();
					EndOfStream?.Invoke();
					return;
				}
				var block = new Block(blockNumber++, buffer, size);
				BlockReaded?.Invoke(block);
			}
		}
	}
}
