namespace Useit;

public interface IMySprocitTest
{
    IEnumerable<MySprocitRecord> GetMySprocitRecords(int Id);
}

public record MySprocitRecord(string Name, int Age);