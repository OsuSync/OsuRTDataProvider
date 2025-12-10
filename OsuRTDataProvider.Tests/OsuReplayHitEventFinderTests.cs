using OsuRTDataProvider.Memory;
using OsuRTDataProvider.Tests.Mocks;
using Xunit;

namespace OsuRTDataProvider.Tests;

public class OsuReplayHitEventFinderTests
{
    [Fact]
    public void TryInit_ShouldFindReplayPattern()
    {
        // Arrange
        // Pattern 0: D9 5D C0 EB 4E A1 ?? ?? ?? ?? 8B 48 34 4E
        // Offset: 6 (points to first ??)

        byte[] patternBytes = new byte[]
        {
            0xD9, 0x5D, 0xC0, 0xEB, 0x4E, 0xA1,
            0xAA, 0xBB, 0xCC, 0xDD, // The target address location
            0x8B, 0x48, 0x34, 0x4E
        };

        int memorySize = 0x2000;
        byte[] memory = new byte[memorySize];
        int patternOffset = 0x200;

        Array.Copy(patternBytes, 0, memory, patternOffset, patternBytes.Length);

        // The logic:
        // 1. Find pattern -> address of 'AA' (patternOffset + 6)
        // 2. Read IntPtr at that address -> 0xDDCCBBAA
        // 3. Check if not zero.

        var mockSigScan = new MockSigScan(memory);
        var finder = new OsuReplayHitEventFinder(mockSigScan);

        // Act
        bool result = finder.TryInit();

        // Assert
        Assert.True(result, "TryInit should return true when replay pattern is found");
    }
}