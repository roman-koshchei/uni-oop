using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6;

public class CalculateState
{
    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    private long totalSize = 0;
    public long TotalSize => Volatile.Read(ref totalSize);

    public void AddToTotalSize(long count)
    {
        Interlocked.Add(ref totalSize, count);
    }

    private int processedFilesCount = 0;
    public int ProcessedFilesCount => Volatile.Read(ref processedFilesCount);

    public void IncrementProcessedFilesCount()
    {
        Interlocked.Increment(ref processedFilesCount);
    }

    private int processedFoldersCount = 0;
    public int ProcessedFoldersCount => Volatile.Read(ref processedFoldersCount);

    public void IncrementProcessedFoldersCount()
    {
        Interlocked.Increment(ref processedFoldersCount);
    }
}