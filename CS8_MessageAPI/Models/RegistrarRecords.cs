using System.Collections.Immutable;
using System.Diagnostics;
using CS8_MessageAPI.Services;

namespace CS8_MessageAPI.Models;


/// <summary>
/// An entry in your schedule of any kind.
/// </summary>
/// <param name="id">Id of this entity</param>
/// <param name="name">display name</param>
/// <param name="type">what kind of thing is this? [academic, advisory, activity]</param>
/// <param name="block">what block does this meet in (if applicable)</param>
/// <param name="room">what room does it meet in</param>
public readonly record struct ScheduleEntry(string id, string name, string type, string block, string room);

/// <summary>
/// details only relevant to students.
/// </summary>
/// <param name="gradYear">What year do they graduate</param>
/// <param name="className">What is their current class</param>
/// <param name="advisor">Who is their advisor (full contact details)</param>
public readonly record struct StudentInfoRecord(int gradYear, string className, AdvisorRecord? advisor = null);

public readonly record struct AdvisorRecord(string id, int blackbaudId, string firstName, string nickname, string lastName, string email)
{
    public override string ToString() => string.IsNullOrWhiteSpace(nickname) || nickname == firstName ?
            $"{firstName} {lastName}" : $"{nickname} {lastName}";

    public static implicit operator AdvisorRecord(UserRecord user) =>
        new(user.id, user.blackbaudId, user.firstName, user.nickname, user.lastName, user.email);

    public static implicit operator UserRecord(AdvisorRecord user) =>
        new(user.id, user.blackbaudId, user.firstName, user.nickname, user.lastName, user.email);
}

/// <summary>
/// User Details useful to your app
/// </summary>
/// <param name="blackbaudId">this person's Blackbaud UserId</param>
/// <param name="firstName">Given Name</param>
/// <param name="nickname">Chosen Prefered Name (from WILD)</param>
/// <param name="lastName">Family Name</param>
/// <param name="email">Email address used for logging in and communication.</param>
/// <param name="studentInfo"></param>
public readonly record struct UserRecord(string id = "", int blackbaudId = -1, string firstName = "", 
    string nickname = "", string lastName = "", string email = "",
    StudentInfoRecord? studentInfo = null, bool hasLogin = false, DocumentHeader? photo = null)
{
    public static readonly UserRecord Empty = new();
    internal readonly bool requiresYearDistinction { get; } = false;
    internal readonly bool requiresNameDistinction { get; } = false;
    public override string ToString()
    {
        var name = string.IsNullOrWhiteSpace(nickname) || nickname == firstName ?
            $"{firstName} {lastName}" : $"{nickname} {lastName}";

        if (studentInfo is not null)
        {
            name += $" [{studentInfo.Value.className}]";
        }
        return name;
    }

    public async Task<ImmutableArray<string>> GetRoles(ApiService api) => await api.SendAsync<ImmutableArray<string>>(HttpMethod.Get, "api/users/self/roles",
        err =>
        {
            Debug.WriteLine($"{err}");
        });
}
public readonly record struct StudentRecordShort(string id, string displayName, string email, int gradYear, string className, string advisorName)
{
    public static implicit operator StudentRecordShort(UserRecord student) =>
        student.studentInfo is null ? 
        new StudentRecordShort(student.id, $"{student.firstName} {student.lastName}", student.email, 0, "N/A", "N/A"):
        new StudentRecordShort(student.id,
            student.requiresNameDistinction ?
                student.firstName == student.nickname ?
                    $"{student.firstName} {student.lastName}" :
                    $"{student.nickname} ({student.firstName}) {student.lastName}" :
            student.requiresYearDistinction ?
                $"{student.nickname} {student.lastName} [{student.studentInfo?.className}]" :
                $"{student.nickname} {student.lastName}",
            student.email,
            student.studentInfo?.gradYear ?? 0, student.studentInfo?.className ?? "", student.studentInfo?.advisor is null ? "" :
            $"{student.studentInfo?.advisor?.firstName} {student.studentInfo?.advisor?.lastName}");

    public override string ToString() => $"{displayName} [{className}]";
}

/// <summary>
/// A school term
/// </summary>
/// <param name="termId">Term Id used for scheduling</param>
/// <param name="schoolYear">School Year of the term</param>
/// <param name="name">Display name (First Semester, Second Semester, etc.)</param>
/// <param name="start">term begin date</param>
/// <param name="end">term end date</param>
public readonly record struct TermRecord(string termId, string schoolYear, string name, DateTime start, DateTime end)
{
    public bool IsCurrent() => start <= DateTime.Today && end >= DateTime.Today;
}

/// <summary>
/// Course details used for scheduling
/// </summary>
/// <param name="courseId">Course Id (used for creating new assessments)</param>
/// <param name="courseCode">Course Code from WILD</param>
/// <param name="displayName">Course Name</param>
/// <param name="lengthInTerms">1 semester, or full year</param>
/// <param name="department">What department is this course listed in.</param>
public readonly record struct CourseRecord(string courseId, string courseCode, string displayName, int lengthInTerms, string department)
{
    public static readonly CourseRecord Empty = new("", "", "", 0, "");
    public override string ToString() => this.displayName;
}

