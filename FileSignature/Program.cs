using System;
using System.IO;

namespace FileSignature
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			Console.WindowWidth = 120;
			string fileName;
			int blockSize;
			ExtractArguments(args, out fileName, out blockSize);

			using (var fs = new FileStream(fileName, FileMode.Open))
			{
				int processors = Environment.ProcessorCount;
				var syncObject = new SemaphoreSlim(processors * 2, processors * 2);
				var reader = new AdvancedStreamReader(fs, blockSize, syncObject);
				var bufferedWriter = new BufferedTextWriter(Console.Out, processors);
				var blocksHandler = new BlocksHandler(syncObject, bufferedWriter);
				reader.BlockReaded += blocksHandler.HandleBlockAsync;
				reader.EndOfStream += blocksHandler.EndOfBlocks;
				reader.ProcessRead();
				blocksHandler.WorkDoneAwaiter.WaitOne();
			}
		}

		static void ExtractArguments(string[] args, out string fileName, out int blockSize)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));
			if (args.Length != 2) throw new ArgumentException(nameof(args));

			fileName = args[0];
			blockSize = int.Parse(args[1]);
			if (blockSize <= 0)
				throw new ArgumentException(nameof(blockSize));
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Log((Exception)(e.ExceptionObject));
			Environment.Exit(1);
		}
	}
}