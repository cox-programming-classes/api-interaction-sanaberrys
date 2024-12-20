namespace CS8_MessageAPI.Models;

public record CycleDay (
    String Date,
    String cycleDay);

public record Teacher(
    String Id,
    String firstName,
    String lastName,
    String email); 
 
public record Students (
    String Id,
    String displayName,
    String email,
    String gradYear,
    String className,
    String advisorName);

public record Term(
    String termId,
    String room,
    String block,
    String blockId,
    String displayName,
    String schoolLevel,
    String isCurrent);

public record Section(
    String SectionId,
    String CourseId,
    String primaryTeacherId,
    Teacher[] teachers,
    Students[] studens,
    String termId);
    
    