/// <summary>
/// Detailed Record of a section with expanded results
/// </summary>
/// <param name="sectionId">Section Id used when scheduling an assessment.</param>
/// <param name="course">Course Details <see cref="CourseRecord"/></param>
/// <param name="primaryTeacher">Who is listed as the primary teacher? <see cref="UserRecord"/></param>
/// <param name="teachers">List of ther teachers.</param>
/// <param name="students">List of students on the roster with full details including who is their advisor.</param>
/// <param name="term">What term does this section belong to. <see cref="TermRecord"/></param>
/// <param name="room">Where does it meet. <see cref="RoomRecord"/></param>
/// <param name="block">What block does it meet in (if it is scheduled)<see cref="BlockRecord"/></param>
public readonly record struct SectionDetailRecord(string sectionId, CourseRecord course, UserRecord primaryTeacher,
    ImmutableArray<AdvisorRecord> teachers, ImmutableArray<StudentRecordShort> students,
    TermRecord term, RoomRecord? room, BlockRecord? block, bool isCurrent)
{
    public string displayName
    {
        get
        {
            var dn = course.displayName;
            if (this.block.HasValue)
                dn += $" [{block?.name}]";

            return dn;
        }
    }

    public static implicit operator SectionRecord(SectionDetailRecord sec) =>
        new(sec.sectionId, sec.course.courseId, sec.primaryTeacher.id,
            [.. sec.teachers],
            [.. sec.students],
            sec.term.termId, sec.room?.name ?? "", sec.block?.name ?? "", sec.displayName, sec.isCurrent);
}

/// <summary>
/// Condensed Section Record with only Ids listed instead of full details.
/// </summary>
/// <param name="sectionId"></param>
/// <param name="courseId"></param>
/// <param name="primaryTeacherId"></param>
/// <param name="teachers">list of teacher Ids</param>
/// <param name="students">list of student Ids</param>
/// <param name="termId"></param>
/// <param name="room">room name</param>
/// <param name="block">block name</param>
/// <param name="displayName">Section Display (as seen on your Google Calendar)</param>
public readonly record struct SectionRecord(string sectionId, string courseId, string? primaryTeacherId,
    ImmutableArray<AdvisorRecord> teachers, ImmutableArray<StudentRecordShort> students, string termId,
    string room, string block, string displayName, bool isCurrent)
{
    public static readonly SectionRecord Empty = new("", "", "", [], [], "", "", "", "", false);
}

public readonly record struct SectionMinimalRecord(string sectionId, string courseId, string? primaryTeacherId,
    ImmutableArray<string> teachers, ImmutableArray<string> students, string termId,
    string room, string block, string blockId, string displayName, string schoolLevel, bool isCurrent);

/// <summary>
/// Information about a room
/// </summary>
/// <param name="roomId">Id used in scheduling</param>
/// <param name="name">Room name</param>
/// <param name="googleCalendarId">Google Resource Calendar Id (incase you want to look at that directly.)</param>
public readonly record struct RoomRecord(string roomId, string name, string googleCalendarId);

/// <summary>
/// Block information
/// </summary>
/// <param name="blockId">Id used in scheduling</param>
/// <param name="name">Block name</param>
/// <param name="schoolLevel">Upper School blocks are different from Lower School blocks.</param>
public readonly record struct BlockRecord(string blockId, string name, string schoolLevel);


public readonly record struct DocumentHeader(string id, string fileName, string mimeType, string location);

public static partial class Extensions
{
    public static string GetUniqueNameWithin(this List<string> names, UserRecord user)
    {
        var name = $"{user}";
        if (!names.Contains(name))
        {
            return name;
        }

        if (names.Distinct().Count(u => name == $"{u}") > 1)
        {
            name = $"{user.firstName} {name}";
            names.Add(name);
        }

        if (names.Distinct().Count(u => name == $"{u}") > 1)
        {
            name = $"{user.nickname} \"{user.firstName}\" {name}";
        }

        //Debug.WriteLine($"Found unique name {name}");
        return name;
    }

    public static string GetUniqueNameWithin(this IEnumerable<UserRecord> users, UserRecord user)
    {
        Debug.WriteLine($"Looking up unique name for {user}");
        List<string> names = users.Select(u => $"{u}").ToList();
        return names.GetUniqueNameWithin(user);

    }
}

public readonly record struct StudentClassName
{
    public static readonly StudentClassName ClassI      = new("Class I");
    public static readonly StudentClassName ClassII     = new("Class II");
    public static readonly StudentClassName ClassIII    = new("Class III");
    public static readonly StudentClassName ClassIV     = new("Class IV");
    public static readonly StudentClassName ClassV      = new("Class V");
    public static readonly StudentClassName ClassVI     = new("Class VI");
    public static readonly StudentClassName ClassVII    = new("Class VII");
    public static readonly StudentClassName ClassVIII   = new("Class VIII");
    public static readonly StudentClassName None        = new("None");

    public static ReadOnlySpan<StudentClassName> AllClasses => new(
    [
        ClassI,
        ClassII,
        ClassIII,
        ClassIV,
        ClassV,
        ClassVI,
        ClassVII,
        ClassVIII
    ]);

    public static ReadOnlySpan<StudentClassName> LowerSchool => new(
    [
        ClassI,
        ClassII,
        ClassIII,
        ClassIV
    ]);

    public static ReadOnlySpan<StudentClassName> UpperSchool => new(
    [
        ClassV,
        ClassVI,
        ClassVII,
        ClassVIII
    ]);

    public static implicit operator string(StudentClassName name) => name._className;
    public static implicit operator StudentClassName(string str) => str.ToLowerInvariant() switch
    {
        "class I" => ClassI,
        "class II" => ClassII,
        "class III" => ClassIII,
        "class IV" => ClassIV,
        "class V" => ClassV,
        "class VI" => ClassVI,
        "class VII" => ClassVII,
        "class VIII" => ClassVIII,
        _ => None
    };

    private readonly string _className;

    private StudentClassName(string cn)
    {
        _className = cn;
    }
}