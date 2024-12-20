namespace CS8_MessageAPI.Models;
/// <summary>
/// An academic Block
/// </summary>
/// <param name="blockId"></param>
/// <param name="name">Name of the Block</param>
/// <param name="schoolLevel">Upper or Lower SChool</param>
public record Block (
    string blockId,
    string name,
    string schoolLevel);

/// <summary>
/// Time period that you're free
/// </summary>
/// <param name="block">Block Information</param>
/// <param name="start">When the Free Block Starts</param>
/// <param name="end">When the Free blocl endds</param>
public record FreeBlock(
    
   
    Block block,
    DateTime start,
    DateTime end);

/// <summary>
/// Collection of FreeBlocks
/// Response to a request for free-blocks in a given range dates
/// </summary>
/// <param name="FreeBlock"></param>
/// <param name="inRange"></param>
public record FreeBlockCollection(
    FreeBlock[] FreeBlock,
    DateOnly[] inRange);
    
    