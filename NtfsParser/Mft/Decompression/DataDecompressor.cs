namespace NtfsParser.Mft.Decompression;

public static class DataDecompressor
{
    public static byte[] Decompress(CompressedData data, int decompressionBufferSize)
    {
        var compressedSpan = data.Data.AsSpan();
        var decompressionBuffer = new DecompressionBuffer(decompressionBufferSize);
        foreach (var unit in data.CompressionUnits)
        {
            ProcessUnit(decompressionBuffer, compressedSpan, unit, data.CompressionUnitSizeCluster, data.ClusterSizeByte);
        }
        
        return decompressionBuffer.Buffer;
    }

    private static void ProcessUnit(DecompressionBuffer decompressionBuffer, Span<byte> data, CompressionUnit unit, 
        int compressionUnitSizeCluster, int clusterSizeByte)
    {
        switch (unit.Type)
        {
            case UnitType.Uncompressed:
                decompressionBuffer.InsertRange(data[unit.Range]);
                break;
            case UnitType.Sparse:
                decompressionBuffer.InsertSparse(compressionUnitSizeCluster * clusterSizeByte);
                break;
            case UnitType.Compressed:
                DecompressUnit(data[unit.Range], decompressionBuffer, clusterSizeByte);
                break;
        }

        decompressionBuffer.CloseCurrentChunk();
    }
    
    private static void DecompressUnit(Span<byte> compressedSpan, DecompressionBuffer decompressionBuffer, int clusterSizeByte)
    {
        var pointer = 0;
        while (pointer < compressedSpan.Length - 2)
        {
            var rawChunkHeader = compressedSpan.Slice(pointer, 2);
            var chunkHeader = new CompressionChunkHeader(rawChunkHeader);
            if (chunkHeader.ChunkSize == 0)
            {
                pointer += clusterSizeByte - pointer % clusterSizeByte; // move pointer to the start of the next chunk
                continue;
            }
            
            pointer += 2;
            var chunk = compressedSpan.Slice(pointer, chunkHeader.ChunkSize);
            if (chunkHeader.IsCompressed)
                DecompressChunk(decompressionBuffer, chunk);
            else
                decompressionBuffer.InsertRange(chunk);
            
            decompressionBuffer.CloseCurrentChunk();
            pointer += chunkHeader.ChunkSize;
        }
    }
    
    private static void DecompressChunk(DecompressionBuffer buffer, Span<byte> compressedChunk)
    {
        var pointer = 0;
        while (pointer < compressedChunk.Length)
        {
            var flags = compressedChunk[pointer++];
            if (flags == 0) // group doesn't contain any backreferences
            {
                var diff = compressedChunk.Length - pointer;
                var length = diff <= 8 ? diff : 8;
                var uncompressedGroup = compressedChunk.Slice(pointer, length);
                buffer.InsertRange(uncompressedGroup);
                pointer += length;
                continue;
            }

            var bitMask = 1;
            for (int i = 0; i < 8 && pointer < compressedChunk.Length; ++i)
            {
                if ((flags & bitMask) == 0)
                {
                    buffer.InsertByte(compressedChunk[pointer++]);
                }
                else
                {
                    var rawBackreference = compressedChunk.Slice(pointer, 2);
                    var backreference = new Backreference(rawBackreference, buffer.BlockPointer);
                    pointer += 2;
                    buffer.InsertFromBackreference(backreference);
                }

                bitMask <<= 1;
            }
        }
    }
    
    /// <summary>
    /// A decompression buffer
    /// </summary>
    private class DecompressionBuffer
    {
        public DecompressionBuffer(int bufferSize) => Buffer = new byte[bufferSize];
        
        public byte[] Buffer { get; init; }
        /// <summary>
        /// Index of the current byte in block. Set to zero when CloseCurrentBlock() is called.
        /// </summary>
        public int BlockPointer { get; private set; }
        /// <summary>
        /// Index of the current byte in the buffer.
        /// </summary>
        public int InsertPointer { get; private set; }

        /// <summary>
        /// Insert a byte at current InsertPointer position
        /// </summary>
        public void InsertByte(byte value)
        {
            if (InsertPointer + 1 > Buffer.Length)
                throw new ArgumentException("The destination buffer is too small.");
            
            Buffer[InsertPointer++] = value;
            BlockPointer += 1;
        }

        public void InsertSparse(int sizeByte)
        {
            if (InsertPointer + sizeByte > Buffer.Length)
                throw new ArgumentException("The destination buffer is too small.");
            
            InsertPointer += sizeByte;
            BlockPointer += sizeByte;
        }
        
        /// <summary>
        /// Insert bytes at current InsertPointer position
        /// </summary>
        public void InsertRange(Span<byte> range)
        {
            if (InsertPointer + range.Length > Buffer.Length)
                throw new ArgumentException("The destination buffer is too small.");
            
            range.CopyToAt(Buffer, InsertPointer);
            InsertPointer += range.Length;
            BlockPointer += range.Length;
        }
    
        /// <summary>
        /// Copy previously inserted bytes using a backreference
        /// </summary>
        public void InsertFromBackreference(Backreference backreference)
        {
            var span = Buffer.AsSpan();
            if (backreference.Size <= backreference.Offset)
            {
                if (InsertPointer + backreference.Size > Buffer.Length)
                    throw new ArgumentException("The destination buffer is too small.");
                
                var data = span.Slice(InsertPointer - backreference.Offset, backreference.Size);
                data.CopyToAt(span, InsertPointer);
                InsertPointer += data.Length;
            }
            else
            {
                if (InsertPointer + (backreference.Size - backreference.Offset) > Buffer.Length)
                    throw new ArgumentException("The destination buffer is too small.");
                
                for (int i = 0; i < backreference.Size; ++i)
                {
                    span[InsertPointer] = span[InsertPointer - backreference.Offset];
                    ++InsertPointer;
                }
            }
            BlockPointer += backreference.Size;
        }
        
        /// <summary>
        /// Close current chunk. Should be called whenever a compression chunk is fully decompressed
        /// </summary>
        public void CloseCurrentChunk()
        {
            BlockPointer = 0;
        }
    }
}